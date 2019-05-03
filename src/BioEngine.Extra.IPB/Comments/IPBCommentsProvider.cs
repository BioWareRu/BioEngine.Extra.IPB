using System;
using System.Threading.Tasks;
using BioEngine.Core.Comments;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Properties;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Properties;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BioEngine.Extra.IPB.Comments
{
    [UsedImplicitly]
    public class IPBCommentsProvider : BaseCommentsProvider
    {
        private readonly PropertiesProvider _propertiesProvider;
        private readonly IPBConfig _options;

        public IPBCommentsProvider(BioContext dbContext,
            ILogger<ICommentsProvider> logger,
            IOptions<IPBConfig> options,
            PropertiesProvider propertiesProvider)
            : base(dbContext, logger)
        {
            _propertiesProvider = propertiesProvider;
            _options = options.Value;
        }


        public override async Task<Uri> GetCommentsUrlAsync(IContentEntity entity)
        {
            var contentPropertiesSet = await _propertiesProvider.GetAsync<IPBContentPropertiesSet>(entity);
            if (contentPropertiesSet.TopicId > 0)
            {
                return new Uri(
                    $"{_options.Url}topic/{contentPropertiesSet.TopicId.ToString()}/?do=getNewComment",
                    UriKind.Absolute);
            }

            return null;
        }
    }
}
