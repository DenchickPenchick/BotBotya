//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System;

namespace DiscordBot.Serializable
{ 
    [Serializable]
    public class SerializableCategories
    {
        public ulong ContentCategoryId { get; set; } = 0;    
        public ulong VoiceRoomsCategoryId { get; set; } = 0;
    }
}
