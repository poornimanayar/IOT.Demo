using IOT.Demo.Umbraco.Models.CmsGenerated;
using IOT.Demo.Umbraco.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.WebApi;

namespace IOT.Demo.Umbraco.Controllers
{
    public class AlexaFlashBriefingApiController : UmbracoApiController
    {

        public HttpResponseMessage GetFlashBriefing()
        {
            var articleList = Umbraco.ContentSingleAtXPath("//home/articleList").Children().OfType<Article>();
            var alexaResponse = new List<AlexaFlashBriefingViewModel>();

            alexaResponse.AddRange(articleList.Where(article => !article.ExcludeFromFlashBriefing).
                OrderByDescending(article => article.Date)
                .Take(5)
                .Select(article => new AlexaFlashBriefingViewModel
                {
                    UId = string.Concat("urn:uuid:", article.Key),
                    UpdateDate = article.Date.ToString("yyyy-MM-dd'T'HH:mm:ss'.0Z'"),
                    Title = article.IotTitle,
                    Description = article.TextContent,
                    Url = string.Concat(Request.RequestUri.Scheme, "://", Request.RequestUri.Host, article.Url)
                }));

            string output = JsonConvert.SerializeObject(alexaResponse);
            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.Write(output);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}