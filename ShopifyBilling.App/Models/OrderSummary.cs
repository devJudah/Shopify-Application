using ShopifySharp;

namespace ShopifyBilling.App.Models
{
    public class OrderSummary
    {
        public OrderSummary(Order order)
        {
            OrderId = order.Id.Value;
            Name = order.Name;
            DateCreated = order.CreatedAt.Value.ToString();

            if (order.Customer != null)
            {
                CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}";
            }
            else
            {
                CustomerName = "(No Customer)";
            }

            var totalLineItems = order.LineItems.Count();

            if (totalLineItems == 0)
            {
                LineItemSummary = "No line items, order is empty";
            }else if (totalLineItems == 1)
            {
                var li = order.LineItems.First();

                LineItemSummary = $"{li.Quantity} x {li.Title}";
            }
            else
            {
                var li = order.LineItems.First();
                var totalOtherItems = order.LineItems.Sum(item => item.Quantity) - li.Quantity;

                LineItemSummary = $"{li.Quantity} x {li.Title} and {totalOtherItems} other items.";
            }
        }


        public long OrderId { get; set; }
        public string Name { get; set; }
        public string DateCreated { get; set; }
        public string CustomerName { get; set; }
        public string LineItemSummary { get; set; }
    }
}
