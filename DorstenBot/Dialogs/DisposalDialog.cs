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
    public class DisposalDialog : IDialog<string>
    {

        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Text(context, QuestionEntered, "Du hat also eine Frage zum Thema Entsorgung. Was willst du wissen?");
        }

        public virtual async Task QuestionEntered(IDialogContext context, IAwaitable<string> argument)
        {
            var result = await argument;
            context.UserData.SetValue("LastQuestion", result);

            // TODO call QnA
            var answer = Helper.GetAnswerToQuestion(result, Helper.QnAType.Disposal);
            await context.PostAsync($"{answer[0].Answer} ({answer[0].Score})");

            PromptDialog.Confirm(context, QustionAnswered, "Ist deine Frage damit beantwortet?", options: new string[] { "Ja", "Nein" });
        }

        public virtual async Task QustionAnswered(IDialogContext context, IAwaitable<bool> argument)
        {
            var result = await argument;
            if (result)
                PromptDialog.Confirm(context, RepeatQuestion, "Brauchst du noch etwas zum Thema Entsorgung?", options: new string[] { "Ja", "Nein" });
            else
                PromptDialog.Text(context, QuestionEntered, "Kannst du die Frage anders formulieren?");
        }

        public virtual async Task RepeatQuestion(IDialogContext context, IAwaitable<bool> argument)
        {
            var result = await argument;
            if (result)
            {
                await context.PostAsync("Alles klar :)");
                context.Done("return");
            }
            else
                PromptDialog.Text(context, QuestionEntered, "Was willst du noch zum Thema Entsorgung wissen?");
        }

    }
}
