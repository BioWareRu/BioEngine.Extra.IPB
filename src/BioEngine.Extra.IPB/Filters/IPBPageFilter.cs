using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Helpers;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Settings;
using BioEngine.Core.Site.Filters;
using BioEngine.Core.Site.Model;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BioEngine.Extra.IPB.Filters
{
    public class IPBPageFilter : IPageFilter
    {
        private readonly SettingsProvider _settingsProvider;
        private readonly IPBApiClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<IPBPageFilter> _logger;
        private readonly IPBConfig _options;

        public IPBPageFilter(SettingsProvider settingsProvider, IOptions<IPBConfig> options,
            IPBApiClientFactory clientFactory, IMemoryCache memoryCache, ILogger<IPBPageFilter> logger)
        {
            _settingsProvider = settingsProvider;
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _logger = logger;
            _options = options.Value;
        }

        private IPBApiClient _apiClient;

        private IPBApiClient GetApiClient()
        {
            return _apiClient ?? (_apiClient = _clientFactory.GetReadOnlyClient());
        }


        public bool CanProcess(Type type)
        {
            return typeof(ContentItem).IsAssignableFrom(type);
        }

        public Task<bool> ProcessPage(PageViewModelContext viewModel)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> ProcessEntities<TEntity, TEntityPk>(PageViewModelContext viewModel,
            IEnumerable<TEntity> entities) where TEntity : class, IEntity<TEntityPk>
        {
            foreach (var entity in entities)
            {
                var contentItem = entity as ContentItem;
                if (contentItem != null)
                {
                    var settings = await _settingsProvider.Get<IPBContentSettings>(entity);
                    if (settings.TopicId > 0)
                    {
                        var url = new Uri($"{_options.Url}topic/{settings.TopicId}/?do=getNewComment",
                            UriKind.Absolute);

                        viewModel.AddFeature(new IPBPageFeature(url, await GetCommentsCount(settings.TopicId)), entity);
                    }
                }
            }

            return true;
        }

        private async Task<int> GetCommentsCount(int topicId)
        {
            var cacheKey = $"ipbCommentsCount{topicId}";
            var count = _memoryCache.Get<int?>(cacheKey);
            if (count == null)
            {
                try
                {
                    var topic = await GetApiClient().GetTopic(topicId);
                    count = topic.Posts - 1; // remove original topic post from comments count
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.ToString());
                    count = null;
                }

                _memoryCache.Set(cacheKey, count, TimeSpan.FromMinutes(1));
            }

            return count ?? 0;
        }
    }

    public class IPBPageFeature
    {
        public IPBPageFeature(Uri commentsUrl, int commentsCount)
        {
            CommentsUrl = commentsUrl;
            CommentsCount = commentsCount;
        }

        public Uri CommentsUrl { get; }
        public int CommentsCount { get; }

        public string CommentsCountString =>
            @"{n, plural,
                    =0 {Обсудить на форуме}
                    one {# комментарий} 
                    few {# комментария} 
                    many {# комментариев} 
                    other {# комментария} 
                }".Pluralize(CommentsCount);
    }
}