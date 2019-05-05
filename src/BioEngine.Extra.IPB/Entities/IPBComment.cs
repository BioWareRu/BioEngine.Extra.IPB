using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Comments;

namespace BioEngine.Extra.IPB.Entities
{
    public class IPBComment : BaseComment
    {
        [NotMapped] public override string Title { get; set; }
        [NotMapped] public override string Url { get; set; }
        [Required] public int PostId { get; set; }
    }
}
