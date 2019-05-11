using System.Collections.Generic;
using Newtonsoft.Json;

namespace BioEngine.Extra.IPB.Models
{
    public class Forum
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Topics { get; set; }
        public string Url { get; set; } = "";
        [JsonProperty("parent_id")] public int? ParentId { get; set; }

        public List<Forum> Children { get; set; } = new List<Forum>();
        [JsonIgnore] public Forum? Parent { get; set; }
        public string FullName => Parent != null ? $"{Parent.FullName} > {Name}" : Name;
        public string? Category => Parent?.FullName;
    }
}
