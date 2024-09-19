using Microsoft.AspNetCore.Mvc;
using ShopifyBilling.App.Attributes;
using ShopifyBilling.App.Data;
using ShopifyBilling.App.Models;
using ShopifySharp.Filters;
using ShopifySharp;
using System.Diagnostics;
using ShopifyBilling.App.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ShopifyBilling.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _dataContext;

        public HomeController(ILogger<HomeController> logger, DataContext ctx)
        {
            _logger = logger;
            _dataContext = ctx;
        }

        [AuthorizeWithActiveSubscription]
        public async Task<IActionResult> Index([FromQuery] string pageInfo = null)
        {
            /*
                pull the user record out of the database so we can use their access token, and then configure a ShopifySharp
                ListFilter to pull in orders for the requested page
            */

            var userSession = HttpContext.User.GetUserSession();
            var user = await _dataContext.Users.FirstAsync(u => u.Id == userSession.UserId);

            var service = new OrderService(user.ShopifyShopDomain, user.ShopifyAccessToken);

            // Build a list filter to get the requested page of orders  
            var limit = 50;

            // Only get the fields we'll use in the OrderSummary model 
            var orderFields = "name,id,customer,line_items,created_at";
            var filter = new ListFilter<Order>(pageInfo, limit, orderFields);
            var orders = await service.ListAsync(filter);

            return View(new HomeViewModel
            {
                Orders = orders.Items.Select(o => new OrderSummary(o)),
                NextPage = orders.GetNextPageFilter()?.PageInfo,
                PreviousPage = orders.GetPreviousPageFilter()?.PageInfo
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var userRequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View(new ErrorViewModel 
            { 
                RequestId = userRequestId 
            });
        }
    }
}
