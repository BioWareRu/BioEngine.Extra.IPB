using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BioEngine.Extra.IPB.Users
{
    public class IPBUserDataProvider : IUserDataProvider
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

        private static string GetCacheKey(int userId)
        {
            return $"ipbuserdata{userId}";
        }

        private List<UserData> GetFromCache(IEnumerable<int> userIds)
        {
            _logger.LogTrace("Get user data from cache");
            return userIds.Select(GetCacheKey).Select(key => _memoryCache.Get<UserData>(key))
                .Where(userData => userData != null).ToList();
        }

        public async Task<List<UserData>> GetDataAsync(int[] userIds)
        {
            var data = GetFromCache(userIds);
            var notFoundUserIds = userIds.Where(id => data.All(ud => ud.Id != id)).ToArray();
            if (notFoundUserIds.Length > 0)
            {
                _logger.LogTrace("Load users data from api");
                var tasks = notFoundUserIds.Select(id => GetApiClient().GetUserByIdAsync(id));
                var users = await Task.WhenAll(tasks);
                var userData = users.Select(user => new UserData
                {
                    Id = user.Id,
                    Name = user.Name,
                    ProfileLink = new Uri(user.ProfileUrl),
                    AvatarLink = new Uri(user.PhotoUrl)
                }).ToList();
                SetToCache(userData);
                data.AddRange(userData);
            }

            _logger.LogTrace("User data loaded");
            return data;
        }

        private void SetToCache(IEnumerable<UserData> userData)
        {
            _logger.LogTrace("Set user data to cache");
            foreach (var data in userData)
            {
                _memoryCache.Set(GetCacheKey(data.Id), data);
            }
        }

        private IPBApiClient _apiClient;

        private IPBApiClient GetApiClient()
        {
            return _apiClient ?? (_apiClient = _clientFactory.GetReadOnlyClient());
        }
    }
}