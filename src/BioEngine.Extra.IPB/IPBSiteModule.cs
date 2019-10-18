using System;
using BioEngine.Core.Comments;
using BioEngine.Extra.IPB.Comments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.IPB
{
    public class IPBSiteModule : IPBModule<IPBSiteModuleConfig>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<ICommentsProvider<string>, IPBCommentsProvider>();
        }
    }

    public class IPBSiteModuleConfig : IPBModuleConfig
    {
        public IPBSiteModuleConfig(Uri url) : base(url)
        {
        }
    }
}
