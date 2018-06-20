using System;

namespace BioEngine.Extra.IPB.Api
{
    public class IPBApiConfig
    {
        public Uri ApiUrl { get; set; }
        public bool DevMode { get; set; }
        public int AdminGroupId { get; set; }
        public int PublisherGroupId { get; set; }
        public int EditorGroupId { get; set; }
        public string ClientId { get; set; }
    }
}