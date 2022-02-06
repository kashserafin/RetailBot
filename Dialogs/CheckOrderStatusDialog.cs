using RetailBot.Models;
using RetailBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RetailBot.Dialogs
{
    public class CheckOrderStatusDialog : CancelDialog
    {
        #region Variables
        private readonly StateService _stateService;
        private readonly BotServices _botServices;
        #endregion

        public CheckOrderStatusDialog(string dialogId, StateService stateService, BotServices botServices) : base(dialogId, botServices)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));

            InitializeCheckOrderStatusDialog();
        }

        private void InitializeCheckOrderStatusDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                GetTrackingNumberStepAsync,
                OrderStatusStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(CheckOrderStatusDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(CheckOrderStatusDialog)}.getTrackingNumber", TrackingNumberValidator));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(CheckOrderStatusDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> GetTrackingNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = (ConversationData)stepContext.Options;

            if (string.IsNullOrEmpty(conversationData.TrackingNumber))
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter your tracking number to check the status of your package. The tracking number is a 6-digit code that was sent to your e-mail address along with your order confirmation. "),
                    RetryPrompt = MessageFactory.Text("Please enter a valid tracking number. The tracking number is a 6-digit code like 123456. "),
                    
                };

                return await stepContext.PromptAsync($"{nameof(CheckOrderStatusDialog)}.getTrackingNumber", promptOptions, cancellationToken);
            }

            return await stepContext.NextAsync(conversationData.TrackingNumber, cancellationToken);
        }

        private async Task<DialogTurnResult> OrderStatusStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData());
            conversationData.TrackingNumber = (string)stepContext.Result;

            if (conversationData.TrackingNumber != "123456")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Sorry, tracking number {conversationData.TrackingNumber} doesn't exist. "), cancellationToken);
                conversationData.TrackingNumber = "";

                return await stepContext.ReplaceDialogAsync($"{ nameof(CheckOrderStatusDialog)}.mainFlow", conversationData, cancellationToken);
            }

            #region AdaptiveCard
            var mockPath = Path.Combine(".", "Mocks", "GetOrderStatus.json");
            var mockJson = File.ReadAllText(mockPath);
            var mock = JsonConvert.DeserializeObject<GetOrderStatusRoot>(mockJson);

            var templatePath = Path.Combine(".", "Cards", "OrderStatus.json");
            var templateJson = (File.ReadAllText(templatePath));
            var template = new AdaptiveCards.Templating.AdaptiveCardTemplate(templateJson);

            var card = template.Expand(mock);
            var cardAttachment = AdaptiveCardService.CreateAdaptiveCardAttachment(card);
            #endregion

            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        #region Validators
        private Task<bool> TrackingNumberValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;

            if (promptContext.Recognized.Succeeded)
            {
                Regex rx = new Regex(@"\d{6}");
                valid = rx.IsMatch(promptContext.Recognized.Value);

                if (valid)
                {
                    var match = rx.Match(promptContext.Recognized.Value);
                    promptContext.Recognized.Value = match.Value;
                }
            }

            return Task.FromResult(valid);
        }
        #endregion
    }
}
