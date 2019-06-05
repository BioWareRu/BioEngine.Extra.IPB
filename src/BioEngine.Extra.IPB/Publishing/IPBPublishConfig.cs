using BioEngine.Core.Social;

namespace BioEngine.Extra.IPB.Publishing
{
    public class IPBPublishConfig : IContentPublisherConfig
    {
        public IPBPublishConfig(int forumId, string accessToken)
        {
            ForumId = forumId;
            AccessToken = accessToken;
        }

        public int ForumId { get; }
        public string AccessToken { get; }
    }
}
