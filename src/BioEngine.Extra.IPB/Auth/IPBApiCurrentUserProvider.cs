using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BioEngine.Extra.IPB.Auth
{
    public class IPBApiCurrentUserProvider : IPBCurrentUserProvider
    {
        public IPBApiCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public override Task<string> GetAccessTokenAsync()
        {
            return Task.FromResult(HttpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "ipbToken")?.Value);
        }
    }
}
