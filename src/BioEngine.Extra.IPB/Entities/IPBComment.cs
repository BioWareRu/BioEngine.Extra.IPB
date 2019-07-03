using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Comments;
using BioEngine.Core.DB;

namespace BioEngine.Extra.IPB.Entities
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    [Entity("ipbcomments")]
    public class IPBComment : BaseComment
    {
        [NotMapped] public override string Title { get; set; }
        [Required] public int PostId { get; set; }
        [Required] public int TopicId { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
