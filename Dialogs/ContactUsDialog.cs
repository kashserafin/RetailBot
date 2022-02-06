using RetailBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RetailBot.Dialogs
{
    public class ContactUsDialog : ComponentDialog
    {
        #region Variables
        private readonly StateService _stateService;
        #endregion

        public ContactUsDialog(string dialogId, StateService stateService) : base(dialogId)
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));

            InitializeContactUsDialog();
        }

        private void InitializeContactUsDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                GetInTouchStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(ContactUsDialog)}.mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(ContactUsDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> GetInTouchStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            TimeSpan start = new TimeSpan(10, 0, 0); //10 o'clock
            TimeSpan end = new TimeSpan(18, 0, 0); //18 o'clock
            TimeSpan now = DateTime.Now.TimeOfDay;

            if ((now > start) && (now < end))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You can reach us at customer@care.com, or by phone at 123456789. "), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("We are currently out of office, but you can reach us at customer@care.com, or by phone at 123456789 within our working hours (Mon-Fri 10am-6pm). "), cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
