using RetailBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RetailBot.Dialogs
{
    public class BrowseBestsellersDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        #endregion

        public BrowseBestsellersDialog(string dialogId, StateService stateService) : base(dialogId)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));

            InitializeBrowseBestsellersDialog();
        }

        private void InitializeBrowseBestsellersDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                BrowseStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(BrowseBestsellersDialog)}.mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(BrowseBestsellersDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> BrowseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = stepContext.Context.Activity.CreateReply();

            // Demonstrate carousel of hero cards
            for (int i = 0; i < 3; i++)
            {
                var heroCard = new HeroCard
                {
                    Text = "With summer just around the corner, a bralette is what you need! It’s as comfy as it is sexy 😉 Have a look at MODEL01, our bestseller in the bralette category.",
                    Images = new List<CardImage> { new CardImage("https://upload.wikimedia.org/wikipedia/commons/thumb/9/97/DIY_Lace_Bralette.jpg/595px-DIY_Lace_Bralette.jpg") },
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Shop now", value: "https://commons.wikimedia.org/wiki/File:DIY_Lace_Bralette.jpg") },
                };
                
                reply.Attachments.Add(heroCard.ToAttachment());
            }
            
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            await stepContext.Context.SendActivityAsync(reply);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
