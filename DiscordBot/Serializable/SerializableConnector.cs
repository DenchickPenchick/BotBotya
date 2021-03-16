//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System.Collections.Generic;

namespace DiscordBot.Serializable
{
    public class SerializableConnector
    {
        public ulong HostId { get; set; } = 0;
        public List<ulong> EndPointsId { get; set; } = new List<ulong>();
    }
}