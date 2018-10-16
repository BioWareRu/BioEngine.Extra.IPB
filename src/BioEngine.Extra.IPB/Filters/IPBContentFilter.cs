using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Repository;
using BioEngine.Core.Settings;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Extra.IPB.Filters
{
    public class IPBContentFilter : BaseRepositoryFilter
    {
        private readonly IPBApiClient _apiClient;
        private readonly SettingsProvider _settingsProvider;
        private readonly BioContext _bioContext;

        public IPBContentFilter(IPBApiClientFactory apiClientFactory, IHttpContextAccessor httpContextAccessor,
            SettingsProvider settingsProvider, BioContext bioContext)
        {
            _apiClient =
                apiClientFactory.GetClient(httpContextAccessor.HttpContext.Features.Get<ICurrentUserFeature>().Token);
            _settingsProvider = settingsProvider;
            _bioContext = bioContext;
        }

        public override bool CanProcess(Type type)
        {
            return typeof(ContentItem).IsAssignableFrom(type);
        }

        public override async Task<bool> AfterSave<T, TId>(T item, PropertyChange[] changes = null)
        {
            var content = item as ContentItem;

            if (content != null)
            {
                var forumId = 0;
                if (content.SectionIds.Length == 1)
                {
                    var section = await _bioContext.Set<Section>().Where(s => s.Id == content.SectionIds.First())
                        .FirstOrDefaultAsync();
                    if (section != null)
                    {
                        var settings = await _settingsProvider.Get<IPBSectionSettings>(section);

                        forumId = settings.ForumId;
                    }
                }
                else
                {
                    // some forum id from setting
                    forumId = 0;
                }

                if (forumId == 0)
                {
                    return false;
                }

                return await _apiClient.CreateOrUpdateContentPost(content, forumId);
            }

            return true;
        }
    }
}