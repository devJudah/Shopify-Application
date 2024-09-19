using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopifyBilling.App.Models;
using ShopifySharp;

namespace ShopifyBilling.App.Attributes
{
    public class ValidateShopifyRequest : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // TODO: check if the request passes Shopify's validation scheme 
            var secrets = (ISecrets)context.HttpContext.RequestServices.GetService(typeof(ISecrets));
            var querystring = context.HttpContext.Request.Query;
            var isAuthentic = AuthorizationService.IsAuthenticRequest(querystring, secrets.ShopifySecretKey);

            if (isAuthentic)
            {
                await next();
            }
            else
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
