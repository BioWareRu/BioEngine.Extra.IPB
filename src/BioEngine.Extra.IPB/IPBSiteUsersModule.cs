using BioEngine.Extra.IPB.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.IPB
{
    public class IPBSiteUsersModule : IPBUsersModule<IPBSiteUsersModuleConfig, IPBSiteCurrentUserProvider>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);

            services.AddIpbOauthAuthentication(Config, environment);
        }
    }

    public class IPBSiteUsersModuleConfig : IPBUsersModuleConfig
    {
    }
}
