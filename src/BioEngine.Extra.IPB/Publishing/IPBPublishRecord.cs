using System.ComponentModel.DataAnnotations;
using BioEngine.Core.Social;

namespace BioEngine.Extra.IPB.Publishing
{
    public class IPBPublishRecord : BasePublishRecord
    {
        [Required] public int TopicId { get; set; }
        [Required] public int PostId { get; set; }
    }
}
