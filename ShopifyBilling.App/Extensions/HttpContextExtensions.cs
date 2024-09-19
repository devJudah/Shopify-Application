using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using ShopifyBilling.App.Models;
using ShopifySharp;
using System.Security.Claims;

namespace ShopifyBilling.App.Extensions
{
    public static class HttpContextExtensions
    {
        /*
            we only need to add the user's ID to the identity/cookie, along with a flag
            indicating whether they're subscribed to the application's monthly plan
        */

        public static async Task SignInAsync(this HttpContext ctx, Session session)
        {
            // TODO: sign the user in 

            //first we use the session parameter to turn the session into an ASP Identity.

            var claims = new List<Claim>
            {
                new Claim("UserId", session.UserId.ToString(), ClaimValueTypes.Integer32), 
                new Claim("IsSubscribed", session.IsSubscribed.ToString(), ClaimValueTypes.Boolean)
            };

            var authScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            var identity = new ClaimsIdentity(claims, authScheme);
            var principal = new ClaimsPrincipal(identity);

            await ctx.SignInAsync(principal);


        }

        public static async Task SignInAsync(this HttpContext ctx, UserAccount userAccount)
        {
            // TODO: sign the user in 

            //to simply convert the userAccount obj into a session.
            await SignInAsync(ctx, new Session(userAccount));
        }

        public static Session GetUserSession(this ClaimsPrincipal userPrincipal)
        {
            if (!userPrincipal.Identity.IsAuthenticated)
            {
                throw new Exception("User is not authenticated, cannot get user session."); 
            }

            T Find<T>(string claimName, Func<string, T> valueConverter)
            {
                var claim = userPrincipal.Claims.FirstOrDefault(claim => claim.Type == claimName);

                if (claim == null)
                {
                    throw new NullReferenceException($"Session claim {claimName} was not found, unable to parse user principal to Session.");
                }
                
                return valueConverter(claim.Value);
            }

            var session = new Session
            {
                UserId = Find("UserId", int.Parse),
                IsSubscribed = Find("IsSubscribed", bool.Parse)
            };

            return session;
        }

        /// <summary>
        /// Reads the request body to a string, handling buffering and rewinding.
        /// </summary>
        public static async Task<string> ReadBodyToStringAsync(this HttpRequest req)
        {
            req.EnableBuffering();

            using (var buffer = new MemoryStream())
            {
                await req.Body.CopyToAsync(buffer);

                buffer.Position = 0;

                if (req.Body.CanSeek && req.Body.Position != 0)
                {
                    req.Body.Position = 0;
                }

                using (var reader = new StreamReader(buffer))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
