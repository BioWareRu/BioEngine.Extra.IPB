using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Comments;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BioEngine.Extra.IPB.Comments
{
    [UsedImplicitly]
    public class IPBCommentsProvider : BaseCommentsProvider
    {
        private readonly IPBConfig _options;

        public IPBCommentsProvider(BioContext dbContext,
            ILogger<ICommentsProvider> logger,
            IOptions<IPBConfig> options,
            IUserDataProvider userDataProvider)
            : base(dbContext, userDataProvider, logger)
        {
            _options = options.Value;
        }


        protected override IQueryable<BaseComment> GetDbSet()
        {
            return DbContext.Set<IPBComment>();
        }

        public override async Task<Uri> GetCommentsUrlAsync(IContentEntity entity)
        {
            var contentSettings = await DbContext.Set<IPBContentSettings>()
                .Where(s => s.Type == entity.GetType().FullName && s.ContentId == entity.Id)
                .FirstOrDefaultAsync();
            if (contentSettings?.TopicId > 0)
            {
                return new Uri(
                    $"{_options.Url}topic/{contentSettings.TopicId.ToString()}/?do=getNewComment",
                    UriKind.Absolute);
            }

            return null;
        }
    }
}
