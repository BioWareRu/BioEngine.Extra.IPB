using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Properties;
using BioEngine.Extra.IPB.Models;
using BioEngine.Extra.IPB.Properties;
using Flurl.Http;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Post = BioEngine.Core.Entities.Post;

namespace BioEngine.Extra.IPB.Api
{
    [UsedImplicitly]
    public class IPBApiClientFactory
    {
        private readonly IOptions<IPBConfig> _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IPBApiClient> _logger;

        public IPBApiClientFactory(IOptions<IPBConfig> options, IServiceProvider serviceProvider,
            ILogger<IPBApiClient> logger)
        {
            _options = options;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IPBApiClient GetClient(string token)
        {
            var scopeServiceProvider = _serviceProvider.CreateScope().ServiceProvider;
            return new IPBApiClient(_options.Value, token,
                scopeServiceProvider.GetRequiredService<PropertiesProvider>(),
                scopeServiceProvider.GetService<IContentRender>(), _logger);
        }

        public IPBApiClient GetReadOnlyClient()
        {
            var scopeServiceProvider = _serviceProvider.CreateScope().ServiceProvider;
            return new IPBApiClient(_options.Value, null,
                scopeServiceProvider.GetRequiredService<PropertiesProvider>(),
                scopeServiceProvider.GetService<IContentRender>(), _logger);
        }
    }

    public class IPBApiClient
    {
        private readonly IPBConfig _config;
        [CanBeNull] private readonly string _token;
        private readonly PropertiesProvider _propertiesProvider;
        [CanBeNull] private readonly IContentRender _contentRender;
        private readonly ILogger<IPBApiClient> _logger;

        public IPBApiClient(IPBConfig config, string token, PropertiesProvider propertiesProvider,
            IContentRender contentRender, ILogger<IPBApiClient> logger)
        {
            _config = config;
            _token = token;
            _propertiesProvider = propertiesProvider;
            _contentRender = contentRender;
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

        private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest item)
        {
            try
            {
                var response = await GetRequest(url).PostUrlEncodedAsync(item.ToKeyValue());
                var json = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<TResponse>(json);
                }

                throw new IPBApiException(response.StatusCode, JsonConvert.DeserializeObject<IPBApiError>(json));
            }
            catch (FlurlHttpException ex)
            {
                _logger.LogError(ex, ex.ToString());
                throw;
            }
        }

        public Task<Response<Forum>> GetForumsAsync(int page = 1, int perPage = 25)
        {
            return GetAsync<Response<Forum>>($"forums/forums?page={page}&perPage={perPage}");
        }

        public Task<User> GetUserByIdAsync(int id)
        {
            return GetAsync<User>($"core/members/{id}");
        }

        public Task<Topic> GetTopicAsync(int topicId)
        {
            return GetAsync<Topic>($"forums/topics/{topicId}");
        }

        public async Task<bool> CreateOrUpdateContentPostAsync(Post item, int forumId)
        {
            if (_contentRender == null)
            {
                throw new ArgumentException("No content renderer is registered!");
            }

            var contentPropertiesSet = await _propertiesProvider.GetAsync<IPBContentPropertiesSet>(item);

            if (contentPropertiesSet.TopicId == 0)
            {
                var topic = new TopicCreateModel
                {
                    Forum = forumId,
                    Title = item.Title,
                    Hidden = !item.IsPublished ? 1 : 0,
                    Pinned = item.IsPinned ? 1 : 0,
                    Post = await _contentRender.RenderHtmlAsync(item)
                };
                var createdTopic = await PostAsync<TopicCreateModel, Topic>("forums/topics", topic);
                contentPropertiesSet.TopicId = createdTopic.Id;
                contentPropertiesSet.PostId = createdTopic.FirstPost.Id;
            }
            else
            {
                var topic = await PostAsync<TopicCreateModel, Topic>($"forums/topics/{contentPropertiesSet.TopicId}",
                    new TopicCreateModel
                    {
                        Title = item.Title,
                        Hidden = !item.IsPublished ? 1 : 0,
                        Pinned = item.IsPinned ? 1 : 0
                    });

                await PostAsync<PostCreateModel, Models.Post>($"forums/posts/{topic.FirstPost.Id}", new PostCreateModel
                {
                    Post = await _contentRender.RenderHtmlAsync(item)
                });
            }

            await _propertiesProvider.SetAsync(contentPropertiesSet, item);

            return true;
        }
    }

    public static class IPBApiClientHelper
    {
        public static IDictionary<string, string> ToKeyValue(this object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            JToken token = metaToken as JToken;
            if (token == null)
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