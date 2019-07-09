using System;
using System.Collections.Generic;
using System.Security.Claims;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Api.Auth;
using BioEngine.Core.Comments;
using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using BioEngine.Core.Properties;
using BioEngine.Core.Social;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Auth;
using BioEngine.Extra.IPB.Comments;
using BioEngine.Extra.IPB.Properties;
using BioEngine.Extra.IPB.Publishing;
using BioEngine.Extra.IPB.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services.AddScoped<IUserDataProvider, IPBUserDataProvider>();
            services.AddScoped<IPBCommentsSynchronizer>();
            services.AddHttpClient();
        }
    }

    public class IPBModuleConfig
    {
        public IPBModuleConfig(Uri url)
        {
            Url = url;
        }

        public bool DevMode { get; set; }
        public int AdminGroupId { get; set; }
        public int[] AdditionalGroupIds { get; set; }
        public Uri Url { get; }
        public Uri ApiUrl => new Uri($"{Url!}/api");
        public string ApiClientId { get; set; } = "";
        public string ApiClientSecret { get; set; } = "";
        public string CallbackPath { get; set; } = "";
        public string AuthorizationEndpoint { get; set; } = "";
        public string TokenEndpoint { get; set; } = "";
        public string ApiReadonlyKey { get; set; } = "";
        public string ApiPublishKey { get; set; } = "";
    }

    public class IPBSiteModuleConfig : IPBModuleConfig
    {
        public IPBSiteModuleConfig(Uri url) : base(url)
        {
        }

        public string DataProtectionPath { get; set; } = "";
    }

    public class IPBSiteModule : IPBModule<IPBSiteModuleConfig>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            services.AddScoped<ICommentsProvider, IPBCommentsProvider>();
            services.AddScoped<ICurrentUserProvider, IPBSiteCurrentUserProvider>();
            services.AddIpbOauthAuthentication(Config, environment);
        }
    }

    public class IPBApiModuleConfig : IPBModuleConfig
    {
        public bool AuthEnabled { get; private set; }
        public Dictionary<string, AuthorizationPolicy> Policies { get; private set; }

        public IPBApiModuleConfig(Uri url) : base(url)
        {
        }

        public void EnableAuth(Dictionary<string, AuthorizationPolicy> policies)
        {
            AuthEnabled = true;
            Policies = policies;
        }
    }

    public class IPBApiModule : IPBModule<IPBApiModuleConfig>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);

            services.AddScoped<IContentPublisher<IPBPublishConfig>, IPBContentPublisher>();
            services.AddScoped<IPBContentPublisher>();
            services.AddScoped<IPropertiesOptionsResolver, IPBSectionPropertiesOptionsResolver>();
            services.AddScoped<ICurrentUserProvider, IPBApiCurrentUserProvider>();

            if (Config.AuthEnabled)
            {
                services
                    .AddAuthentication("ipbToken")
                    .AddScheme<IPBTokenAuthOptions, TokenAuthenticationHandler>("ipbToken", null);
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(BioPolicies.Admin,
                        builder => builder.RequireAuthenticatedUser().RequireClaim(ClaimTypes.Role, "admin"));
                    options.AddPolicy(BioPolicies.User, builder => builder.RequireAuthenticatedUser());
                    foreach (var policy in Config.Policies)
                    {
                        options.AddPolicy(policy.Key, policy.Value);
                    }
                });
            }
        }
    }
}
