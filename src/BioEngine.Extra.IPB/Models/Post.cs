using System;

namespace BioEngine.Extra.IPB.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public User Author { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public bool Hidden { get; set; }
        public string Url { get; set; }
    }

    public class PostCreateModel
    {
        public string Post { get; set; }
    }
}