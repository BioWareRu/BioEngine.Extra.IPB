using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Extra.IPB.Auth
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddIPBTokenAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddAuthentication("ipbToken")
                .AddScheme<IPBTokenAuthOptions, TokenAuthenticationHandler>("ipbToken", null);

            return services;
        }

        public static IApplicationBuilder UseIPBTokenAuth(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            return app;
        }

        public static IWebHostBuilder AddIPBTokenAuth(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.ConfigureServices(
                (context, services) =>
                {
                    services.AddIPBTokenAuth(context.Configuration);
                });
            webHostBuilder.Configure(app => { app.UseIPBTokenAuth(); });
            return webHostBuilder;
        }
    }
}