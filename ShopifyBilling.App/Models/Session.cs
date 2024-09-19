namespace ShopifyBilling.App.Models
{
    public class Session
    {
        public Session(UserAccount user = null)
        {
            // create a session by passing in the user account
            // used when creating a session and signing a user in

            if (user != null)
            {
                UserId = user.Id;
                IsSubscribed = user.ShopifyChargeId.HasValue;
            }
            
        }
        public Session()
        {
            // used when reading a session cookie on subsequent requests.
            
        }

        public int UserId { get; set; }
        public bool IsSubscribed { get;  set; }
    }
}
