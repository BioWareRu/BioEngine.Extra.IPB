using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using BioEngine.Core.Users;
using BioEngine.Core.Web;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Entities;
using BioEngine.Extra.IPB.Models;
using BioEngine.Extra.IPB.Properties;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Post = BioEngine.Core.Entities.Post;

namespace BioEngine.Extra.IPB.Filters
{
    public class IPBContentHook : BaseRepositoryHook
    {
        private readonly IPBApiClient _apiClient;
        private readonly PropertiesProvider _propertiesProvider;
        private readonly BioContext _bioContext;
        private readonly IContentRender _contentRender;

        public IPBContentHook(IPBApiClientFactory apiClientFactory, IHttpContextAccessor httpContextAccessor,
            PropertiesProvider propertiesProvider, BioContext bioContext, IContentRender contentRender)
        {
            _apiClient =
                apiClientFactory.GetClient(httpContextAccessor.HttpContext.Features.Get<ICurrentUserFeature>().Token);
            _propertiesProvider = propertiesProvider;
            _bioContext = bioContext;
            _contentRender = contentRender;
        }

        public override bool CanProcess(Type type)
        {
            return typeof(Post).IsAssignableFrom(type);
        }

        public override async Task<bool> AfterSaveAsync<T>(T item, PropertyChange[]? changes = null,
            IBioRepositoryOperationContext? operationContext = null)
        {
            if (item is Post content)
            {
                var forumId = 0;
                if (content.SectionIds.Length == 1)
                {
                    var section = await _bioContext.Set<Section>().Where(s => s.Id == content.SectionIds.First())
                        .FirstOrDefaultAsync();
                    if (section != null)
                    {
                        var sectionPropertiesSet = await _propertiesProvider.GetAsync<IPBSectionPropertiesSet>(section);

                        forumId = sectionPropertiesSet.ForumId;
                    }
                }
                else
                {
                    // some forum id from site properties?
                    forumId = 0;
                }

                if (forumId == 0)
                {
                    return false;
                }

                return await CreateOrUpdateContentPostAsync(content, forumId);
            }

            return true;
        }

        public async Task<bool> CreateOrUpdateContentPostAsync(Post item, int forumId)
        {
            if (_contentRender == null)
            {
                throw new ArgumentException("No content renderer is registered!");
            }

            //var contentPropertiesSet = await _propertiesProvider.GetAsync<IPBContentPropertiesSet>(item);
            var contentSettings = await _bioContext.Set<IPBContentSettings>()
                                      .Where(s => s.Type == item.GetType().FullName && s.ContentId == item.Id)
                                      .FirstOrDefaultAsync() ?? new IPBContentSettings
                                  {
                                      ContentId = item.Id, Type = item.GetType().FullName
                                  };

            if (contentSettings.TopicId == 0)
            {
                var topic = new TopicCreateModel
                {
                    Forum = forumId,
                    Title = item.Title,
                    Hidden = !item.IsPublished ? 1 : 0,
                    Pinned = item.IsPinned ? 1 : 0,
                    Post = await _contentRender.RenderHtmlAsync(item)
                };
                var createdTopic = await _apiClient.PostAsync<TopicCreateModel, Topic>("forums/topics", topic);
                if (createdTopic.FirstPost != null)
                {
                    contentSettings.TopicId = createdTopic.Id;
                    contentSettings.PostId = createdTopic.FirstPost.Id;
                }
            }
            else
            {
                var topic = await _apiClient.PostAsync<TopicCreateModel, Topic>(
                    $"forums/topics/{contentSettings.TopicId.ToString()}",
                    new TopicCreateModel
                    {
                        Title = item.Title, Hidden = !item.IsPublished ? 1 : 0, Pinned = item.IsPinned ? 1 : 0
                    });
                if (topic.FirstPost != null)
                {
                    await _apiClient.PostAsync<PostCreateModel, Models.Post>(
                        $"forums/posts/{topic.FirstPost.Id.ToString()}",
                        new PostCreateModel {Post = await _contentRender.RenderHtmlAsync(item)});
                }
            }

            if (contentSettings.Id == Guid.Empty)
            {
                _bioContext.Add(contentSettings);
            }
            else
            {
                _bioContext.Update(contentSettings);
            }

            await _bioContext.SaveChangesAsync();

            return true;
        }
    }
}
