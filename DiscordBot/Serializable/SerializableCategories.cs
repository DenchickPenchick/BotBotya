using System;

namespace DiscordBot.Modules.FileManaging
{ 
    [Serializable]
    public class SerializableCategories
    {
        public string MainTextCategoryName { get; set; }
        public string ContentCategoryName { get; set; }
        public string MainVoiceCategoryName { get; set; }
        public string VoiceRoomsCategoryName { get; set; }
        public string BotCategoryName { get; set; }
    }
}
