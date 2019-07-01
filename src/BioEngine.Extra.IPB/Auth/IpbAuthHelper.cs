using System;
using System.Linq;
using System.Security.Claims;
using BioEngine.Extra.IPB.Api;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Extra.IPB.Auth
{
    public static class IpbAuthHelper
    {
        public static void AddIpbOauthAuthentication(this IServiceCollection services, IPBModuleConfig configuration)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
            {
                o.LoginPath = new PathString("/login");
                o.ExpireTimeSpan = TimeSpan.FromDays(30);
            }).AddOAuth("IPB",
                options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ClientId = configuration.ApiClientId;
                    options.ClientSecret = configuration.ApiClientSecret;
                    options.CallbackPath = new PathString(configuration.CallbackPath);
                    options.AuthorizationEndpoint = configuration.AuthorizationEndpoint;
                    options.TokenEndpoint = configuration.TokenEndpoint;
                    options.Scope.Add("profile");
                    options.SaveTokens = true;
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var factory = context.HttpContext.RequestServices.GetRequiredService<IPBApiClientFactory>();
                            var ipbApiClient = factory.GetClient(context.AccessToken);
                            var user = await ipbApiClient.GetUserAsync();

                            InsertClaims(user, context.Identity, context.Options.ClaimsIssuer);
                        }
                    };
                });
        }

        public static void InsertClaims(User user, ClaimsIdentity identity, string issuer, string token = null,
            IPBModuleConfig options = null)
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
            identity.AddClaim(new Claim("photo", user.PhotoUrl));
            identity.AddClaim(new Claim(ClaimTypes.Webpage, user.ProfileUrl));
            if (!string.IsNullOrEmpty(token))
            {
                identity.AddClaim(new Claim("ipbToken", token));
            }

            var groups = user.GetGroupIds();
            identity.AddClaim(new Claim(ClaimTypes.PrimaryGroupSid, user.PrimaryGroup.Id.ToString()));
            foreach (var group in groups)
            {
                identity.AddClaim(new Claim(ClaimTypes.GroupSid, group.ToString()));
            }

            if (options != null)
            {
                if (groups.Contains(options.AdminGroupId))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
                }

                if (groups.Contains(options.PublisherGroupId))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, "publisher"));
                }

                if (groups.Contains(options.EditorGroupId))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, "editor"));
                }
            }
        }
    }
}
