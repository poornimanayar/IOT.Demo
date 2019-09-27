using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using IOT.Demo.Umbraco.Models.CmsGenerated;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.WebApi;

namespace IOT.Demo.Umbraco.Controllers
{
    public class GoogleActionNewsApiController : UmbracoApiController
    {
        private static readonly JsonParser jsonParser =
            new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        [System.Web.Http.HttpPost]
        public HttpResponseMessage GetBlogs([FromBody] JObject rawRequest)
        {
            try
            {
                WebhookRequest request;
                using (var reader = new StringReader(rawRequest.ToString()))
                {
                    request = jsonParser.Parse<WebhookRequest>(reader);
                }
                var responseJson = new WebhookResponse { FulfillmentText = "Hey, I did not get you. What do you want to know about?" };

                if (request.QueryResult.Intent.DisplayName == "NewsIntent")
                {
                    var articleList = Umbraco.ContentSingleAtXPath("//home/articleList").Children().OfType<Article>();
                    var outputText = new StringBuilder();
                    Logger.Info(typeof(GoogleActionNewsApiController), "entered intent");

                    var fulfillmentCarousel = new Intent.Types.Message.Types.CarouselSelect();
                    var fulfillmentMessages = new Intent.Types.Message();
                    fulfillmentMessages.CarouselSelect = fulfillmentCarousel;

                    foreach (var article in articleList)
                    {
                        //this can be made better :D
                        outputText.Append(string.Concat(article.IotTitle, " - ", article.TextContent, " "));
                        fulfillmentCarousel.Items.Add(new Intent.Types.Message.Types.CarouselSelect.Types.Item
                        {
                            Title = article.IotTitle,
                            Description = article.TextContent,
                            Info = new Intent.Types.Message.Types.SelectItemInfo { Key = article.Key.ToString() },
                            Image = new Intent.Types.Message.Types.Image { ImageUri = "https://iot-demo.kvtechsltd.co.uk/media/xldnfcwl/friendly-chair.jpg", AccessibilityText = "Friendly Umbraco Chair" }
                        }
                        );
                    }
                    fulfillmentMessages.CarouselSelect = fulfillmentCarousel;
                    responseJson = new WebhookResponse
                    {
                        FulfillmentMessages = { fulfillmentMessages },
                        FulfillmentText = "Hey, here is the latest Meetup News : " + outputText.ToString()

                    };
                    Logger.Info(typeof(GoogleActionNewsApiController), outputText.ToString());
                }

                HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    // Ask Protobuf to format the JSON to return.
                    // Again, we don't want to use Json.NET - it doesn't know how to handle Struct
                    // values etc.
                    Content = new StringContent(responseJson.ToString())
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue("text/json") }
                    }
                };
                return httpResponse;
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(GoogleActionNewsApiController), ex.ToString());
                var responseJson = new WebhookResponse { FulfillmentText = "Hey, Error occurred" };
                return Request.CreateResponse(HttpStatusCode.OK, responseJson.ToString());
            }
        }
    }
}