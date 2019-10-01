using System;
using Newtonsoft.Json;

namespace IOT.Demo.Umbraco.Models.ViewModels
{
    [JsonObject]
    public class AlexaResponseViewModel
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("sessionAttributes")]
        public SessionAttributes Session { get; set; }

        [JsonProperty("response")]
        public ResponseAttributes Response { get; set; }

        public AlexaResponseViewModel()
        {
            Version = "1.0";
            Session = new SessionAttributes();
            Response = new ResponseAttributes();
        }

        public AlexaResponseViewModel(string outputSpeechText)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;
            Response.Card.Text = outputSpeechText;
        }

        public AlexaResponseViewModel(string outputSpeechText, bool shouldEndSession)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;
            Response.ShouldEndSession = shouldEndSession;

            if (shouldEndSession)
            {
                Response.Card.Text = outputSpeechText;
            }
            else
            {
                Response.Card = null;
            }
        }

        public AlexaResponseViewModel(string outputSpeechText, string cardContent, bool shouldEndSession)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;
            Response.Card.Text = cardContent;
            Response.ShouldEndSession = shouldEndSession;

            if (shouldEndSession)
            {
                Response.Card.Text = outputSpeechText;
            }
            else
            {
                Response.Card = new ResponseAttributes.CardAttributes { Text = cardContent };
            }
        }

        public AlexaResponseViewModel(string outputSpeechText, string cardContent, string cardTitle, string imageUrl, bool shouldEndSession)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;
            Response.Card.Text = cardContent;
            Response.Card.Title = cardTitle;
            Response.Card.Image = new AlexaResponseViewModel.ResponseAttributes.ImageAttributes
            {
                SmallImageUrl = imageUrl,
                LargeImageUrl = imageUrl
            };
            Response.ShouldEndSession = shouldEndSession;

            if (shouldEndSession)
            {
                Response.Card.Text = outputSpeechText;
            }
            else
            {
                Response.Card = new ResponseAttributes.CardAttributes { Title = cardTitle, Text = cardContent, Image= new ResponseAttributes.ImageAttributes { SmallImageUrl = imageUrl, LargeImageUrl = imageUrl } };
            }
        }

        [JsonObject("sessionAttributes")]
        public class SessionAttributes
        {
            [JsonProperty("memberId")]
            public int MemberId { get; set; }
        }

        [JsonObject("response")]
        public class ResponseAttributes
        {
            [JsonProperty("shouldEndSession")]
            public bool ShouldEndSession { get; set; }

            [JsonProperty("outputSpeech")]
            public OutputSpeechAttributes OutputSpeech { get; set; }

            [JsonProperty("card")]
            public CardAttributes Card { get; set; }

            [JsonProperty("reprompt")]
            public RepromptAttributes Reprompt { get; set; }

            public ResponseAttributes()
            {
                ShouldEndSession = true;
                OutputSpeech = new OutputSpeechAttributes();
                Card = new CardAttributes();
                Reprompt = new RepromptAttributes();
            }

            [JsonObject("outputSpeech")]
            public class OutputSpeechAttributes
            {
                [JsonProperty("type")]
                public string Type { get; set; }

                [JsonProperty("text")]
                public string Text { get; set; }

                [JsonProperty("ssml")]
                public string Ssml { get; set; }

                public OutputSpeechAttributes()
                {
                    Type = "PlainText";
                }
            }

            [JsonObject("card")]
            public class CardAttributes
            {
                [JsonProperty("type")]
                public string Type { get; set; }

                [JsonProperty("title")]
                public string Title { get; set; }

                [JsonProperty("text")]
                public string Text { get; set; }

                [JsonProperty("image")]
                public ImageAttributes Image { get; set; }

                public CardAttributes()
                {
                    Type = "Standard";
                }
            }

            [JsonObject("image")]
            public class ImageAttributes
            {
                [JsonProperty("smallImageUrl")]
                public string SmallImageUrl { get; set; }

                [JsonProperty("largeImageUrl")]
                public string LargeImageUrl { get; set; }
            }


            [JsonObject("reprompt")]
            public class RepromptAttributes
            {
                [JsonProperty("outputSpeech")]
                public OutputSpeechAttributes OutputSpeech { get; set; }

                public RepromptAttributes()
                {
                    OutputSpeech = new OutputSpeechAttributes();
                }
            }
        }


    }
}