using System;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using BioEngine.Core.Site.Filters;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Filters;
using BioEngine.Extra.IPB.Properties;
using BioEngine.Extra.IPB.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Extra.IPB
{
    public abstract class IPBModule : BioEngineModule
    {
        public override void ConfigureServices(WebHostBuilderContext builderContext, IServiceCollection services)
        {
            PropertiesProvider.RegisterBioEngineSectionProperties<IPBSectionPropertiesSet>();
            PropertiesProvider.RegisterBioEngineContentProperties<IPBContentPropertiesSet>();

            bool.TryParse(builderContext.Configuration["BE_IPB_API_DEV_MODE"] ?? "", out var devMode);
            int.TryParse(builderContext.Configuration["BE_IPB_API_ADMIN_GROUP_ID"], out var adminGroupId);
            int.TryParse(builderContext.Configuration["BE_IPB_API_PUBLISHER_GROUP_ID"], out var publisherGroupId);
            int.TryParse(builderContext.Configuration["BE_IPB_API_EDITOR_GROUP_ID"], out var editorGroupId);
            if (!Uri.TryCreate(builderContext.Configuration["BE_IPB_URL"], UriKind.Absolute, out var ipbUrl))
            {
                throw new ArgumentException($"Can't parse IPB url; {builderContext.Configuration["BE_IPB_URL"]}");
            }

            services.Configure<IPBConfig>(config =>
            {
                config.Url = ipbUrl;
                config.ApiUrl = new Uri($"{ipbUrl}/api");
                config.DevMode = devMode;
                config.AdminGroupId = adminGroupId;
                config.PublisherGroupId = publisherGroupId;
                config.EditorGroupId = editorGroupId;
                config.ClientId = builderContext.Configuration["BE_IPB_API_CLIENT_ID"];
                config.ReadOnlyKey = builderContext.Configuration["BE_IPB_API_READONLY_KEY"];
            });
            services.AddSingleton<IPBApiClientFactory>();
            services.AddScoped<IUserDataProvider, IPBUserDataProvider>();
        }
    }

    public class IPBSiteModule : IPBModule
    {
        public override void ConfigureServices(WebHostBuilderContext builderContext, IServiceCollection services)
        {
            base.ConfigureServices(builderContext, services);
            services.AddScoped<IPageFilter, IPBPageFilter>();
        }
    }

    public class IPBApiModule : IPBModule
    {
        public override void ConfigureServices(WebHostBuilderContext builderContext, IServiceCollection services)
        {
            base.ConfigureServices(builderContext, services);

            services.AddMvc().AddApplicationPart(typeof(WebHostBuilderExtensions).Assembly);
            services.AddScoped<IRepositoryFilter, IPBContentFilter>();
            services.AddScoped<IPropertiesOptionsResolver, IPBSectionPropertiesOptionsResolver>();
        }
    }
}