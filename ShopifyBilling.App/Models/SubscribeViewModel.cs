namespace ShopifyBilling.App.Models
{
    public class SubscribeViewModel
    {
        // This model for now is only ised to relay an error message when the app fails to start a subscription for the user
        public string Error { get; set; }

        public bool ShowError => !string.IsNullOrWhiteSpace(Error);
    }
}
