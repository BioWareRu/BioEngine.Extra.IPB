using System;
using System.ComponentModel.DataAnnotations;
using BioEngine.Core.Entities;

namespace BioEngine.Extra.IPB.Entities
{
    public class IPBContentSettings : BaseEntity
    {
        [Required] public Guid ContentId { get; set; }
        [Required] public string Type { get; set; }
        [Required] public int TopicId { get; set; }
        [Required] public int PostId { get; set; }
    }
}
