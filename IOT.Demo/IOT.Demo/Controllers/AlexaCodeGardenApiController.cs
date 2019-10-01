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

        private string AlexaUmbracoFestivalsApplicationId = WebConfigurationManager.AppSettings["Alexa.ApplicationId"];


        [System.Web.Http.HttpPost]
        public AlexaResponseViewModel CodeGarden(AlexaRequestViewModel request)
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
                    response = SessionEndedRequestHandler(request);
                    break;
            }

            return response;
        }

        private AlexaResponseViewModel LaunchRequestHandler(AlexaRequestViewModel request)
        {
            return new AlexaResponseViewModel("Welcome to Friendly Events!", "Welcome to Friendly Events!", "Friendly Events", "https://iot-demo.kvtechsltd.co.uk/media/cxvl1tw3/codegarden.jpg", false);
        }

        private AlexaResponseViewModel IntentRequestHandler(AlexaRequestViewModel request)
        {
            AlexaResponseViewModel response = null;
            
            switch (request.Request.Intent.Name)
            {
                case "CodegardenIntent":
                    Logger.Info(typeof(AlexaCodeGardenApiController), "entered intent");
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
                    response = new AlexaResponseViewModel(speechResponse, speechResponse, "Friendly Events", "https://iot-demo.kvtechsltd.co.uk/media/cxvl1tw3/codegarden.jpg", false);
                    break;
                case "AMAZON.CancelIntent":
                case "AMAZON.StopIntent":
                    response = CancelOrStopIntentHandler(request);
                    break;
                case "AMAZON.HelpIntent":
                    response = HelpIntentHandler(request);
                    break;
                case "AMAZON.FallbackIntent":
                    response = FallbackIntentHandler(request);
                    break;
                default:
                    response = FallbackIntentHandler(request);
                    break;
            }

            return response;
        }

        private AlexaResponseViewModel HelpIntentHandler(AlexaRequestViewModel request)
        {
            var response = new AlexaResponseViewModel("To use the Friendly Events skill, you can say, Alexa, ask Friendly Events how many days to Codegarden?", false);
            response.Response.Reprompt.OutputSpeech.Text = "To use the Friendly Events skill, you can say, Alexa, ask Friendly Events how many days to Codegarden?";
            return response;
        }

        private AlexaResponseViewModel FallbackIntentHandler(AlexaRequestViewModel request)
        {
            var response = new AlexaResponseViewModel("Sorry, I didnt understand that. To use this skill say, Alexa, ask Friendly Events how many days to codegarden?", false);
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