using System;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Modules;
using BioEngine.Core.Providers;
using BioEngine.Core.Site.Filters;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Filters;
using BioEngine.Extra.IPB.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Extra.IPB
{
    public abstract class IPBModule : BioEngineModule
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
            SettingsProvider.RegisterBioEngineSectionSettings<IPBSectionSettings>();
            SettingsProvider.RegisterBioEngineContentSettings<IPBContentSettings>();
        }
    }

    public class IPBSiteModule : IPBModule
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
            base.ConfigureServices(context, services);
            services.AddScoped<IPageFilter, IPBPageFilter>();
        }
    }

    public class IPBApiModule : IPBModule
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
            base.ConfigureServices(context, services);
            bool.TryParse(context.Configuration["BE_IPB_API_DEV_MODE"] ?? "", out var devMode);
            int.TryParse(context.Configuration["BE_IPB_API_ADMIN_GROUP_ID"], out var adminGroupId);
            int.TryParse(context.Configuration["BE_IPB_API_PUBLISHER_GROUP_ID"], out var publisherGroupId);
            int.TryParse(context.Configuration["BE_IPB_API_EDITOR_GROUP_ID"], out var editorGroupId);
            services.Configure<IPBApiConfig>(config =>
            {
                config.ApiUrl = new Uri(context.Configuration["BE_IPB_API_URL"]);
                config.DevMode = devMode;
                config.AdminGroupId = adminGroupId;
                config.PublisherGroupId = publisherGroupId;
                config.EditorGroupId = editorGroupId;
                config.ClientId = context.Configuration["BE_IPB_API_CLIENT_ID"];
            });
            services.AddSingleton<IPBApiClientFactory>();
            services.AddMvc().AddApplicationPart(typeof(WebHostBuilderExtensions).Assembly);
            services.AddScoped<IRepositoryFilter, IPBContentFilter>();
            services.AddScoped<ISettingsOptionsResolver, IPBSectionSettingsOptionsResolver>();
        }
    }
}