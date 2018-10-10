using BioEngine.Core.Providers;

namespace BioEngine.Extra.IPB.Settings
{
    [SettingsClass(Name = "Публикация на форуме")]
    public class IPBContentSettings : SettingsBase
    {
        [SettingsProperty(Name = "Тема", Type = SettingType.Number)]
        public int TopicId { get; set; }

        [SettingsProperty(Name = "Пост", Type = SettingType.Number)]
        public int PostId { get; set; }
    }
}