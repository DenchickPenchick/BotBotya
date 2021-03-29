//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System.Collections.Generic;

namespace DiscordBot.Serializable
{
    public class SerializableConfig
    {
        public string Token { get; set; } = "NOTSETED";
        public string Path { get; set; } = "NOTSETED";
        public ulong SupportGuildId { get; set; } = 0;
        public ulong AdminId { get; set; } = 0;
        public List<ulong> ConfidantsId { get; set; } = new List<ulong>();
    }
}
