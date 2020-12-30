using System;
using System.Collections.Generic;

namespace DiscordBot.CustomCommands
{
    [Serializable]
    public class CustomCommands
    {
        public List<CustomCommand> Commands { get; set; } = new List<CustomCommand>();
    }
}
