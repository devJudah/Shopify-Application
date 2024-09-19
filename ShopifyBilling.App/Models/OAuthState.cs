namespace ShopifyBilling.App.Models
{
    public class OAuthState
    {
        // this class is used to track and validate all login requests issued by the application
        /*every time a user tries to log in to the app, an oauthstate record is added to the database, when they complete
            the oauth process, we check to see if that record is still there; if not, they must log in again. the goal is
            to limit the user to one login per oauth state*/

        public int Id { get; set; }
        
        //a tiimestamp that tracks when it was created.
        public DateTimeOffset DateCreated { get; set; }

        // randomly generated token
        public string Token { get; set; }



    }
}
