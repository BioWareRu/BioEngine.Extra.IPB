using BioEngine.Core.Abstractions;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Auth;
using BioEngine.Extra.IPB.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.IPB
{
    public abstract class
        IPBUsersModule<TConfig, TCurrentUserProvider> : BaseUsersModule<TConfig, User,
            IPBUserDataProvider, TCurrentUserProvider> where TConfig : IPBUsersModuleConfig
        where TCurrentUserProvider : class, ICurrentUserProvider
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);

            services.AddSingleton(typeof(IPBUsersModuleConfig), Config);
            services.AddSingleton(Config);
            services.AddHttpClient();
        }
    }

    public abstract class IPBUsersModuleConfig : BaseUsersModuleConfig
    {
        public string ApiClientId { get; set; } = "";
        public string ApiClientSecret { get; set; } = "";
        public string CallbackPath { get; set; } = "";
        public string AuthorizationEndpoint { get; set; } = "";
        public string TokenEndpoint { get; set; } = "";
        public string DataProtectionPath { get; set; } = "";
        public bool DevMode { get; set; }
        public int AdminGroupId { get; set; }
        public int[] AdditionalGroupIds { get; set; }
    }
}
