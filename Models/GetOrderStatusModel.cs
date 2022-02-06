using System.Collections.Generic;

namespace RetailBot.Models
{
    public class GetOrderStatusRoot
    {
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public string CarrierLogo { get; set; }
        public List<Sender> Sender { get; set; }
        public List<Recipient> Recipient { get; set; }
        public List<OrderStatus> OrderStatus { get; set; }
        public List<OrderContent> OrderContent { get; set; }
    }

    public class Sender
    {
        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PhoneNumber { get; set; }
        public string ZipCode { get; set; }
    }

    public class Recipient
    {
        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PhoneNumber { get; set; }
        public string ZipCode { get; set; }
    }

    public class OrderStatus
    {
        public string Date { get; set; }
        public string Status { get; set; }
    }

    public class OrderContent
    {
        public string SKU { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
