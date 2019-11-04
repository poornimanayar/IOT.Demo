using IOT.Demo.Umbraco.Models.CmsGenerated;
using IOT.Demo.Umbraco.Models.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace IOT.Demo.Umbraco.Controllers
{
    public class AlexaApiController : UmbracoApiController
    {

        private string AlexaUmbracoFestivalsApplicationId = WebConfigurationManager.AppSettings["Alexa.ApplicationId"];

        private string fallbackMedia = "https://iot-demo.kvtechsltd.co.uk/media/cxvl1tw3/codegarden.jpg";

        private string skillName = "Friendly Events!";


        [System.Web.Http.HttpPost]
        public AlexaResponseViewModel FriendlyEvents(AlexaRequestViewModel request)
        {
            if (WebConfigurationManager.AppSettings["mode"] != "debug")
            {
                //verify the application id
                if (request.Session.Application.ApplicationId != AlexaUmbracoFestivalsApplicationId)
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

                //service should only process requests in which the timestamp is within 150 seconds of the current time. 
                //If the timestamp differs from the current time by more than 150 seconds, discard the request. 
                var totalSeconds = (System.DateTime.UtcNow - request.Request.Timestamp).TotalSeconds;
                if (totalSeconds < 0 || totalSeconds > 150)
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }
            }

            AlexaResponseViewModel response = null;

            switch (request.Request.Type)
            {
                case "LaunchRequest":
                    response = LaunchRequestHandler(request);
                    break;
                case "IntentRequest":
                    response = IntentRequestHandler(request);
                    break;
                case "SessionEndedRequest":
                    response = SessionEndedRequestHandler();
                    break;
            }

            return response;
        }

        private AlexaResponseViewModel LaunchRequestHandler(AlexaRequestViewModel request)
        {
            //new AlexaResponseViewModel(string outputSpeech, string cardTitle, string cardContent, string mediUrl, bool shouldEndSession)
            return new AlexaResponseViewModel("Welcome to Friendly Events!", "Welcome to Friendly Events!",
                "Friendly Events", fallbackMedia, false);
        }

        private AlexaResponseViewModel IntentRequestHandler(AlexaRequestViewModel request)
        {
            AlexaResponseViewModel response = null;
            Logger.Info(typeof(AlexaApiController), request.Request.Intent.Name);
            switch (request.Request.Intent.Name)
            {
                case "CodegardenIntent":
                    response = CodeGardenHandler();
                    break;
                case "FestivalIntent":
                    response = FestivalHandler(request);
                    break;
                case "FestivalTalkIntent":
                    response = FestivalTalkHandler(request);
                    break;
                case "AMAZON.CancelIntent": //handling built-in intents
                case "AMAZON.StopIntent":
                    response = CancelOrStopIntentHandler();
                    break;
                case "AMAZON.HelpIntent":
                    response = HelpIntentHandler();
                    break;
                case "AMAZON.FallbackIntent":
                    response = FallbackIntentHandler();
                    break;
                default:
                    response = FallbackIntentHandler();
                    break;
            }

            return response;
        }

        private AlexaResponseViewModel HelpIntentHandler()
        {
            Logger.Info(typeof(AlexaApiController), "entered help intent");

            var outputSpeech = "To use the Friendly Events skill, you can say, Alexa, ask Friendly Events how many days to Codegarden? " +
                "If you wish to know when a festival is say Alexa, Ask Friendly events, when is the UK Fest. " +
                "If you wish to know more about talks sat Alexa, Ask friendly events what talk is on at 11am at the UK Fest.";

            //new AlexaResponseViewModel(string outputSpeech, string cardTitle, string cardContent, string mediaUrl, bool shouldEndSession)
            return new AlexaResponseViewModel(outputSpeech, skillName, outputSpeech, fallbackMedia, false);
        }

        private AlexaResponseViewModel FallbackIntentHandler()
        {
            Logger.Info(typeof(AlexaApiController), "entered fallback intent");

            var outputSpeech = "Sorry, I didnt understand that. To use the Friendly Events skill, you can say, Alexa, ask Friendly Events how many days to Codegarden?" +
                " If you wish to know when a festival is say Alexa, Ask Friendly events, when is the UK Fest. " +
                "If you wish to know more about talks sat Alexa, Ask friendly events what talk is on at 11am at the UK Fest.";

            //new AlexaResponseViewModel(string outputSpeech, string cardTitle, string cardContent, string mediaUrl, bool shouldEndSession)
            var response = new AlexaResponseViewModel(outputSpeech, skillName, outputSpeech, fallbackMedia, false);
            return response;
        }

        private AlexaResponseViewModel CancelOrStopIntentHandler()
        {
            Logger.Info(typeof(AlexaApiController), "entered cancel/stop intent");

            var outputSpeech = "Thanks for listening, let's talk again soon.";

            //new AlexaResponseViewModel(string outputSpeech, string cardTitle, string cardContent, string mediaUrl, bool shouldEndSession)
            return new AlexaResponseViewModel(outputSpeech, skillName, outputSpeech, fallbackMedia, true);
        }

        private AlexaResponseViewModel SessionEndedRequestHandler()
        {
            return null;
        }

        private AlexaResponseViewModel CodeGardenHandler()
        {
            Logger.Info(typeof(AlexaApiController), "entered codegarden intent");

            var codegardenNode = Umbraco.ContentSingleAtXPath("//home/codeGarden") as CodeGarden;
            var prompt = codegardenNode.Response;
            var codegardenDate = codegardenNode.Date;
            var speechResponse = "Codegarden is now on!";
            if (codegardenDate > DateTime.UtcNow)
            {
                var numberOfDays = codegardenDate.Subtract(DateTime.UtcNow).Days;
                var daysText = numberOfDays > 1 ? "days" : "day";
                speechResponse = prompt.Replace("{date}", codegardenDate.ToLongDateString()).Replace("{number}", numberOfDays.ToString()).Replace("{days}", daysText);

            }

            //new AlexaResponseViewModel(string outputSpeech, string cardTitle, string cardContent, string mediaUrl, bool shouldEndSession)
            return new AlexaResponseViewModel(speechResponse, skillName, speechResponse, fallbackMedia, false);
        }

        private AlexaResponseViewModel FestivalHandler(AlexaRequestViewModel request)
        {
            Logger.Info(typeof(AlexaApiController), "entered festival intent");

            //a request will not be made by Alexa till the slot is fulfilled as it is a required slot. So you can get away with a  null check.
            //But it is always recommended.
            var festival = request.Request.Intent.GetSlots().FirstOrDefault(s => s.Key == "festival");
            string speechResponse;

            //searching for the festival node, finding a match for the incoming slot value in the conference synonyms tags
            var festivalNode = Umbraco.ContentAtXPath("//home/festival").OfType<Festival>()
                            .FirstOrDefault(f => f.ConferenceNameSynonyms.Contains(festival.Value, StringComparer.InvariantCultureIgnoreCase));
            if (festivalNode == null)
            {
                speechResponse = $"Sorry, I could not find any information for {festival.Value}";

                //new AlexaResponseViewModel(string outputSpeech, string cardTitle, string cardContent, string mediaUrl, bool shouldEndSession)
                return new AlexaResponseViewModel(speechResponse, skillName, speechResponse, fallbackMedia, false);
            }

            speechResponse = $"{festival.Value} is on {festivalNode.ConferenceStartDate.ToString("dd MMMM yyyy")}";
            if (festivalNode.ConferenceStartDate != festivalNode.ConferenceEndDate)
            {
                speechResponse = $"{festival.Value} starts on {festivalNode.ConferenceStartDate.ToString("dd MMMM yyyy")} and " +
                    $"finishes on {festivalNode.ConferenceEndDate.ToString("dd MMMM yyyy")}";
            }

            var imageUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Host + festivalNode.IOtcardImage.Url;

            //new AlexaResponseViewModel(string outputSpeech, string cardTitle, string cardContent, string mediaUrl, bool shouldEndSession)
            return new AlexaResponseViewModel(speechResponse, skillName, speechResponse, imageUrl, false);

        }

        private AlexaResponseViewModel FestivalTalkHandler(AlexaRequestViewModel request)
        {
            Logger.Info(typeof(AlexaApiController), "entered festival talk intent");

            //a request will not be made by Alexa till the slot is fulfilled as it is a required slot. So you can get away with a  null check.
            //But it is always recommended.
            var festival = request.Request.Intent.GetSlots().FirstOrDefault(s => s.Key == "festival");
            var time = request.Request.Intent.GetSlots().FirstOrDefault(s => s.Key == "time");

            Logger.Info(typeof(AlexaApiController), $"Festival - {festival} - Time - {time}");

            //searching for the festival node, finding a match for the incoming slot value in the conference synonyms tags
            var festivalNode = Umbraco.ContentAtXPath("//home/festival").OfType<Festival>().
                FirstOrDefault(f => f.ConferenceNameSynonyms.Contains(festival.Value, StringComparer.InvariantCultureIgnoreCase));
           
            Logger.Info(typeof(AlexaApiController), festivalNode.Key.ToString());

            var speechResponse = new System.Text.StringBuilder();

            if (festivalNode == null)
            {
                speechResponse.Append($"Sorry, I could not find any information for {festival.Value}");

                //new AlexaResponseViewModel(string outputSpeech, string cardTitle, string cardContent, string mediaUrl, bool shouldEndSession)
                return new AlexaResponseViewModel(speechResponse.ToString(), skillName, speechResponse.ToString(), fallbackMedia, false);
            }
            //find the correct time slots based on the incoming slot time. 
            //Those nested content items where the incoming slot time falls between the time from and time to are chosen
            var timeSlots = festivalNode.Schedule.OfType<ScheduleItem>()
                .Where(s => DateTime.ParseExact(time.Value, "HH:mm", CultureInfo.InvariantCulture) >= DateTime.ParseExact(s.TimeFrom, "HH:mm", CultureInfo.InvariantCulture) && DateTime.ParseExact(time.Value, "HH:mm", CultureInfo.InvariantCulture) <= DateTime.ParseExact(s.TimeTo, "HH:mm", CultureInfo.InvariantCulture));

            if (timeSlots == null || !timeSlots.Any())
            {
                speechResponse.Append("Sorry, I could not find any talks at that time");

                //new AlexaResponseViewModel(string outputSpeech, string cardTitle, string cardContent, string mediaUrl, bool shouldEndSession)
                return new AlexaResponseViewModel(speechResponse.ToString(), skillName, speechResponse.ToString(), fallbackMedia, false);
            }

           
            int talkCount = 0;

            //looping through timeslots as there could be overlapping timeslots
            foreach (var timeSlot in timeSlots)
            {
                var talkDetails = timeSlot.Talks.OfType<TalkDetails>();
                //looping through the talks for each timeslot
                foreach (var talkDetail in talkDetails)
                {
                    talkCount = talkCount + 1;
                    speechResponse.Append($"{talkDetail.Talk} by {talkDetail.Speaker} at the {talkDetail.RoomName}. ");
                }
            }

            speechResponse.Insert(0, $"There are {talkCount} {(talkCount > 1 ? "talks" : "talk")} at {time.Value}. ");

            Logger.Info(typeof(AlexaApiController), speechResponse.ToString());

            var imageUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Host + festivalNode.IOtcardImage.Url;

            //new AlexaResponseViewModel(string outputSpeech, string cardTitle, string cardContent, string mediaUrl, bool shouldEndSession)
            return new AlexaResponseViewModel(speechResponse.ToString(), skillName, speechResponse.ToString(), imageUrl, false);
        }
    }
}