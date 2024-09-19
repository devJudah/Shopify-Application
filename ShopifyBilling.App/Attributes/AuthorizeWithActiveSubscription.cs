using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopifyBilling.App.Extensions;

namespace ShopifyBilling.App.Attributes
{
    public class AuthorizeWithActiveSubscription : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext ctx)
        {
            // Check if the user is authenticated first
            if (!ctx.HttpContext.User.Identity.IsAuthenticated)
            {
                // The base class will handle basic authentication 
                return;
            }

            // Get the user's session and check if they're subscribed 
            var session = ctx.HttpContext.User.GetUserSession();
            if (!session.IsSubscribed)
            {
                // Redirect the user to Subscription/Start route where they can start a subscription
                ctx.Result = new RedirectToActionResult("Start", "Subscription", null);
            }
        }
    }
}
