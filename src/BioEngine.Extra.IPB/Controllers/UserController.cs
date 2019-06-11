using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Extra.IPB.Controllers
{
    public class UserController : Controller
    {
        [HttpGet("/login")]
        public async Task LoginAsync()
        {
            await HttpContext.ChallengeAsync("IPB",
                new AuthenticationProperties {RedirectUri = "/", IsPersistent = true});
        }

        [HttpGet("/logout")]
        public async Task LogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Response.Redirect("/");
        }
    }
}
