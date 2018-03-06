#region
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Net.Http;
#endregion

namespace DottyBot.Dialogs
{
    [Serializable]
    public class CitizenOfficeDialog : IDialog<string>
    {
        /*Choices*/
        #region
        private const string choiceDocument = "Dokument beantragen";
        private const string choiceQuestion = "Ich habe eine Frage";
        private const string choiceBack = "Ich möchte etwas anderes";
        private IEnumerable<string> choices1 = new List<string> { choiceDocument, choiceQuestion, choiceBack };
        #endregion

        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, InformationTypeRetrieved, choices1, "Du hast also eine Frage zum Bürgerbüro. Was brauchst du?");
        }


        public virtual async Task InformationTypeRetrieved(IDialogContext context, IAwaitable<string> argument)
        {
            var result = await argument;

            switch (result) {
                case choiceDocument:
                    PromptDialog.Text(context, NameEntered, "Dazu brauche ich deinen vollen Namen!");
                    break;
                case choiceQuestion:
                    PromptDialog.Text(context, QuestionEntered, "Was willst du wissen?");
                    break;
                case choiceBack:
                    await context.PostAsync("Alles klar");
                    context.Done("return");
                    break;
            }
        }

        #region Dokument
        public virtual async Task NameEntered(IDialogContext context, IAwaitable<string> argument)
        {
            var result = await argument;
            context.UserData.SetValue("Name", result);
            PromptDialog.Text(context, BirthdayEntered, "Wie lautet dein Geburtsdatum?");
        }

        public virtual async Task BirthdayEntered(IDialogContext context, IAwaitable<string> argument)
        {
            var result = await argument;
            context.UserData.SetValue("Birthday", result);
            context.UserData.TryGetValue("Name", out string name);
            await context.PostAsync($"Danke!\n\nDeine Daten lauten:\n\n{name}\n\nGeburtsdatum: {result}");
            await context.PostAsync("Du kannst das Dokument in 14 Tagen im Bürgerbüro abholen. :)");

            PromptDialog.Choice(context, InformationTypeRetrieved, choices1, "Brauchst du noch etwas vom Bürgerbüro?");
        }
        #endregion

        #region Question
        public virtual async Task QuestionEntered(IDialogContext context, IAwaitable<string> argument)
        {
            var result = await argument;
            context.UserData.SetValue("LastQuestion", result);

            // TODO call QnA
            var answer = Helper.GetAnswerToQuestion(result, Helper.QnAType.CitizenOffice);
            await context.PostAsync($"{answer[0].Answer} ({answer[0].Score})");

            PromptDialog.Confirm(context, QustionAnswered, "Ist deine Frage damit beantwortet?", options: new string[] { "Ja", "Nein" });
        }

        public virtual async Task QustionAnswered(IDialogContext context, IAwaitable<bool> argument)
        {
            var result = await argument;
            if (result)
                PromptDialog.Choice(context, InformationTypeRetrieved, choices1, "Brauchst du noch etwas vom Bürgerbüro?");
            else
                PromptDialog.Text(context, QuestionEntered, "Kannst du die Frage anders formulieren?");
        }
        #endregion
    }
}
