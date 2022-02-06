using RetailBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace RetailBot.Dialogs
{
    public class CancelDialog : ComponentDialog
    {
        #region Variables
        private readonly BotServices _botServices;
        #endregion

        public CancelDialog(string dialogId, BotServices botServices) : base(dialogId)
        {
            _botServices = botServices;
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var recognizerResult = await _botServices.LuisGeneral.RecognizeAsync(innerDc.Context, cancellationToken);
                var topIntent = recognizerResult.GetTopScoringIntent();

                if (topIntent.intent == Constans.Intents.Cancel && topIntent.score >= 0.85)
                {
                    await innerDc.Context.SendActivityAsync(MessageFactory.Text("Action cancelled. "), cancellationToken);
                    await innerDc.Context.SendActivityAsync(MessageFactory.Text("What else can I do for you ? "), cancellationToken);

                    return await innerDc.CancelAllDialogsAsync(cancellationToken);
                }
            }

            return null;
        }
    }
}
