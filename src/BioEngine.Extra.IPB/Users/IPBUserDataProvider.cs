using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.IPB.Users
{
    public class IPBUserDataProvider : IUserDataProvider<string>
    {
        private readonly IPBApiClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<IPBUserDataProvider> _logger;

        public IPBUserDataProvider(IPBApiClientFactory clientFactory, IMemoryCache memoryCache,
            ILogger<IPBUserDataProvider> logger)
        {
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        private static string GetCacheKey(string userId)
        {
            return $"ipbuserdata{userId}";
        }

        private List<IUser<string>> GetFromCache(IEnumerable<string> userIds)
        {
            _logger.LogTrace("Get user data from cache");
            return userIds.Select(GetCacheKey).Select(key => _memoryCache.Get<IUser<string>>(key))
                .Where(userData => userData != null).ToList();
        }

        public async Task<List<IUser<string>>> GetDataAsync(string[] userIds)
        {
            var data = GetFromCache(userIds);
            var notFoundUserIds = userIds
                .Where(id => data.All(ud => ud.Id != id) && !string.IsNullOrEmpty(id) && id != "0").ToArray();
            if (notFoundUserIds.Length > 0)
            {
                _logger.LogTrace("Load users data from api");
                var tasks = notFoundUserIds.Select(id => GetApiClient().GetUserByIdAsync(id));
                var users = await Task.WhenAll(tasks);
                SetToCache(users);
                data.AddRange(users);
            }

            _logger.LogTrace("User data loaded");
            return data;
        }

        private void SetToCache(IEnumerable<IUser<string>> userData)
        {
            _logger.LogTrace("Set user data to cache");
            foreach (var data in userData)
            {
                _memoryCache.Set(GetCacheKey(data.Id), data);
            }
        }

        private IPBApiClient? _apiClient;

        private IPBApiClient GetApiClient()
        {
            return _apiClient ??= _clientFactory.GetReadOnlyClient();
        }
    }
}
