using System.Threading.Tasks;
using BioEngine.Core.Web;
using BioEngine.Extra.IPB.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioEngine.Extra.IPB.Controllers
{
    [ApiController]
    [Authorize]
    [Route("v1/ipb/[controller]")]
    public abstract class IPBController : BaseController
    {
        private readonly IPBApiClientFactory _factory;

        protected IPBController(BaseControllerContext context, IPBApiClientFactory factory) : base(context)
        {
            _factory = factory;
        }

        protected async Task<IPBApiClient> GetClientAsync()
        {
            return _factory.GetClient(await CurrentToken);
        }

        protected IPBApiClient ReadOnlyClient => _factory.GetReadOnlyClient();
    }
}
