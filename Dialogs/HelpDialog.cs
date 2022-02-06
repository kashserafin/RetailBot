using RetailBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace RetailBot.Dialogs
{
    public class HelpDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        #endregion

        public HelpDialog(string dialogId, StateService stateService) : base(dialogId)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));

            InitializeContactUsDialog();
        }

        private void InitializeContactUsDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                HelpStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(HelpDialog)}.mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(HelpDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> HelpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = stepContext.Context.Activity.CreateReply();

            var heroCard = new HeroCard
            {
                Text = "Here’s a list of things you can ask: - Browse this season’s bestsellers \r- Check order status \r- Return purchased items \r- Get in touch with the store \r- How often should I wash my bra? \r- How do I wash my bra? \r- Can I wear a bra to bed? \r- How to determine my bra size?"
            };

            reply.Attachments.Add(heroCard.ToAttachment());
            await stepContext.Context.SendActivityAsync(reply);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
