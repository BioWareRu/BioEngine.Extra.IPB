using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.Entities;

namespace BioEngine.Extra.IPB.Entities
{
    public class IPBContentSettings : BaseEntity
    {
        [NotMapped] public override string Title { get; set; }
        [NotMapped] public override string Url { get; set; }
        [Required] public Guid ContentId { get; set; }
        [Required] public string Type { get; set; }
        [Required] public int TopicId { get; set; }
        [Required] public int PostId { get; set; }
    }
}
