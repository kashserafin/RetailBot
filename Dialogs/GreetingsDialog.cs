using RetailBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace RetailBot.Dialogs
{
    public class GreetingsDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        #endregion

        public GreetingsDialog(string dialogId, StateService stateService) : base(dialogId)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));

            InitializeGreetingDialog();
        }

        private void InitializeGreetingDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                GreetUserStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingsDialog)}.mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(GreetingsDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> GreetUserStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Hi and welcome to Fictitious. I can help you browse bestsellers, check order status, return a product, or more. Type \"help\" any time to see the full list of requests I can handle. "), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("What can I do for you? "), cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
