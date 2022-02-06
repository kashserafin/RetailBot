using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace RetailBot.Services
{
    public class AdaptiveCardService
    {
        public static Attachment CreateAdaptiveCardAttachment(string card)
        {
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(card),
            };

            return adaptiveCardAttachment;
        }
    }
}
