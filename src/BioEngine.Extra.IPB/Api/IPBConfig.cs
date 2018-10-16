using System;

namespace BioEngine.Extra.IPB.Api
{
    public class IPBConfig
    {
        public Uri Url { get; set; }
        public Uri ApiUrl { get; set; }
        public bool DevMode { get; set; }
        public int AdminGroupId { get; set; }
        public int PublisherGroupId { get; set; }
        public int EditorGroupId { get; set; }
        public string ClientId { get; set; }
        public string ReadOnlyKey { get; set; }
    }
}