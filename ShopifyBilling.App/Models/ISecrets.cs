namespace ShopifyBilling.App.Models
{
    public interface ISecrets
    {
        public string ShopifySecretKey { get; }
        public string ShopifyPublicKey { get; }
    }
}
