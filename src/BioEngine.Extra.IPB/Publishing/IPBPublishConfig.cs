using BioEngine.Core.Social;

namespace BioEngine.Extra.IPB.Publishing
{
    public class IPBPublishConfig : IContentPublisherConfig
    {
        public IPBPublishConfig(int forumId)
        {
            ForumId = forumId;
        }

        public int ForumId { get; }
    }
}
