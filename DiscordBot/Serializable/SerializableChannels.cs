using System;

namespace DiscordBot.Modules.FileManaging
{
    [Serializable]
    public class SerializableChannels
    {
        public ulong LinksChannelId { get; set; }
        public ulong VideosChannelId { get; set; }
        public ulong CreateRoomChannelId { get; set; }                
    }
}
