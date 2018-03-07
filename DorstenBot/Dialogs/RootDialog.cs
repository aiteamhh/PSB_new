using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace DorstenBot2
{
    [Serializable]
    public class RootDialog : IDialog<string>
    {
        bool first = true;
        bool wrong = false;
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Willkommen. Wie kann ich behilflich sein?");
            context.Wait(Step);
        }

        private async Task Step(IDialogContext context, IAwaitable<object> result)
        {
            if (first)
            {
                context.Wait(QuestionEntered);
                first = false;
            }
            else if (!first && !wrong)
            {
                await context.PostAsync("Wie darf ich noch weiterhelfen?");
            }
            if (wrong)
            {
                context.Wait(QuestionEntered);
                wrong = false;
            }
        }


        public async Task QuestionEntered(IDialogContext context, IAwaitable<object> argument)
        {
            var activity = await argument as Activity;


            // TODO call QnA
            var answer = GetAnswerToQuestion(activity.Text);

            if (answer[0].Answer.Contains("DoSomething"))
            {
                string buffer = answer[0].Answer;
                buffer = buffer.Replace("DoSomething", "");

                int last = 0;
                int n = 0;
                char[] a = answer[0].Answer.ToCharArray();
                foreach (char c in a)
                {
                    if (c == ':')
                    {
                        last = n;
                    }
                    n++;
                }

                if (buffer.Contains("DoMap"))
                {
                    Regex r = new Regex(":");

                    string[] parts = r.Split(buffer);

                    var message = context.MakeMessage();
                    var heroCard = new ThumbnailCard
                    {
                        Title = "Das Bürgerbüro finden Sie hier:",
                        Subtitle = parts[parts.Length - 1],
                        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Google Maps", value: "https://www.google.de/maps/place/Halterner+Str.+5,+46284+Dorsten/@51.6697362,6.9671216,17z/data=!3m1!4b1!4m5!3m4!1s0x47b8f18a59c0bd4b:0xd81648b42bd0495f!8m2!3d51.6697362!4d6.9693156") }
                    };
                    var attachment = heroCard.ToAttachment();
                    message.Attachments.Add(attachment);
                    await context.PostAsync(message);

                }

                PromptDialog.Confirm(context, QustionAnswered, "Ist Ihre Frage damit beantwortet?", options: new string[] { "Ja", "Nein" }, patterns: new string[][] { new string[] { "Yes", "ja", "Ja" }, new string[] { "No", "nein", "Nein" } });


            }
            else
            {
                if (answer[0].Score > 0.5)
                {
                    await context.PostAsync($"{answer[0].Answer}");

                    PromptDialog.Confirm(context, QustionAnswered, "Ist Ihre Frage damit beantwortet?", options: new string[] { "Ja", "Nein" }, patterns: new string[][] { new string[] { "Yes", "ja", "Ja" }, new string[] { "No", "nein", "Nein" } });
                }
                else
                {
                    wrong = true;
                    await context.PostAsync("Tut mir leid, das habe ich nicht richtig verstanden. Können Sie die Frage bitte anders formulieren?");
                    Step(context, null);
                }
            }


        }

        public virtual async Task QustionAnswered(IDialogContext context, IAwaitable<bool> argument)
        {
            var result = await argument;
            if (result)
                Step(context, null);
            else
            {
                await context.PostAsync("Können Sie die Frage bitte anders formulieren?");
                Step(context, null);
            }
        }

        public virtual async Task RepeatQuestion(IDialogContext context, IAwaitable<bool> argument)
        {
            var result = await argument;
            if (!result)
            {
                await context.PostAsync("Alles klar.");
                context.Done("return");
            }
            else
                PromptDialog.Text(context, QuestionEntered, "Welche Frage darf ich ich noch beantworten?");
        }

        public struct QnAObject
        {
            public string Answer;
            public float Score;
        }

        public static List<QnAObject> GetAnswerToQuestion(string question)
        {

            var allAnswers = new List<JToken>();

            string uriString = $"https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/f99df37a-bfb2-4b7a-b88b-4ccb28ec7ab0/generateAnswer";
            string subscriptionKey = "c3661bc9434f4ee9ac29af3b5d78993f";

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
            for (var i = 0; i < allAnswers.Count; i++)
            {
                answers.Add(new QnAObject()
                {
                    Answer = allAnswers[i].SelectToken("answer").ToString(),
                    Score = float.Parse(allAnswers[i].SelectToken("score").ToString())
                });
            }

            return answers;
        }




    }
}