using BioEngine.Core.Modules;
using BioEngine.Extra.IPB.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Extra.IPB
{
    public class BioEngineIPBAuthModule : BioEngineModule
    {
        public override void ConfigureServices(WebHostBuilderContext builderContext, IServiceCollection services)
        {
            services
                .AddAuthentication("ipbToken")
                .AddScheme<IPBTokenAuthOptions, TokenAuthenticationHandler>("ipbToken", null);
        }

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();
        }
    }
}