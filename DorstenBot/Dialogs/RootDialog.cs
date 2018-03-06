using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Net.Http;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;

namespace DottyBot.Dialogs
{
    [LuisModel("97e2e1f8-1191-4cc1-9271-43d71f29d447", "f02536e856a548f6a89fa27a7bafd5ec")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        /*Main Choices*/
        private const string choiceHelp = "Hilfe";
        private const string choiceMenu = "Menü";
        private const string choiceCitizenOffice = "Bürgerbüro";
        private const string choiceGreenSpaces = "Grünanlagen";
        private const string choiceDisposal = "Entsorgung";
        private IEnumerable<string> mainChoices = new List<string> { choiceHelp, choiceMenu, choiceCitizenOffice, choiceGreenSpaces, choiceDisposal };

        //https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/97e2e1f8-1191-4cc1-9271-43d71f29d447?subscription-key=f02536e856a548f6a89fa27a7bafd5ec&verbose=true&timezoneOffset=60&q=

        private const string HelpText = "Wähle zwischen den Kategorien\n\n- Bürgerbüro\n\n- Grünanlagen\n\n- Entsorgung\n\n, rufe \"Hilfe\" oder das \"Menü\" auf";


        public virtual async Task ReturnDialog(IDialogContext context, IAwaitable<object> argument)
        {
			string value = String.Empty;
            context.UserData.TryGetValue("nachfrage", out value);
            context.UserData.SetValue("entity", "");
			if (value != "ja")
            {
				await context.PostAsync("Kann ich dir noch mit etwas anderem Helfen?");
				context.UserData.SetValue("nachfrage", "");
			}
            context.Wait(MessageReceived);
        }

        [LuisIntent("Hilfe")]
        private async Task Hilfe(IDialogContext context, LuisResult result)
        {
            /*TODO: Einstellung keine Freitexteingabe*/
            await context.PostAsync(HelpText);
        }

        [LuisIntent("Menu")]
        private async Task Menu(IDialogContext context, LuisResult result)
        {
            PromptDialog.Choice(context, MenuChoiceMade, new string[] { choiceCitizenOffice, choiceGreenSpaces, choiceDisposal }, "Folgende Themen habe ich im Angebot");
        }
        private async Task MenuChoiceMade(IDialogContext context, IAwaitable<object> argument)
        {
            var choice = await argument;

            switch (choice)
            {
                case choiceCitizenOffice:
                    context.Call(new CitizenOfficeDialog(), ReturnDialog);
                    break;
                case choiceGreenSpaces:
                    context.Call(new GreenPlacesDialog(), ReturnDialog);
                    break;
                case choiceDisposal:
                    context.Call(new DisposalDialog(), ReturnDialog);
                    break;
            }
        }

        [LuisIntent("Hallo")]
        private async Task Hallo(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hallo! Wenn du wissen willst, was ich kann, versuch es mal mit \"Hilfe\"");
        }

        [LuisIntent("Wiedersehen")]
        private async Task Wiedersehen(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Okay! Ich hoffe, ich konnte helfen. Bis zum nächsten Mal!");
        }


        [LuisIntent("None")]
        private async Task None(IDialogContext context, LuisResult result)
        {
            /*TODO: Einstellung keine Freitexteingabe*/
            await context.PostAsync("Das hab ich leider nicht verstanden :(");
            await context.PostAsync(HelpText);
        }


        [LuisIntent("CitizenOffice")]
        private async Task CitizenOffice(IDialogContext context, LuisResult result)
        {
            if (result.Entities.Count > 0)
                context.UserData.SetValue("entity", result.Entities[0].Type.ToLower());
            context.Call(new CitizenOfficeDialog(), ReturnDialog);
        }


        [LuisIntent("GreenSpaces")]
        private async Task Greenspaces(IDialogContext context, LuisResult result)
        {
            if (result.Entities.Count > 0 )
                context.UserData.SetValue("entity", result.Entities[0].Type.ToLower());
            context.Call(new GreenPlacesDialog(), ReturnDialog);
        }

        [LuisIntent("Disposal")]
        private async Task Disposal(IDialogContext context, LuisResult result)
        {
            if (result.Entities.Count > 0)
                context.UserData.SetValue("entity", result.Entities[0].Type.ToLower());
            context.Call(new DisposalDialog(), ReturnDialog);
        }
    }
}