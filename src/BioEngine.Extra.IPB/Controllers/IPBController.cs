using BioEngine.Core.Interfaces;
using BioEngine.Extra.IPB.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.IPB.Controllers
{
    [ApiController]
    [Authorize]
    [Route("v1/ipb/[controller]")]
    public class IPBController : Controller
    {
        private readonly IPBApiClientFactory _factory;
        protected readonly ILogger Logger;

        public IPBController(IPBApiClientFactory factory, ILogger logger)
        {
            _factory = factory;
            Logger = logger;
        }

        protected IPBApiClient Client => _factory.GetClient(CurrentUserToken);

        protected IUser CurrentUser
        {
            get
            {
                var feature = HttpContext.Features.Get<ICurrentUserFeature>();
                return feature.User;
            }
        }

        protected string CurrentUserToken
        {
            get
            {
                var feature = HttpContext.Features.Get<ICurrentUserFeature>();
                return feature.Token;
            }
        }
    }
}