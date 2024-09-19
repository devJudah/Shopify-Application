namespace ShopifyBilling.App.Models
{
    public class LoginViewModel
    {
        //since we're using shopify OAuth to login
        // our model only needs one shop domain string and an error prop
        public string ShopDomain { get; set; }
        public string Error { get; set; }
        public bool ShowError => !string.IsNullOrWhiteSpace(Error);
    }
}
