using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IOT.Demo.Umbraco.Models.ViewModels
{
    public class AlexaFlashBriefingViewModel
    {
        [JsonProperty("uid")]
        public string UId { get; set; }

        [JsonProperty("updateDate")]
        public string UpdateDate { get; set; }

        [JsonProperty("titleText")]
        public string Title { get; set; }

        [JsonProperty("mainText")]
        public string Description { get; set; }

        [JsonProperty("streamUrl")]
        public string AudioContent { get; set; }
        
        [JsonProperty("redirectionUrl")]
        public string Url { get; set; }
    }
}