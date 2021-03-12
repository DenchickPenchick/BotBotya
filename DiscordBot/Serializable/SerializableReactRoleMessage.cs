//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System.Collections.Generic;

namespace DiscordBot.Serializable
{
    public class SerializableReactRoleMessage
    {
        public ulong Id { get; set; } = 0;
        public List<(string, ulong)> EmojiesRoleId { get; set; } = new List<(string, ulong)>();
    }
}