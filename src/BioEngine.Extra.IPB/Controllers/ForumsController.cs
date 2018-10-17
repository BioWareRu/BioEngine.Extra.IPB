using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.API.Response;
using BioEngine.Extra.IPB.Api;
using BioEngine.Extra.IPB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BioEngine.Extra.IPB.Controllers
{
    public class ForumsController : IPBController
    {
        public ForumsController(IPBApiClientFactory factory, ILogger<ForumsController> logger) : base(factory, logger)
        {
        }

        [HttpGet]
        public async Task<ActionResult<ListResponse<Forum>>> GetAsync(int offset = 1, int limit = 25)
        {
            var page = offset / limit + 1;
            var response = await Client.GetForumsAsync(page, limit);
            var roots = response.Results.Where(f => f.ParentId == null).ToList();
            var forums = new List<Forum>();
            foreach (var forum in roots)
            {
                FillTree(forum, forums, response.Results.ToList());
            }

            return new ListResponse<Forum>(forums, forums.Count);
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