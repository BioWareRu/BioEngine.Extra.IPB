using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BioEngine.Extra.IPB.Auth;
using BioEngine.Extra.IPB.Models;
using Flurl.Http;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BioEngine.Extra.IPB.Api
{
    [UsedImplicitly]
    public class IPBApiClientFactory
    {
        private readonly IPBModuleConfig _options;
        private readonly ILogger<IPBApiClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public IPBApiClientFactory(IPBModuleConfig options, ILogger<IPBApiClient> logger,
            IHttpClientFactory httpClientFactory)
        {
            _options = options;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public IPBApiClient GetClient(string token)
        {
            return new IPBApiClient(_options, token, null, _logger, _httpClientFactory);
        }

        public IPBApiClient GetReadOnlyClient()
        {
            if (string.IsNullOrEmpty(_options.ApiReadonlyKey))
            {
                throw new Exception("Api readonly key don't configured");
            }

            return new IPBApiClient(_options, null, _options.ApiReadonlyKey, _logger, _httpClientFactory);
        }

        public IPBApiClient GetPublishClient()
        {
            if (string.IsNullOrEmpty(_options.ApiPublishKey))
            {
                throw new Exception("Api publish key don't configured");
            }

            return new IPBApiClient(_options, null, _options.ApiPublishKey, _logger, _httpClientFactory);
        }
    }

    public class IPBApiClient
    {
        private readonly IPBModuleConfig _config;
        private readonly string? _token;
        private readonly string? _apiKey;
        private readonly ILogger<IPBApiClient> _logger;
        private readonly FlurlClient _flurlClient;

        public IPBApiClient(IPBModuleConfig config, string? token, string? apiKey, ILogger<IPBApiClient> logger,
            IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _token = token;
            _apiKey = apiKey;
            _logger = logger;
            _flurlClient = new FlurlClient(httpClientFactory.CreateClient());
        }

        public Task<User> GetUserAsync()
        {
            return GetAsync<User>("core/me");
        }

        private IFlurlRequest GetRequest(string url)
        {
            var requestUrl = new FlurlRequest($"{_config.ApiUrl}/{url}").WithClient(_flurlClient);
            if (!string.IsNullOrEmpty(_token))
            {
                requestUrl.WithOAuthBearerToken(_token);
            }
            else
            {
                if (!string.IsNullOrEmpty(_apiKey))
                {
                    requestUrl.SetQueryParam("key", _apiKey);
                }
                else
                {
                    throw new Exception("No token and no key for ipb client");
                }
            }

            return requestUrl;
        }


        private Task<T> GetAsync<T>(string url)
        {
            return GetRequest(url).GetJsonAsync<T>();
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest item)
        {
            try
            {
                if (item != null)
                {
                    var data = item.ToKeyValue();
                    var response = await GetRequest(url).PostUrlEncodedAsync(data);
                    var json = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<TResponse>(json);
                    }

                    throw new IPBApiException(response.StatusCode, JsonConvert.DeserializeObject<IPBApiError>(json));
                }

                throw new IPBApiException(HttpStatusCode.BadRequest, new IPBApiError {ErrorMessage = "Empty request"});
            }
            catch (FlurlHttpException ex)
            {
                _logger.LogError(ex, ex.ToString());
                throw;
            }
        }

        public Task<Response<Forum>> GetForumsAsync(int page = 1, int perPage = 25)
        {
            return GetAsync<Response<Forum>>($"forums/forums?page={page.ToString()}&perPage={perPage.ToString()}");
        }

        public Task<User> GetUserByIdAsync(string id)
        {
            return GetAsync<User>($"core/members/{id}");
        }

        public Task<Topic> GetTopicAsync(int topicId)
        {
            return GetAsync<Topic>($"forums/topics/{topicId.ToString()}");
        }

        public Task<Response<Post>> GetForumsPostsAsync(int[] forumIds, string orderBy = null,
            bool orderDescending = false, int page = 1, int perPage = 100)
        {
            if (forumIds == null || forumIds.Length == 0)
            {
                throw new ArgumentException(nameof(forumIds));
            }

            var url = $"forums/posts?page={page.ToString()}&perPage={perPage.ToString()}";
            if (!string.IsNullOrEmpty(orderBy))
            {
                url += $"&sortBy={orderBy}";
            }

            if (orderDescending)
            {
                url += "&sortDir=desc";
            }

            url += $"&forums={string.Join(',', forumIds)}";

            return GetAsync<Response<Post>>(url);
        }

        public Task<Response<Post>> GetTopicPostsAsync(int topicId, int page = 1, int perPage = 25)
        {
            return GetAsync<Response<Post>>(
                $"forums/topics/{topicId.ToString()}/posts?page={page.ToString()}&perPage={perPage.ToString()}");
        }
    }

    public static class IPBApiClientHelper
    {
        public static IDictionary<string, string>? ToKeyValue(this object metaToken)
        {
            if (!(metaToken is JToken token))
            {
                return ToKeyValue(JObject.FromObject(metaToken));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = child.ToKeyValue();
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                            .ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue.Type == JTokenType.Date
                ? jValue.ToString("o", CultureInfo.InvariantCulture)
                : jValue.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> {{token.Path.ToLowerInvariant(), value}};
        }
    }
}
