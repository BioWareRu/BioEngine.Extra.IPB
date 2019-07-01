using System;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Extra.IPB.Entities;
using BioEngine.Extra.IPB.Publishing;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BioEngine.Extra.IPB.Controllers
{
    [ApiController]
    [Route("v1/ipb/[controller]")]
    public class CommentsController : Controller
    {
        private readonly BioContext _dbContext;
        private IPBModuleConfig _options;

        public CommentsController(BioContext dbContext, IPBModuleConfig options)
        {
            _dbContext = dbContext;
            _options = options;
        }

        private bool CheckAccess()
        {
            if (!Request.Headers.ContainsKey("X-IPB-KEY"))
            {
                return false;
            }

            if (Request.Headers["X-IPB-KEY"][0] != _options.IntegrationKey)
            {
                return false;
            }

            return true;
        }

        [HttpPost("add")]
        public async Task<ActionResult<bool>> AddCommentAsync([FromBody] CommentData commentData)
        {
            if (!CheckAccess())
            {
                return Forbid();
            }

            var settings = await _dbContext.Set<IPBPublishRecord>().Where(s => s.TopicId == commentData.TopicId)
                .FirstOrDefaultAsync();
            if (settings != null)
            {
                var comment = await _dbContext.Set<IPBComment>().Where(c => c.PostId == commentData.Id)
                                  .FirstOrDefaultAsync() ?? new IPBComment
                              {
                                  ContentId = settings.ContentId,
                                  AuthorId = commentData.AuthorId,
                                  PostId = commentData.Id,
                                  DateAdded = DateTimeOffset.FromUnixTimeSeconds(commentData.Timestamp)
                              };
                comment.DateUpdated = DateTimeOffset.Now;
                if (comment.Id == Guid.Empty)
                {
                    _dbContext.Add(comment);
                }
                else
                {
                    _dbContext.Update(comment);
                }

                var entity = await _dbContext.ContentItems.Where(c => c.Id == settings.ContentId).FirstOrDefaultAsync();

                if (entity != null)
                {
                    comment.SiteIds = entity.SiteIds;
                }

                await _dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        [HttpPost("delete")]
        public async Task<ActionResult<bool>> DeleteCommentAsync([FromBody] CommentData commentData)
        {
            if (!CheckAccess())
            {
                return Forbid();
            }

            var settings = await _dbContext.Set<IPBPublishRecord>().Where(s => s.TopicId == commentData.TopicId)
                .FirstOrDefaultAsync();
            if (settings != null)
            {
                var comment = await _dbContext.Set<IPBComment>().Where(c => c.PostId == commentData.Id)
                    .FirstOrDefaultAsync();
                if (comment != null)
                {
                    _dbContext.Remove(comment);
                }

                await _dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }

    [PublicAPI]
    public class CommentData
    {
        public int Id { get; set; }
        public int TopicId { get; set; }
        public int ForumId { get; set; }
        public string AuthorId { get; set; }
        public long Timestamp { get; set; }
    }
}
