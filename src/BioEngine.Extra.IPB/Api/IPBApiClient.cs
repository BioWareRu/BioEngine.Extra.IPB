using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BioEngine.Extra.IPB.Models;
using Flurl.Http;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BioEngine.Extra.IPB.Api
{
    [UsedImplicitly]
    public class IPBApiClientFactory
    {
        private readonly IOptions<IPBConfig> _options;
        private readonly ILogger<IPBApiClient> _logger;

        public IPBApiClientFactory(IOptions<IPBConfig> options, ILogger<IPBApiClient> logger)
        {
            _options = options;
            _logger = logger;
        }

        public IPBApiClient GetClient(string token)
        {
            return new IPBApiClient(_options.Value, token, _logger);
        }

        public IPBApiClient GetReadOnlyClient()
        {
            return new IPBApiClient(_options.Value, null, _logger);
        }
    }

    public class IPBApiClient
    {
        private readonly IPBConfig _config;
        private readonly string? _token;
        private readonly ILogger<IPBApiClient> _logger;

        public IPBApiClient(IPBConfig config, string? token, ILogger<IPBApiClient> logger)
        {
            _config = config;
            _token = token;
            _logger = logger;
        }

        public Task<User> GetUserAsync()
        {
            return GetAsync<User>("core/me");
        }

        private IFlurlRequest GetRequest(string url)
        {
            var requestUrl = new FlurlRequest($"{_config.ApiUrl}/{url}");
            if (!string.IsNullOrEmpty(_token))
            {
                requestUrl.WithOAuthBearerToken(_token);
            }
            else
            {
                requestUrl.SetQueryParam("key", _config.ReadOnlyKey);
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

        public Task<User> GetUserByIdAsync(int id)
        {
            return GetAsync<User>($"core/members/{id.ToString()}");
        }

        public Task<Topic> GetTopicAsync(int topicId)
        {
            return GetAsync<Topic>($"forums/topics/{topicId.ToString()}");
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
