namespace ShopifyBilling.App.Models
{
    public class HomeViewModel
    {
        public IEnumerable<OrderSummary> Orders { get; set; }
        public string NextPage { get; set; }
        public string PreviousPage { get; set; }
    }
}
