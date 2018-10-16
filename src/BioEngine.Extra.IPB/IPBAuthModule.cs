using System;
using BioEngine.Core.Modules;
using BioEngine.Extra.IPB.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Extra.IPB
{
    public class IPBAuthModule : BioEngineModule
    {
        public override void ConfigureServices(WebHostBuilderContext builderContext, IServiceCollection services)
        {
            services
                .AddAuthentication("ipbToken")
                .AddScheme<IPBTokenAuthOptions, TokenAuthenticationHandler>("ipbToken", null);
            services.AddSingleton<IStartupFilter, IPBAuthStartupFilter>();
        }
    }

    public class IPBAuthStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseAuthentication();
                next(app);
            };
        }
    }
}