using RetailBot.Models;
using RetailBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RetailBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        private readonly BotServices _botServices;
        protected readonly ILogger _logger;
        #endregion  

        public MainDialog(StateService stateService, BotServices botServices, ILogger<MainDialog> logger) : base(nameof(MainDialog))
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));
            _botServices = botServices ?? throw new System.ArgumentNullException(nameof(botServices));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new BrowseBestsellersDialog($"{nameof(MainDialog)}.bestsellers", _stateService));
            AddDialog(new CheckOrderStatusDialog($"{nameof(MainDialog)}.checkOrder", _stateService, _botServices));
            AddDialog(new ContactUsDialog($"{nameof(MainDialog)}.contact", _stateService));
            AddDialog(new GreetingsDialog($"{nameof(MainDialog)}.greetings", _stateService));
            AddDialog(new HelpDialog($"{nameof(MainDialog)}.help", _stateService));
            AddDialog(new ReturnProductDialog($"{nameof(MainDialog)}.returns", _stateService));

            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var dc = new DialogContext(new DialogSet(), stepContext.Context, new DialogState());

            var allScores = await _botServices.Dispatch.RecognizeAsync(dc, stepContext.Context.Activity, cancellationToken);
            var orchestratorTopIntent = allScores.Intents.First().Key;
            
            var recognizerResult = new RecognizerResult();

            switch (orchestratorTopIntent)
            {
                case "DISPLAY_BOT_TWO":
                    recognizerResult = await _botServices.LuisDisplayBotTwo.RecognizeAsync(stepContext.Context, cancellationToken);
                    break;
                case "GENERAL":
                    recognizerResult = await _botServices.LuisGeneral.RecognizeAsync(stepContext.Context, cancellationToken);
                    break;
                case "QNA":
                    await ProcessQnAAsync(stepContext.Context, cancellationToken);
                    break;
                default:
                    _logger.LogInformation($"Dispatch unrecognized intent: {orchestratorTopIntent}.");
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Dispatch unrecognized intent: {orchestratorTopIntent}."), cancellationToken);
                    break;
            }

            var luisTopIntent = recognizerResult.GetTopScoringIntent().intent;
            var entities = recognizerResult.Entities.ToObject<Entities>();
            var conversationData = new ConversationData();

            switch (luisTopIntent)
            {
                case Constans.Intents.BrowseBestsellers:
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bestsellers", null, cancellationToken);
                case Constans.Intents.Cancel:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Cancelling... "), cancellationToken);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("What can I do for you? "), cancellationToken);

                    return await stepContext.CancelAllDialogsAsync(cancellationToken);
                case Constans.Intents.CheckOrderStatus:
                    var trackingNumber = entities.TrackingNumber?.FirstOrDefault();
                    conversationData.TrackingNumber = trackingNumber != null ? trackingNumber : conversationData.TrackingNumber;

                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.checkOrder", conversationData, cancellationToken);
                case Constans.Intents.ContactUs:
                case Constans.Intents.Escalate:
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.contact", null, cancellationToken);
                case Constans.Intents.Greetings:
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greetings", null, cancellationToken);
                case Constans.Intents.Help:
                    return await stepContext.ReplaceDialogAsync($"{nameof(MainDialog)}.help", null, cancellationToken);
                case Constans.Intents.ReturnProduct:
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.returns", null, cancellationToken);
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"LUIS unrecognized intent: {luisTopIntent}. "), cancellationToken);
                    break;
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task ProcessQnAAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessQnAAsync");

            var results = await _botServices.QnA.GetAnswersAsync(turnContext);
            if (results.Any())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, could not find an answer in the Q and A system."), cancellationToken);
            }
        }
    }
}
