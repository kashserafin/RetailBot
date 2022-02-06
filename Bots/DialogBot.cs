using RetailBot.Extensions;
using RetailBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RetailBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        #region Variables
        protected readonly StateService _stateService;
        protected readonly Dialog _dialog;
        protected readonly ILogger _logger;
        #endregion

        public DialogBot(StateService stateService, BotServices botServices, T dialog, ILogger<DialogBot<T>> logger)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));
            _dialog = dialog ?? throw new System.ArgumentNullException(nameof(dialog));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            string card = Path.Combine(".", "Cards", "ProactiveGreetings.json");
            var adaptiveCardJson = (File.ReadAllText(card));
            var cardAttachment = AdaptiveCardService.CreateAdaptiveCardAttachment(adaptiveCardJson);

            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _stateService.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _stateService.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            if (turnContext.Activity.Text == null && turnContext.Activity.Value != null)
            {
                var value = turnContext.Activity.Value;
                turnContext.Activity.Text = JsonConvert.SerializeObject(value);
            }

            // Run the Dialog with the new message Activity.
            await _dialog.Run(turnContext, _stateService.DialogStateAccessor, cancellationToken);
        }
    }
}
