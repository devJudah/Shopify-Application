using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopifyBilling.App.Attributes;
using ShopifyBilling.App.Data;
using ShopifyBilling.App.Extensions;
using ShopifyBilling.App.Models;
using ShopifySharp;
using System.Net;

namespace ShopifyBilling.App.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly DataContext _dataContext;

        private readonly IHostEnvironment _environment;

        private readonly string _subscriptionRedirectUrl;

        public SubscriptionController(DataContext ctx, IHostEnvironment env, ISecrets secrets)
        {
            _dataContext = ctx;
            _environment = env;

            // Add you localhost URL and Shopify Redirection strings
            _subscriptionRedirectUrl = "http://localhost:5080/Subscription/ChargeResult";
        }

        [HttpGet]
        public async Task<IActionResult> Start()
        {
            // Make sure the user isn't already subscribed 
            var userSession = HttpContext.User.GetUserSession();
            var user = await _dataContext.Users.FirstAsync(u => u.Id == userSession.UserId);

            if (user.ShopifyChargeId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new SubscribeViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> HandleStartSubscription()
        {
            // get the user's account record
            var userSession = HttpContext.User.GetUserSession();
            var user = await _dataContext.Users.FirstAsync(u => u.Id == userSession.UserId);

            if (user.ShopifyChargeId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var service = new RecurringChargeService(user.ShopifyShopDomain, user.ShopifyAccessToken);

            var charge = await service.CreateAsync(new RecurringCharge
            {
                TrialDays = 7,
                Name = "StarPlatinum Subscription Plan",
                Price = 9.99M,
                ReturnUrl = _subscriptionRedirectUrl,

                // If the app is running in development mode, make this a test charge 
                Test = _environment.IsDevelopment()
            });

            return Redirect(charge.ConfirmationUrl);
        }

        [HttpGet]
        public async Task<IActionResult> ChargeResult([FromQuery] long charge_id)
        {
            // Again, grab the user and make sure they are not already subscribed
            var userSession = HttpContext.User.GetUserSession();
            var user = await _dataContext.Users.FirstAsync(u => u.Id == userSession.UserId);

            if (user.ShopifyChargeId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            // Get the subscription they're activating
            var service = new RecurringChargeService(user.ShopifyShopDomain, user.ShopifyAccessToken);
            var charge = await service.GetAsync(charge_id);


            // Check the status of the charge
            switch (charge.Status)
            {
                case "pending":
                    // User has not accepted or declined the charge. Send them back to the confirmation url
                    return Redirect(charge.ConfirmationUrl);

                case "expired":
                case "declined":
                    // The charge expired or declined. Prompt the user to accept a new charge.
                    return RedirectToAction(actionName:"Start");

                case "active":
                    // User has activated the charge, update their account and session.
                    user.ShopifyChargeId = charge_id;

                    await _dataContext.SaveChangesAsync();
                    await HttpContext.SignInAsync(user);

                    // User's subscription has been activated, they can now use the app
                    return RedirectToAction(actionName: "Index", controllerName: "Home");


                default:
                    throw new ArgumentOutOfRangeException(nameof(charge.Status), $"Unhandled Shopify charge status of {charge.Status}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var userSession = HttpContext.User.GetUserSession();
            var user = await _dataContext.Users.SingleAsync(u => u.Id == userSession.UserId);

            if (user.ShopifyChargeId.HasValue == false)
            {
                return RedirectToAction("Start");
            }

            // Pull in the user's subscription charge from Shopify 
            var service = new RecurringChargeService(user.ShopifyShopDomain, user.ShopifyAccessToken);
            var charge = await service.GetAsync(user.ShopifyChargeId.Value);

            return View(new SubscriptionViewModel(charge));
        }

/*        [AuthorizeWithActiveSubscription]
        public async Task<IActionResult> Index()
        {
            var userSession = HttpContext.User.GetUserSession();
            var user = await _dataContext.Users.SingleAsync(u => u.Id == userSession.UserId);

            if (user.ShopifyChargeId.HasValue == false)
            {
                return RedirectToAction("Start");
            }

            // Pull in the user's subscription data from Shopify
            var chargeService = new RecurringChargeService(user.ShopifyShopDomain, user.ShopifyAccessToken);
            RecurringCharge charge;

            try
            {
                charge = await chargeService.GetAsync(user.ShopifyChargeId.Value);
            }
            catch (ShopifyException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
            {
                // The user's subscription no longer exists. Update their user model to delete their charge ID
                user.ShopifyChargeId = null;

                await _dataContext.SaveChangesAsync();

                // Update the user's session, then redirect them to the subscription page to accept a new charge
                await HttpContext.SignInAsync(user);

                return RedirectToAction("Start");
            }

            return View(new SubscriptionViewModel(charge));
        }*/
    }
}
