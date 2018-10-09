using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Providers;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace BioEngine.Extra.IPB.Settings
{
    [SettingsClass(Name = "Публикация на форуме", IsEditable = true)]
    public class IPBSectionSettings : SettingsBase
    {
        [SettingsProperty(Name = "Раздел на форуме", Type = SettingType.Dropdown)]
        public int ForumId { get; set; }
    }

    [UsedImplicitly]
    public class IPBSectionSettingsOptionsResolver : ISettingsOptionsResolver
    {
        private IPBApiClient _apiClient;

        public IPBSectionSettingsOptionsResolver(IPBApiClientFactory apiClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _apiClient =
                apiClientFactory.GetClient(httpContextAccessor.HttpContext.Features.Get<ICurrentUserFeature>().Token);
        }

        public bool CanResolve(SettingsBase settings)
        {
            return settings is IPBSectionSettings;
        }

        public async Task<List<SettingsOption>> Resolve(SettingsBase settings, string property)
        {
            switch (property)
            {
                case "ForumId":
                    var response = await _apiClient.GetForums(1, 1000);
                    var roots = response.Results.Where(f => f.ParentId == null).ToList();
                    var forums = new List<Forum>();
                    foreach (var forum in roots)
                    {
                        FillTree(forum, forums, response.Results.ToList());
                    }

                    return forums.Select(f => new SettingsOption(f.FullName, f.Id, f.Category)).ToList();
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