namespace ShopifyBilling.App.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        public long ShopifyShopId { get; set; }
        public string ShopifyShopDomain { get; set; }
        public string ShopifyAccessToken { get; set; }
        public long? ShopifyChargeId { get; set; }
    }
}
