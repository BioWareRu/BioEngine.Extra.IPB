using System;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Comments;
using BioEngine.Extra.IPB.Entities;
using BioEngine.Extra.IPB.Properties;
using BioEngine.Extra.IPB.Publishing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.IPB
{
    public abstract class IPBModule<TConfig> : BaseBioEngineModule<TConfig> where TConfig : IPBModuleConfig
    {
        protected override void CheckConfig()
        {
            if (Config.Url == null)
            {
                throw new ArgumentException("IPB url is not set");
            }
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            PropertiesProvider.RegisterBioEngineProperties<IPBSitePropertiesSet, Site>("ipbsite");

            services.AddSingleton(typeof(IPBModuleConfig), Config);
            services.AddSingleton(Config);
            services.AddSingleton<IPBApiClientFactory>();
            services.AddScoped<IPBCommentsSynchronizer>();
            services.AddHttpClient();
        }
    }

    public abstract class IPBModuleConfig
    {
        protected IPBModuleConfig(Uri url)
        {
            Url = url;
        }
        
        public Uri Url { get; }
        public Uri ApiUrl => new Uri($"{Url!}/api");
        public string ApiReadonlyKey { get; set; } = "";
        public string ApiPublishKey { get; set; } = "";
    }
    
    public class IpbBioContextConfigurator: IBioContextModelConfigurator{
        public void Configure(ModelBuilder modelBuilder, ILogger<BioContext> logger)
        {
            modelBuilder.RegisterEntity<IPBPublishRecord>();
            modelBuilder.RegisterEntity<IPBComment>();
        }
    }
}
