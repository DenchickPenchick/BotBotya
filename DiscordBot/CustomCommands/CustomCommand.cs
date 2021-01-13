using System;
using System.Collections.Generic;

namespace DiscordBot.CustomCommands
{
    [Serializable]
    public class CustomCommand
    {
        public ulong GuildId { get; set; } = 0;
        public string Name { get; set; } = null;
        public List<Action> Actions { get; set; } = new List<Action>();

        public string Message { get; set; }

        public enum Action { Message, Kick, Ban }
    }
}
