namespace ShopifyBilling.App.Models
{
    public class Secrets : ISecrets
    {
        public Secrets(IConfiguration config)
        {
            string Find(string key) 
            {
                var value = config.GetValue<string>(key);

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new NullReferenceException(key);
                }

                return value;
            }

            ShopifySecretKey = Find("SHOPIFY_SECRET_KEY");
            ShopifyPublicKey = Find("SHOPIFY_PUBLIC_KEY");
        }

        public string ShopifySecretKey { get; }
        public string ShopifyPublicKey { get; }
    }
}
