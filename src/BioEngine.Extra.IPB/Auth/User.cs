using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Extra.IPB.Models;

namespace BioEngine.Extra.IPB.Auth
{
    [Entity("ipbuser")]
    public class User : IUser
    {
        public string Id { get; set; }
        public string Name { get; set; } = "";
        public string PhotoUrl { get; set; } = "";
        public string ProfileUrl { get; set; } = "";
        public Group PrimaryGroup { get; set; } = new Group();
        public Group[] SecondaryGroups { get; set; } = new Group[0];

        public int[] GetGroupIds()
        {
            var groupIds = new List<int> {PrimaryGroup.Id};
            groupIds.AddRange(SecondaryGroups.Select(x => x.Id));
            return groupIds.ToArray();
        }

        public string GetId()
        {
            return Id;
        }
    }
}
