using System;
using BioEngine.Core.Properties;
using BioEngine.Core.Social;
using BioEngine.Extra.IPB.Properties;
using BioEngine.Extra.IPB.Publishing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.IPB
{
    public class IPBApiModule : IPBModule<IPBApiModuleConfig>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);

            services.AddScoped<IContentPublisher<IPBPublishConfig>, IPBContentPublisher>();
            services.AddScoped<IPBContentPublisher>();
            services.AddScoped<IPropertiesOptionsResolver, IPBSectionPropertiesOptionsResolver>();
        }
    }

    public class IPBApiModuleConfig : IPBModuleConfig
    {
        public IPBApiModuleConfig(Uri url) : base(url)
        {
        }
    }
}
