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

        protected IPBApiClient Client => _factory.GetClient(CurrentToken);
    }
}
