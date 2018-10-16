using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Providers;
using BioEngine.Core.Site.Filters;
using BioEngine.Core.Site.Model;
using BioEngine.Extra.IPB.Settings;

namespace BioEngine.Extra.IPB.Filters
{
    public class IPBPageFilter : IPageFilter
    {
        private readonly SettingsProvider _settingsProvider;

        public IPBPageFilter(SettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
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
            var random = new Random();
            foreach (var entity in entities)
            {
                var contentItem = entity as ContentItem;
                if (contentItem != null)
                {
                    var settings = await _settingsProvider.Get<IPBContentSettings>(entity);
                    // TODO: DO REAL STUFF =)
                    var url = new Uri($"/topic/{settings.TopicId}#{settings.PostId}", UriKind.Relative);
                    var count = random.Next(1, 100);
                    viewModel.AddFeature(new IPBPageFeature(url, count), entity);
                }
            }

            return true;
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
    }
}