using System;
using BioEngine.Core.Comments;
using BioEngine.Core.DB;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Auth;
using BioEngine.Extra.IPB.Comments;
using BioEngine.Extra.IPB.Entities;
using BioEngine.Extra.IPB.Filters;
using BioEngine.Extra.IPB.Properties;
using BioEngine.Extra.IPB.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.IPB
{
    public abstract class IPBModule : BioEngineModule<IPBModuleConfig>
    {
        protected override void CheckConfig()
        {
            if (Config.Url == null)
            {
                throw new ArgumentException($"IPB url is not set");
            }
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            PropertiesProvider.RegisterBioEngineSectionProperties<IPBSectionPropertiesSet>("ipbsection");


            services.AddSingleton(Config);
            services.AddSingleton<IPBApiClientFactory>();
            services.AddScoped<IUserDataProvider, IPBUserDataProvider>();
        }

        public override void RegisterEntities(BioEntitiesManager entitiesManager)
        {
            entitiesManager.Register<IPBContentSettings>();
            entitiesManager.Register<IPBComment>();
        }
    }

    public class IPBModuleConfig
    {
        public bool DevMode { get; set; }
        public int AdminGroupId { get; set; }
        public int PublisherGroupId { get; set; }
        public int EditorGroupId { get; set; }
        public Uri Url { get; set; }
        public Uri ApiUrl => new Uri($"{Url}/api");
        public string ApiClientId { get; set; } = "";
        public string ApiClientSecret { get; set; } = "";
        public string CallbackPath { get; set; } = "";
        public string AuthorizationEndpoint { get; set; } = "";
        public string TokenEndpoint { get; set; } = "";
        public string ApiReadonlyKey { get; set; } = "";

        public string IntegrationKey { get; set; } = "";
    }

    public class IPBSiteModule : IPBModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<ICommentsProvider, IPBCommentsProvider>();
            services.AddIpbOauthAuthentication(Config);
        }
    }

    public class IPBApiModule : IPBModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);

            services.AddScoped<IRepositoryHook, IPBContentHook>();
            services.AddScoped<IPropertiesOptionsResolver, IPBSectionPropertiesOptionsResolver>();
        }
    }
}
