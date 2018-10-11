using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Providers;
using BioEngine.Extra.IPB.Models;
using BioEngine.Extra.IPB.Settings;
using Flurl.Http;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BioEngine.Extra.IPB.Api
{
    [UsedImplicitly]
    public class IPBApiClientFactory
    {
        private readonly IOptions<IPBApiConfig> _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IPBApiClient> _logger;

        public IPBApiClientFactory(IOptions<IPBApiConfig> options, IServiceProvider serviceProvider, ILogger<IPBApiClient> logger)
        {
            _options = options;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IPBApiClient GetClient(string token)
        {
            var scopeServiceProvider = _serviceProvider.CreateScope().ServiceProvider;
            return new IPBApiClient(_options.Value, token,
                scopeServiceProvider.GetRequiredService<SettingsProvider>(),
                scopeServiceProvider.GetRequiredService<IContentRender>(), _logger);
        }
    }

    public class IPBApiClient
    {
        private readonly IPBApiConfig _apiConfig;
        private readonly string _token;
        private readonly SettingsProvider _settingsProvider;
        private readonly IContentRender _contentRender;
        private readonly ILogger<IPBApiClient> _logger;

        public IPBApiClient(IPBApiConfig apiConfig, string token, SettingsProvider settingsProvider,
            IContentRender contentRender, ILogger<IPBApiClient> logger)
        {
            _apiConfig = apiConfig;
            _token = token;
            _settingsProvider = settingsProvider;
            _contentRender = contentRender;
            _logger = logger;
        }

        public Task<User> GetUser()
        {
            return Get<User>("core/me");
        }

        private IFlurlRequest GetRequest(string url)
        {
            return $"{_apiConfig.ApiUrl}/{url}".WithOAuthBearerToken(_token);
        }


        private Task<T> Get<T>(string url)
        {
            return GetRequest(url).GetJsonAsync<T>();
        }

        private async Task<TResponse> Post<TRequest, TResponse>(string url, TRequest item)
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

        public Task<Response<Forum>> GetForums(int page = 1, int perPage = 25)
        {
            return Get<Response<Forum>>($"forums/forums?page={page}&perPage={perPage}");
        }

        public async Task<bool> CreateOrUpdateContentPost(ContentItem item, int forumId)
        {
            var contentSettings = await _settingsProvider.Get<IPBContentSettings>(item);

            if (contentSettings.TopicId == 0)
            {
                var topic = new TopicCreateModel
                {
                    Forum = forumId,
                    Title = item.Title,
                    Hidden = !item.IsPublished ? 1 : 0,
                    Pinned = item.IsPinned ? 1 : 0,
                    Post = await _contentRender.RenderHtml(item)
                };
                var createdTopic = await Post<TopicCreateModel, Topic>("forums/topics", topic);
                contentSettings.TopicId = createdTopic.Id;
                contentSettings.PostId = createdTopic.FirstPost.Id;
            }
            else
            {
                var topic = await Post<TopicCreateModel, Topic>($"forums/topics/{contentSettings.TopicId}",
                    new TopicCreateModel
                    {
                        Title = item.Title,
                        Hidden = !item.IsPublished ? 1 : 0,
                        Pinned = item.IsPinned ? 1 : 0
                    });

                await Post<PostCreateModel, Post>($"forums/posts/{topic.FirstPost.Id}", new PostCreateModel
                {
                    Post = await _contentRender.RenderHtml(item)
                });
            }

            await _settingsProvider.Set(contentSettings, item);

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