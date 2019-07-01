namespace BioEngine.Extra.IPB.Models
{
    public class IpbUser
    {
        public string Id { get; set; }
        public string Name { get; set; } = "";
        public string PhotoUrl { get; set; } = "";
        public string ProfileUrl { get; set; } = "";
        public Group PrimaryGroup { get; set; } = new Group();
        public Group[] SecondaryGroups { get; set; } = new Group[0];
    }
}
