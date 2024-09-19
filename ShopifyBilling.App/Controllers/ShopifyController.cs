using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopifyBilling.App.Attributes;
using ShopifyBilling.App.Data;
using ShopifyBilling.App.Extensions;
using ShopifyBilling.App.Models;
using ShopifySharp;
using ShopifySharp.Utilities;

namespace ShopifyBilling.App.Controllers
{
    public class ShopifyController : Controller
    {
        public ShopifyController(DataContext ctx, ISecrets secrets)
        {
            _dataContext = ctx;
            _secrets = secrets;
        }

        private readonly DataContext _dataContext;
        public readonly ISecrets _secrets;
    


        [HttpGet, ValidateShopifyRequest]
        public async Task<IActionResult> Handshake([FromQuery] string shop)
        {


            //check if shop value is empty
            if (string.IsNullOrWhiteSpace(shop))
            {
                return Problem("Request is missing shop querystring parameter.", statusCode: 422);
            }

            /* 
                we need to check not only if a user is logged in, but if they're logged in as the same shop
                this is for cases where a user has multiple shops and installs the app on both shops
            */

            //check if user is already logged in
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                // check if they're logged in as the same shop
                var session = HttpContext.User.GetUserSession();
                var user =  await _dataContext.Users.FirstAsync(u => u.Id == session.UserId);

                //if the domains match, the user is logged in as the same shop and can be sent to the home page
                if (user.ShopifyShopDomain == shop)
                {
                    return RedirectToAction("Index", "Home");
                }

                // if the domains don't match, so (probably) the user owns 2 or more shopify shops
                // we will erase their auth cookie and send them to the login/install page
                await HttpContext.SignOutAsync();
            }

            // if the user has not installed the app, is not logged in  or was logged in to a different shop,
            // we send them to the login/install page
            return RedirectToAction("Login", "Auth");
        }


        [HttpGet, ValidateShopifyRequest]
        public async Task<IActionResult> AuthResult([FromQuery] string shop, [FromQuery] string code, [FromQuery] string state)
        {
            // Create the user account or log them in

            // Check if token exist in db
            var dbToken = await _dataContext.LoginStates.FirstOrDefaultAsync(t => t.Token == state);

            if (dbToken == null)
            {
                // the token has been used or never existed
                return RedirectToAction("HandleLogin", "Auth");
            }

            // If not delete the token so it can't be used again
            _dataContext.LoginStates.Remove(dbToken);
            await _dataContext.SaveChangesAsync();

            // get a shopify access token
            // which grants permanent access to the users store via API calls (for as long as they have the app installed)/

            // Exchange the code for an access token
            //var auth = new ShopifyOauthUtility();
            //var accessToken = await auth.AuthorizeAsync(code, shop, _secrets.ShopifyPublicKey, _secrets.ShopifySecretKey);
            
            var accessToken = await AuthorizationService.Authorize(code, shop, _secrets.ShopifyPublicKey, _secrets.ShopifySecretKey);

            // Use the access token to get the user's shop info
            var shopService = new ShopService(shop, accessToken);
            var shopData = await shopService.GetAsync();
            var shopId = shopData.Id.Value;

            //Check to see if the user's account already exists and should be updated or if it needs to be created

            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ShopifyShopDomain == shop);
            if (user == null)
            {
                // Create the account 
                user = new UserAccount
                {
                    ShopifyAccessToken = accessToken,
                    ShopifyShopDomain = shop,
                    ShopifyShopId = shopId
                };

                // Add the user to the database context 
                _dataContext.Add(user);
            }
            else
            {
                // Update the user's account 
                user.ShopifyAccessToken = accessToken;
                user.ShopifyShopDomain = shop;
                user.ShopifyShopId = shopId;
            }

            // Save the user account 
            await _dataContext.SaveChangesAsync();

            // Sign in the user and Issue them the Auth Cookie

            //Sign the user in
            await HttpContext.SignInAsync(user);

            // Check if the user needs to activate their subscrption charge 
            if (!user.ShopifyChargeId.HasValue)
            {
                return RedirectToAction("Start", "Subscription");
            }

            // User is subscribed, send them to the home page 
            return RedirectToAction("Index", "Home");
        }
    }
}
