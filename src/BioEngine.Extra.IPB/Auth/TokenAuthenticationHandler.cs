using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BioEngine.Core.Users;
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
        private readonly IOptions<IPBConfig> _ipbApiOptions;
        private readonly IPBApiClientFactory _apiClientFactory;

        private static readonly ConcurrentDictionary<string, User>
            TokenUsers = new ConcurrentDictionary<string, User>();

        public TokenAuthenticationHandler(IOptionsMonitor<IPBTokenAuthOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, IOptions<IPBConfig> ipbApiOptions,
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
                        if (_ipbApiOptions.Value.DevMode)
                            return HandleAuthenticateDev(tokenString);
                        var user = await GetUserAsync(tokenString);
                        if (user != null)
                        {
                            var userTicket = AuthenticationTicket(user);
                            Context.Features.Set<ICurrentUserFeature>(new CurrentUserFeature(user, tokenString));
                            result = AuthenticateResult.Success(userTicket);
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

        private AuthenticationTicket AuthenticationTicket(User user)
        {
            var identity = new ClaimsIdentity("token");
            identity.AddClaim(new Claim("Id", user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Name));
            var groups = user.GetGroupIds();
            if (groups.Contains(_ipbApiOptions.Value.AdminGroupId))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
            }

            if (groups.Contains(_ipbApiOptions.Value.PublisherGroupId))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "publisher"));
            }

            if (groups.Contains(_ipbApiOptions.Value.EditorGroupId))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "editor"));
            }

            var userTicket =
                new AuthenticationTicket(new ClaimsPrincipal(identity), null, "token");
            return userTicket;
        }

        private AuthenticateResult HandleAuthenticateDev(string token)
        {
            var user = new User
            {
                Id = 1,
                Name = "Sonic",
                PhotoUrl = "/assets/img/avatar.png",
                ProfileUrl = "#",
                PrimaryGroup = new Group {Id = _ipbApiOptions.Value.AdminGroupId}
            };
            var userTicket = AuthenticationTicket(user);
            Context.Features.Set<ICurrentUserFeature>(new CurrentUserFeature(user, token));
            return AuthenticateResult.Success(userTicket);
        }

        private async Task<User> GetUserAsync(string token)
        {
            var exists = TokenUsers.TryGetValue(token, out var user);
            if (!exists)
            {
                user = await GetUserInformationAsync(token);
                if (user != null)
                {
                    TokenUsers.TryAdd(token, user);
                }
            }

            return user;
        }

        private async Task<User> GetUserInformationAsync(string token)
        {
            var apiClient = _apiClientFactory.GetClient(token);
            return await apiClient.GetUserAsync();
        }
    }

    public class CurrentUserFeature : ICurrentUserFeature
    {
        public IUser User { get; }
        public string Token { get; }

        public CurrentUserFeature(IUser user, string token)
        {
            User = user;
            Token = token;
        }
    }
}
