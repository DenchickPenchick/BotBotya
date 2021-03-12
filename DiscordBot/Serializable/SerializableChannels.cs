//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System;

namespace DiscordBot.Serializable
{
    [Serializable]
    public class SerializableChannels
    {
        public ulong LinksChannelId { get; set; } = 0;
        public ulong VideosChannelId { get; set; } = 0;
        public ulong NewUsersChannelId { get; set; } = 0;
        public ulong LeaveUsersChannelId { get; set; } = 0;
        public ulong CreateRoomChannelId { get; set; } = 0;
        public ulong LogsChannelId { get; set; } = 0;
    }
}
