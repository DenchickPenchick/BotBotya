using System;

namespace DiscordBot.Modules.FileManaging
{ 
    [Serializable]
    public class SerializableCategories
    {        
        public ulong ContentCategoryId { get; set; }     
        public ulong VoiceRoomsCategoryId { get; set; }        
    }
}
