using System;
using BioEngine.Core.Comments;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Comments;
using BioEngine.Extra.IPB.Filters;
using BioEngine.Extra.IPB.Properties;
using BioEngine.Extra.IPB.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Extra.IPB
{
    public abstract class IPBModule : BioEngineModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostingEnvironment environment)
        {
            PropertiesProvider.RegisterBioEngineSectionProperties<IPBSectionPropertiesSet>("ipbsection");
            PropertiesProvider.RegisterBioEngineContentProperties<IPBContentPropertiesSet>("ipbcontent");

            bool.TryParse(configuration["BE_IPB_API_DEV_MODE"] ?? "", out var devMode);
            int.TryParse(configuration["BE_IPB_API_ADMIN_GROUP_ID"], out var adminGroupId);
            int.TryParse(configuration["BE_IPB_API_PUBLISHER_GROUP_ID"], out var publisherGroupId);
            int.TryParse(configuration["BE_IPB_API_EDITOR_GROUP_ID"], out var editorGroupId);
            if (!Uri.TryCreate(configuration["BE_IPB_URL"], UriKind.Absolute, out var ipbUrl))
            {
                throw new ArgumentException($"Can't parse IPB url; {configuration["BE_IPB_URL"]}");
            }

            services.Configure<IPBConfig>(config =>
            {
                config.Url = ipbUrl;
                config.ApiUrl = new Uri($"{ipbUrl}/api");
                config.DevMode = devMode;
                config.AdminGroupId = adminGroupId;
                config.PublisherGroupId = publisherGroupId;
                config.EditorGroupId = editorGroupId;
                config.ClientId = configuration["BE_IPB_API_CLIENT_ID"];
                config.ReadOnlyKey = configuration["BE_IPB_API_READONLY_KEY"];
            });
            services.AddSingleton<IPBApiClientFactory>();
            services.AddScoped<IUserDataProvider, IPBUserDataProvider>();
        }
    }

    public class IPBSiteModule : IPBModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostingEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<ICommentsProvider, IPBCommentsProvider>();
        }
    }

    public class IPBApiModule : IPBModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostingEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);

            services.AddMvc().AddApplicationPart(typeof(WebHostBuilderExtensions).Assembly);
            services.AddScoped<IRepositoryHook, IPBContentHook>();
            services.AddScoped<IPropertiesOptionsResolver, IPBSectionPropertiesOptionsResolver>();
        }
    }
}
