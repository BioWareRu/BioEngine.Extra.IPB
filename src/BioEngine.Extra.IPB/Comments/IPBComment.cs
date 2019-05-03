using System.ComponentModel.DataAnnotations;
using BioEngine.Core.Comments;

namespace BioEngine.Extra.IPB.Comments
{
    public class IPBComment : BaseComment
    {
        [Required] public int PostId { get; set; }
    }
}
