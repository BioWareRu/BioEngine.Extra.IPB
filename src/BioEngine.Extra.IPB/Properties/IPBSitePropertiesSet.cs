using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Properties;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Models;
using JetBrains.Annotations;

namespace BioEngine.Extra.IPB.Properties
{
    [PropertiesSet("Публикация на форуме", IsEditable = true)]
    public class IPBSitePropertiesSet : PropertiesSet
    {
        [PropertiesElement("Включено?", PropertyElementType.Checkbox)]
        public bool IsEnabled { get; set; }

        [PropertiesElement("Раздел на форуме", PropertyElementType.Dropdown)]
        public int ForumId { get; set; }
    }

    [UsedImplicitly]
    public class IPBSectionPropertiesOptionsResolver : IPropertiesOptionsResolver
    {
        private readonly IPBApiClientFactory _apiClientFactory;
        private readonly ICurrentUserProvider<string> _currentUserProvider;
        private IPBApiClient _apiClient;

        public IPBSectionPropertiesOptionsResolver(IPBApiClientFactory apiClientFactory,
            ICurrentUserProvider<string> currentUserProvider)
        {
            _apiClientFactory = apiClientFactory;
            _currentUserProvider = currentUserProvider;
        }

        public bool CanResolve(PropertiesSet properties)
        {
            return properties is IPBSitePropertiesSet;
        }

        private async Task<IPBApiClient> GetClientAsync()
        {
            return _apiClient ??= _apiClientFactory.GetClient(await _currentUserProvider.GetAccessTokenAsync());
        }

        public async Task<List<PropertiesOption>?> ResolveAsync(PropertiesSet properties, string property)
        {
            switch (property)
            {
                case "ForumId":
                    var response = await (await GetClientAsync()).GetForumsAsync(1, 1000);
                    var roots = response.Results.Where(f => f.ParentId == null).ToList();
                    var forums = new List<Forum>();
                    foreach (var forum in roots)
                    {
                        FillTree(forum, forums, response.Results.ToList());
                    }

                    return forums.Select(f => new PropertiesOption(f.FullName, f.Id, f.Category)).ToList();
                default: return null;
            }
        }

        private void FillTree(Forum forum, List<Forum> forums, List<Forum> allForums)
        {
            var children = allForums.Where(f => f.ParentId == forum.Id);
            foreach (var child in children)
            {
                child.Parent = forum;
                FillTree(child, forums, allForums);
                forum.Children.Add(child);
            }

            if (!forum.Children.Any())
            {
                forums.Add(forum);
            }
        }
    }
}
