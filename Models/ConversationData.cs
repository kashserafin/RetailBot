namespace RetailBot.Models
{
    public class ConversationData
    {
        public string TrackingNumber { get; set; }
        public ReturnsForm ReturnsForm { get; set; } = new ReturnsForm();
    }
}
