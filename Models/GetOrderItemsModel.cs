using System.Collections.Generic;

namespace RetailBot.Models
{
    public class GetOrderItemsRoot
    {
        public string OrderNumber { get; set; }
        public string DateOfDelivery { get; set; }
        public List<OrderRecipient> OrderRecipient { get; set; }
        public List<OrderItems> OrderItems { get; set; }
    }

    public class OrderRecipient
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PhoneNumber { get; set; }
        public string ZipCode { get; set; }
    }

    public class OrderItems
    {
        public string SKU { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
