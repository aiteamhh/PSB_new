using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace DottyBot.Dialogs
{
    public static class Helper
    {
        //public static string[][] ConfirmPatterns = new string[][]{new string[] {"Ja", "ja danke"}, new string[]{"nein", "nee", "nein danke"} };
        public struct Entity
        {
            public string Type;
            public string Child;
        }
        public struct LuisObject {
            public string Intent;
            public List<Entity> Entities;
        }
        public struct QnAObject {
            public string Answer;
            public float Score;
        }

        public static string ServerAddress = "";

        #region LUIS
        public async static Task<LuisObject> GetIntentAndEntities(string message)
        {
            //call Luis with result text
            JObject jobj;
            using (HttpClient httpClient = new HttpClient())
            {
                var url = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/19eec619-57f0-492f-845d-ed2d74021e6b?subscription-key=&timezoneOffset=60&verbose=true&q=";
                var responseString = await httpClient.GetStringAsync(url + message);
                jobj = JObject.Parse(responseString);
            }

            var topScoringIntent = jobj.SelectToken("topScoringIntent");
            var intent = topScoringIntent.SelectToken("intent").ToString();
            var score = topScoringIntent.SelectToken("score").ToString();

            var entities = (JArray)jobj.SelectToken("entities");
            var entityList = new List<Entity>();
            for (var i = 0; i < entities.Count; i++)
            {
                entityList.Add(new Entity()
                {
                    Type = entities[i].SelectToken("type").ToString(),
                    Child = entities[i].SelectToken("entity").ToString()
                });
            }

            return new LuisObject() { Intent = intent, Entities = entityList };
        }
        #endregion

        #region QnA
        public enum QnAType { CitizenOffice, GreenPlaces, Disposal };
        public static List<QnAObject> GetAnswerToQuestion(string question, QnAType type)
        {
            string kb;
            switch (type)
            {
                case QnAType.CitizenOffice:
                    kb = "";
                    break;
                case QnAType.GreenPlaces:
                    kb = "";
                    break;
                case QnAType.Disposal:
                    kb = "";
                    break;
                default:
                    throw new Exception("Wrong QnA Knowledgebase type!");
            }

            var allAnswers = new List<JToken>();

            string uriString = $"https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/{kb}/generateAnswer";
            string subscriptionKey = "";

            using (WebClient webClient = new WebClient())
            {
                var top = 1;
                var uri = new Uri(uriString);
                var body = $"{{ \"question\": \"{question}\", \"top\": \"{top}\" }}";

                webClient.Encoding = System.Text.Encoding.UTF8;
                webClient.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                webClient.Headers.Add("Content-Type", "application/json");
                var responseString = webClient.UploadString(uri, body);

                JObject jobj = JObject.Parse(responseString);
                allAnswers = jobj.SelectToken("answers").ToList();
            }

            var answers = new List<QnAObject>();
            for (var i = 0; i < allAnswers.Count; i++) {
                answers.Add(new QnAObject()
                {
                    Answer = allAnswers[i].SelectToken("answer").ToString(),
                    Score = float.Parse(allAnswers[i].SelectToken("score").ToString())
                });
            }
           
            return answers;
        }
        #endregion


        #region Cards
        public static Attachment GetCardAttachment(int index)
        {
            HeroCard herocard = null;
          
            herocard = new HeroCard
            {
                Title = "Aktien",
                Images = new List<CardImage> { new CardImage(url: ServerAddress+"content/imgs/aktien.jpg", alt: "aktienImage") },
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.ImBack, "Muster AG", value: "Muster AG")
                }
            };
             
            return herocard.ToAttachment();
        }

        public static Attachment GetStudieCard(int index)
        {
            var thumbnailcard = new ThumbnailCard();
           
            thumbnailcard = new ThumbnailCard()
            {
                Title = "Studie zu erneuerbaren Energien",
                Subtitle = "EINE KURZANALYSE VON KIRCHHOFF CONSULT UND VALUATION METRICS",
                Text = "JUNI 2016",
                Images = new List<CardImage> { new CardImage(ServerAddress + "content/imgs/pdficon.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Studie zu erneuerbaren Energien", value: Helper.ServerAddress + "content/docs/STUDIE_Erneuerbare_Energien.pdf") }
            };
                              
            return thumbnailcard.ToAttachment();
        }

        public static Attachment GetContactPersonCard(int index)
        {
            var txtUrl = ServerAddress+"content/asp/person" + index + ".txt";
            var path = System.Web.HttpContext.Current.Server.MapPath("/content/asp/");
            var pathToFile = path + "person" + index + ".txt";

            StreamReader sr = new StreamReader(pathToFile);
            var title = sr.ReadLine();
            var subTitle = sr.ReadLine();
            var tel = sr.ReadLine();
            var email = sr.ReadLine();
            String mailto = "mailto:" + email;
            var text = sr.ReadLine();
            var img = sr.ReadLine();
            sr.Close();
            var herocard = new ThumbnailCard
            {
                Title = title,
                Subtitle = subTitle,
                Text = "**Telefon:** " +tel + "\n\n" + "**Email:** ["+title+"]("+mailto+")" + "\n\n" + "**Tätigkeitsbereich:** " +text,
                Images = new List<CardImage> { new CardImage(url: img, alt: "Image") }
            };

            return herocard.ToAttachment();
        }
        #endregion
    }
}