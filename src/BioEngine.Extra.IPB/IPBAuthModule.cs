using System;
using BioEngine.Core.Modules;
using BioEngine.Extra.IPB.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.IPB
{
    public class IPBAuthModule : BioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
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
