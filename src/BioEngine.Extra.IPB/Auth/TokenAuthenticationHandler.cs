using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BioEngine.Extra.IPB.Auth
{
    [UsedImplicitly]
    public class TokenAuthenticationHandler : AuthenticationHandler<IPBTokenAuthOptions>
    {
        private readonly IPBModuleConfig _ipbApiOptions;
        private readonly IPBApiClientFactory _apiClientFactory;

        private static readonly ConcurrentDictionary<string, User>
            TokenUsers = new ConcurrentDictionary<string, User>();

        public TokenAuthenticationHandler(IOptionsMonitor<IPBTokenAuthOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IPBModuleConfig ipbApiOptions,
            IPBApiClientFactory apiClientFactory) : base(options, logger, encoder, clock)
        {
            _ipbApiOptions = ipbApiOptions;
            _apiClientFactory = apiClientFactory;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = AuthenticateResult.Fail("No token");
            if (Context.Request.Headers.ContainsKey("Authorization"))
            {
                var headerString = Context.Request.Headers["Authorization"][0];
                if (headerString.Contains("Bearer"))
                {
                    var tokenString = headerString.Replace("Bearer ", "");

                    if (!string.IsNullOrEmpty(tokenString))
                    {
                        if (_ipbApiOptions.DevMode)
                            return HandleAuthenticateDev(tokenString);
                        var user = await GetUserAsync(tokenString);
                        var groupIds = new List<int> {_ipbApiOptions.AdminGroupId};
                        groupIds.AddRange(_ipbApiOptions.AdditionalGroupIds);
                        if (user != null)
                        {
                            if (!groupIds.Contains(user.PrimaryGroup.Id) &&
                                !groupIds.Intersect(user.SecondaryGroups.Select(g => g.Id).ToList()).Any())
                            {
                                result = AuthenticateResult.Fail($"Bad groups: {string.Join(", ", groupIds)}");
                            }
                            else
                            {
                                var userTicket = AuthenticationTicket(user, tokenString);
                                result = AuthenticateResult.Success(userTicket);
                                if (!TokenUsers.ContainsKey(tokenString))
                                {
                                    TokenUsers.TryAdd(tokenString, user);
                                }
                            }
                        }
                        else
                        {
                            result = AuthenticateResult.Fail("Bad token");
                        }
                    }
                }
            }

            stopwatch.Stop();
            Logger.LogWarning("Auth process: {time}", stopwatch.ElapsedMilliseconds);
            return result;
        }

        private AuthenticationTicket AuthenticationTicket(User user, string token)
        {
            var identity = new ClaimsIdentity("token");
            IpbAuthHelper.InsertClaims(user, identity, "", token, _ipbApiOptions);

            var userTicket =
                new AuthenticationTicket(new ClaimsPrincipal(identity), null, "token");
            return userTicket;
        }

        private AuthenticateResult HandleAuthenticateDev(string token)
        {
            var user = new User
            {
                Id = "1",
                Name = "Sonic",
                PhotoUrl = "/assets/img/avatar.png",
                ProfileUrl = "#",
                PrimaryGroup = new Group {Id = _ipbApiOptions.AdminGroupId}
            };
            var userTicket = AuthenticationTicket(user, token);
            return AuthenticateResult.Success(userTicket);
        }

        private async Task<User?> GetUserAsync(string token)
        {
            var exists = TokenUsers.TryGetValue(token, out var user);
            if (!exists)
            {
                user = await GetUserInformationAsync(token);
            }

            return user;
        }

        private async Task<User> GetUserInformationAsync(string token)
        {
            var apiClient = _apiClientFactory.GetClient(token);
            return await apiClient.GetUserAsync();
        }
    }
}
