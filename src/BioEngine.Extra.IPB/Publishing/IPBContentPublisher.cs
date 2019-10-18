using System;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Social;
using BioEngine.Core.Web;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Models;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.IPB.Publishing
{
    public class IPBContentPublisher : BaseContentPublisher<IPBPublishConfig, IPBPublishRecord>
    {
        private readonly IPBApiClientFactory _apiClientFactory;
        private readonly IContentRender _contentRender;

        public IPBContentPublisher(IPBApiClientFactory apiClientFactory, IContentRender contentRender,
            BioContext dbContext,
            ILogger<IPBContentPublisher> logger) : base(dbContext, logger)
        {
            _apiClientFactory = apiClientFactory;
            _contentRender = contentRender;
        }

        protected override async Task<IPBPublishRecord> DoPublishAsync(IPBPublishRecord record, IContentItem entity,
            Site site,
            IPBPublishConfig config)
        {
            return await CreateOrUpdateContentPostAsync(record, entity, site, config);
        }

        protected override async Task<bool> DoDeleteAsync(IPBPublishRecord record, IPBPublishConfig config)
        {
            var apiClient = _apiClientFactory.GetPublishClient();
            var result = await apiClient.PostAsync<TopicCreateModel, Topic>(
                $"forums/topics/{record.TopicId.ToString()}",
                new TopicCreateModel {Hidden = 1});
            return result.Hidden;
        }

        private async Task<IPBPublishRecord> CreateOrUpdateContentPostAsync(IPBPublishRecord record, IContentItem item,
            Site site,
            IPBPublishConfig config)
        {
            if (_contentRender == null)
            {
                throw new ArgumentException("No content renderer is registered!");
            }

            var apiClient = _apiClientFactory.GetPublishClient();

            if (record.TopicId == 0)
            {
                var topic = new TopicCreateModel
                {
                    Forum = config.ForumId,
                    Title = item.Title,
                    Hidden = !item.IsPublished ? 1 : 0,
                    Post = await _contentRender.RenderHtmlAsync(item, site),
                    Author = int.Parse(config.AuthorId)
                };
                var createdTopic = await apiClient.PostAsync<TopicCreateModel, Topic>("forums/topics", topic);
                if (createdTopic.FirstPost != null)
                {
                    record.TopicId = createdTopic.Id;
                    record.PostId = createdTopic.FirstPost.Id;
                }
            }
            else
            {
                await apiClient.PostAsync<TopicCreateModel, Topic>(
                    $"forums/topics/{record.TopicId.ToString()}",
                    new TopicCreateModel
                    {
                        Title = item.Title,
                        Hidden = !item.IsPublished ? 1 : 0,
                        Author = int.Parse(config.AuthorId),
                        Forum = config.ForumId,
                        Post = await _contentRender.RenderHtmlAsync(item, site)
                    });
            }

            return record;
        }
    }
}
