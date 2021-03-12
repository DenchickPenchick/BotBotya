//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System.Collections.Generic;

namespace DiscordBot.Serializable
{
    public class SerializableConnectors
    {
        public ulong GuildId { get; set; } = 0;
        public List<SerializableConnector> SerializableConnectorsChannels { get; set; } = new List<SerializableConnector>();
    }
}
