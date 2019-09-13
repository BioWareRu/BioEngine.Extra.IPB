using BioEngine.Extra.IPB.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.IPB
{
    public class IPBApiUsersModule : IPBUsersModule<IPBApiUsersModuleConfig, IPBApiCurrentUserProvider>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);

            services
                .AddAuthentication("ipbToken")
                .AddScheme<IPBTokenAuthOptions, TokenAuthenticationHandler>("ipbToken", null);
        }
    }
    
    public class IPBApiUsersModuleConfig : IPBUsersModuleConfig
    {
    }
}
