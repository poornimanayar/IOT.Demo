using IOT.Demo.Umbraco.Models.CmsGenerated;
using IOT.Demo.Umbraco.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using Umbraco.Web.WebApi;

namespace IOT.Demo.Umbraco.Controllers
{
    public class AlexaCodeGardenApiController : UmbracoApiController
    {

        const string AlexaNewsApplicationId = "amzn1.ask.skill.0f15b94f-c1d7-4913-96fe-9df735259821";


        [System.Web.Http.HttpPost]
        public AlexaResponseViewModel CodeGarden(AlexaRequestViewModel request)
        {
            if (WebConfigurationManager.AppSettings["mode"] != "debug")
            {
                if (request.Session.Application.ApplicationId != AlexaNewsApplicationId)
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));
                }

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
                    response = SessionEndedRequestHandler(request);
                    break;
            }

            return response;
        }

        private AlexaResponseViewModel LaunchRequestHandler(AlexaRequestViewModel request)
        {
            var response = new AlexaResponseViewModel("Welcome to Umbraco scoop!");
            response.Response.Card.Title = "Umbraco Scoop";
            response.Response.Card.Content = "Welcome to Umbraco scoop!";
            response.Response.Reprompt.OutputSpeech.Text = "Please pick one, news, packages, meet-ups or upcoming festivals?";
            response.Response.ShouldEndSession = false;
            return response;
        }

        private AlexaResponseViewModel IntentRequestHandler(AlexaRequestViewModel request)
        {
            AlexaResponseViewModel response = null;
            
            switch (request.Request.Intent.Name)
            {
                case "CodeGardenIntent":
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

                    response = new AlexaResponseViewModel(speechResponse, speechResponse, "How many days to CodeGarden?", false);
                    break;
                case "AMAZON.CancelIntent":
                case "AMAZON.StopIntent":
                    response = CancelOrStopIntentHandler(request);
                    break;
                case "AMAZON.HelpIntent":
                    response = HelpIntentHandler(request);
                    break;
                default:
                    response = HelpIntentHandler(request);
                    break;
            }

            return response;
        }

        private AlexaResponseViewModel HelpIntentHandler(AlexaRequestViewModel request)
        {
            var response = new AlexaResponseViewModel("To use the Umbraco Scoop skill, you can say, Alexa, ask Umbraco Scoop for what's latest in Umbraco, to retrieve the upcoming festivals or say, Alexa, ask Umbraco Scoop for upcoming festivals, to retrieve the upcoming meet-ups or say, Alexa, ask Umbraco Scoop for meet-ups, to retrieve information about latest packages say, Alexa, ask Umbraco Scoop for packages. You can also say, Alexa, stop or Alexa, cancel, at any time to exit the Umbraco Scoop skill. To know about next Codegarden ask, how many days to Codegarden. For now, do you want to hear the latest scoop, packages, meet-ups or upcoming festivals?", false);
            response.Response.Reprompt.OutputSpeech.Text = "Please select one, latest scoop, packages, meet-ups or upcoming festivals?";
            return response;
        }

        private AlexaResponseViewModel CancelOrStopIntentHandler(AlexaRequestViewModel request)
        {
            return new AlexaResponseViewModel("Thanks for listening, let's talk again soon.", true);
        }

        private AlexaResponseViewModel SessionEndedRequestHandler(AlexaRequestViewModel request)
        {
            return null;
        }
    }
}