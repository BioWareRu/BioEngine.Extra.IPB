﻿using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.Interfaces;

namespace BioEngine.Extra.IPB.Models
{
    public class User : IUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
        public string ProfileUrl { get; set; }
        public Group PrimaryGroup { get; set; } = new Group();
        public Group[] SecondaryGroups { get; set; } = new Group[0];

        public int[] GetGroupIds()
        {
            var groupIds = new List<int> {PrimaryGroup.Id};
            groupIds.AddRange(SecondaryGroups.Select(x => x.Id));
            return groupIds.ToArray();
        }
    }
}