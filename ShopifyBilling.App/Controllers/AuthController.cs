using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using ShopifyBilling.App.Data;
using ShopifyBilling.App.Models;
using ShopifySharp;
using ShopifySharp.Utilities;

namespace ShopifyBilling.App.Controllers
{
    public class AuthController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ISecrets _secrets;
        private readonly string _oauthRedirectUrl;

        public AuthController(DataContext ctx, ISecrets secrets)
        {
            _dataContext = ctx;
            _secrets = secrets;
            // Add you localhost URL and Shopify Redirection strings
            _oauthRedirectUrl = "http://localhost:5080/Shopify/AuthResult";
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            // log the user out 
            await HttpContext.SignOutAsync();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<ActionResult> Login([FromQuery] string shop = null)
        {
            //  return a view showing the login form 
            var model = new LoginViewModel
            {
                ShopDomain = shop
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> HandleLogin([FromForm] string shop)
        {
            // TODO: log the user in 

            // Check to make sure the domain is a real Shopify store 
            var isValidShopDomain = await AuthorizationService.IsValidShopDomainAsync(shop);

            if (!isValidShopDomain)
            {
                return View("Login", new LoginViewModel
                {
                    ShopDomain = shop,
                    Error = $"It looks like {shop} is not a valid Shopify shop domain."
                });
            }

            // Create a list of permissions to request
            var permissions = new[] { "read_orders" };

            // Create an OAuthState token 
            var oauthState = await _dataContext.LoginStates.AddAsync(new OAuthState
            {
                DateCreated = DateTimeOffset.Now,
                Token = Guid.NewGuid().ToString()
            });

            // Save the token 
            await _dataContext.SaveChangesAsync();

            // Create an OAuth URL using the token and permissions 
            var oauthUrl = AuthorizationService.BuildAuthorizationUrl
                (
                    permissions,
                    shop,
                    _secrets.ShopifyPublicKey,
                    _oauthRedirectUrl,
                    oauthState.Entity.Token
                );

            // Redirect the user to the OAuth URL 
            return Redirect(oauthUrl.ToString());
        }
    }
}
