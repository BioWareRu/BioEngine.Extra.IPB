using BioEngine.Core.Properties;

namespace BioEngine.Extra.IPB.Properties
{
    [PropertiesSet(Name = "Публикация на форуме")]
    public class IPBContentPropertiesSet : PropertiesSet
    {
        [PropertiesElement(Name = "Тема", Type = PropertyElementType.Number)]
        public int TopicId { get; set; }

        [PropertiesElement(Name = "Пост", Type = PropertyElementType.Number)]
        public int PostId { get; set; }
    }
}