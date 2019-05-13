using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Comments;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.IPB.Comments
{
    [UsedImplicitly]
    public class IPBCommentsProvider : BaseCommentsProvider
    {
        private readonly IPBModuleConfig _options;

        public IPBCommentsProvider(BioContext dbContext,
            ILogger<ICommentsProvider> logger,
            IPBModuleConfig options,
            IUserDataProvider userDataProvider)
            : base(dbContext, userDataProvider, logger)
        {
            _options = options;
        }


        protected override IQueryable<BaseComment> GetDbSet()
        {
            return DbContext.Set<IPBComment>();
        }

        [SuppressMessage("ReSharper", "RCS1198")]
        public override async Task<Dictionary<Guid, Uri?>> GetCommentsUrlAsync(IContentEntity[] entities)
        {
            var types = entities.Select(e => e.GetType().FullName).Distinct().ToArray();
            var ids = entities.Select(e => e.Id).ToArray();

            var contentSettings = await DbContext.Set<IPBContentSettings>()
                .Where(s => types.Contains(s.Type) && ids.Contains(s.ContentId))
                .ToListAsync();

            var result = new Dictionary<Guid, Uri?>();
            foreach (var entity in entities)
            {
                Uri? uri = null;
                var settings = contentSettings.FirstOrDefault(c =>
                    c.Type == entity.GetType().FullName && c.ContentId == entity.Id);
                if (settings?.TopicId > 0)
                {
                    uri = new Uri(
                        $"{_options.Url}topic/{settings.TopicId.ToString()}/?do=getNewComment",
                        UriKind.Absolute);
                }

                result.Add(entity.Id, uri);
            }

            return result;
        }
    }
}
