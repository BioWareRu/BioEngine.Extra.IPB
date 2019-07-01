using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace BioEngine.Extra.IPB.Auth
{
    public class IPBSiteCurrentUserProvider : IPBCurrentUserProvider
    {
        public IPBSiteCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public override Task<string> GetAccessTokenAsync()
        {
            return HttpContextAccessor.HttpContext.GetTokenAsync("access_token");
        }
    }
}
