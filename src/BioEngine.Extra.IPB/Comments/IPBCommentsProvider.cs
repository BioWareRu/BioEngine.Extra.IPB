using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Comments;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Users;
using BioEngine.Extra.IPB.Entities;
using BioEngine.Extra.IPB.Publishing;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.IPB.Comments
{
    [UsedImplicitly]
    public class IPBCommentsProvider : BaseCommentsProvider<string>
    {
        private readonly IPBModuleConfig _options;

        public IPBCommentsProvider(BioContext dbContext,
            ILogger<IPBCommentsProvider> logger,
            IPBModuleConfig options,
            IUserDataProvider<string> userDataProvider)
            : base(dbContext, userDataProvider, logger)
        {
            _options = options;
        }


        protected override IQueryable<BaseComment<string>> GetDbSet()
        {
            return DbContext.Set<IPBComment>();
        }

        [SuppressMessage("ReSharper", "RCS1198")]
        public override async Task<Dictionary<Guid, Uri?>> GetCommentsUrlAsync(IContentItem[] entities, Site site)
        {
            var types = entities.Select(e => e.GetKey()).Distinct().ToArray();
            var ids = entities.Select(e => e.Id).ToArray();

            var contentSettings = await DbContext.Set<IPBPublishRecord>()
                .Where(s => types.Contains(s.Type) && ids.Contains(s.ContentId) && s.SiteIds.Contains(site.Id))
                .ToListAsync();

            var result = new Dictionary<Guid, Uri?>();
            foreach (var entity in entities)
            {
                Uri? uri = null;
                var settings = contentSettings.FirstOrDefault(c =>
                    c.Type == entity.GetKey() && c.ContentId == entity.Id);
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

        public override Task<BaseComment<string>> AddCommentAsync(IContentItem entity, string text, string authorId,
            Guid? replyTo = null)
        {
            throw new NotImplementedException();
        }

        public override Task<BaseComment<string>> UpdateCommentAsync(IContentItem entity, Guid commentId, string text)
        {
            throw new NotImplementedException();
        }
    }
}
