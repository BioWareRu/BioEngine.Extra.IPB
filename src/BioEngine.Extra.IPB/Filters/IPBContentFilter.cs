using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Properties;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Extra.IPB.Filters
{
    public class IPBContentFilter : BaseRepositoryFilter
    {
        private readonly IPBApiClient _apiClient;
        private readonly PropertiesProvider _propertiesProvider;
        private readonly BioContext _bioContext;

        public IPBContentFilter(IPBApiClientFactory apiClientFactory, IHttpContextAccessor httpContextAccessor,
            PropertiesProvider propertiesProvider, BioContext bioContext)
        {
            _apiClient =
                apiClientFactory.GetClient(httpContextAccessor.HttpContext.Features.Get<ICurrentUserFeature>().Token);
            _propertiesProvider = propertiesProvider;
            _bioContext = bioContext;
        }

        public override bool CanProcess(Type type)
        {
            return typeof(Post).IsAssignableFrom(type);
        }

        public override async Task<bool> AfterSaveAsync<T>(T item, PropertyChange[] changes = null)
        {
            var content = item as Post;

            if (content != null)
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

                return await _apiClient.CreateOrUpdateContentPostAsync(content, forumId);
            }

            return true;
        }
    }
}
