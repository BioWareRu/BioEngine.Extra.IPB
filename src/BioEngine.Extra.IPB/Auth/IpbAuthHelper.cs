using System;
using System.Security.Claims;
using BioEngine.Core.Users;
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

        private static void InsertClaims(IUser userInfo, ClaimsIdentity identity, string claimsIssuer)
        {
            if (userInfo.Id > 0)
                identity.AddClaim(
                    new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString(), ClaimValueTypes.String, claimsIssuer));

            if (!string.IsNullOrEmpty(userInfo.Name))
                identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, userInfo.Name,
                    ClaimValueTypes.String,
                    claimsIssuer));

            if (!string.IsNullOrEmpty(userInfo.ProfileUrl))
                identity.AddClaim(new Claim(ClaimTypes.Webpage, userInfo.ProfileUrl, ClaimValueTypes.String,
                    claimsIssuer));

            if (!string.IsNullOrEmpty(userInfo.PhotoUrl))
                identity.AddClaim(new Claim("avatarUrl", userInfo.PhotoUrl, ClaimValueTypes.String, claimsIssuer));
        }
    }
}
