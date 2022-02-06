using RetailBot.Models;
using RetailBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RetailBot.Dialogs
{
    public class ReturnProductDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        #endregion

        public ReturnProductDialog(string dialogId, StateService stateService) : base(dialogId)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));

            InitializeReturnProductDialog();
        }

        private void InitializeReturnProductDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                ReturnsFormOneStepAsync,
                ReturnsFormTwoStepAsync,
                ReturnsFormSummaryStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(ReturnProductDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(ReturnProductDialog)}.returnsForm1"));
            AddDialog(new TextPrompt($"{nameof(ReturnProductDialog)}.returnsForm2"));
            AddDialog(new WaterfallDialog($"{nameof(ReturnProductDialog)}.returnsSummary"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(ReturnProductDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> ReturnsFormOneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            #region AdaptiveCard
            var cardPath = Path.Combine(".", "Cards", "ReturnsForm1.json");
            var cardJson = (File.ReadAllText(cardPath));
            var cardAttachment = AdaptiveCardService.CreateAdaptiveCardAttachment(cardJson);

            var promptOptions = new PromptOptions
            {
                Prompt = new Activity
                {
                    Attachments = new List<Attachment>() { cardAttachment },
                    Type = ActivityTypes.Message
                }
            };
            #endregion

            return await stepContext.PromptAsync($"{nameof(ReturnProductDialog)}.returnsForm1", promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ReturnsFormTwoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = stepContext.Context.Activity?.Value as JObject;
            var resultArray = result?.Properties().Select(p => p.Value?.ToString()).ToList();

            if (resultArray != null)
            {
                var conversationData = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData());
                
                conversationData.ReturnsForm.FirstName = resultArray[0];
                conversationData.ReturnsForm.LastName = resultArray[1];
                conversationData.ReturnsForm.OrderNumber = resultArray[2];

                await _stateService.ConversationDataAccessor.SetAsync(stepContext.Context, conversationData);

                var response = GetOrderItems(conversationData.ReturnsForm.FirstName, conversationData.ReturnsForm.LastName, conversationData.ReturnsForm.OrderNumber);

                if (response == null)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Order number {conversationData.ReturnsForm.OrderNumber} for {conversationData.ReturnsForm.FirstName} {conversationData.ReturnsForm.LastName} doesn't exist. "), cancellationToken);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Check order details and try again. "), cancellationToken);

                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }

                #region AdaptiveCard
                var templatePath = Path.Combine(".", "Cards", "ReturnsForm2.json");
                var templateJson = (File.ReadAllText(templatePath));
                var template = new AdaptiveCards.Templating.AdaptiveCardTemplate(templateJson);

                var card = template.Expand(response);
                var cardAttachment = AdaptiveCardService.CreateAdaptiveCardAttachment(card);

                var promptOptions = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Attachments = new List<Attachment>() { cardAttachment },
                        Type = ActivityTypes.Message
                    }
                };
                #endregion

                return await stepContext.PromptAsync($"{nameof(ReturnProductDialog)}.returnsForm2", promptOptions, cancellationToken);
            }
            
            return await stepContext.ReplaceDialogAsync($"{nameof(MainDialog)}.mainFlow", null, cancellationToken);
        }

        private async Task<DialogTurnResult> ReturnsFormSummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = stepContext.Context.Activity?.Value as JObject;
            var resultArray = result?.Properties().Select(p => p.Value?.ToString()).ToList();

            if (resultArray != null)
            {
                var conversationData = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData());
                
                var bulletListOfItems = "- " + resultArray[1].Replace(",", " \r- ") + " \r";
                conversationData.ReturnsForm.Items = bulletListOfItems;
                if (resultArray.Count == 3) { conversationData.ReturnsForm.Feedback = resultArray[2]; }
                conversationData.ReturnsForm.DateOfSubmission = DateTime.Today.Date.ToString();

                await _stateService.ConversationDataAccessor.SetAsync(stepContext.Context, conversationData);

                #region AdaptiveCard
                var templatePath = Path.Combine(".", "Cards", "ReturnsFormSummary.json");
                var templateJson = (File.ReadAllText(templatePath));
                var template = new AdaptiveCards.Templating.AdaptiveCardTemplate(templateJson);

                var summaryValues = conversationData.ReturnsForm;
                var card = template.Expand(summaryValues);
                var cardAttachment = AdaptiveCardService.CreateAdaptiveCardAttachment(card);

                var promptOptions = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Attachments = new List<Attachment>() { cardAttachment },
                        Type = ActivityTypes.Message
                    }
                };
                #endregion

                await stepContext.Context.SendActivityAsync(new Activity { Type = ActivityTypes.Typing }, cancellationToken);
                await Task.Delay(1000);

                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync($"{nameof(MainDialog)}.mainFlow", null, cancellationToken);
        }

        #region Mocked API call
        public GetOrderItemsRoot GetOrderItems(string firstName, string lastName, string orderNumber)
        {
            GetOrderItemsRoot response = null;

            var mockPath = Path.Combine(".", "Mocks", "GetOrderItems.json");
            var mockJson = File.ReadAllText(mockPath);
            var mock = JsonConvert.DeserializeObject<GetOrderItemsRoot>(mockJson);

            if (mock.OrderNumber == orderNumber &&
                mock.OrderRecipient[0].Name == firstName &&
                mock.OrderRecipient[0].Surname == lastName)
            {
                response = mock;
            }

            return response;
        }
        #endregion
    }
}
