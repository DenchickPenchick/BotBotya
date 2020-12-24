using System;

namespace DiscordBot.Modules.FileManaging
{
    [Serializable]
    public class SerializableChannels
    {
        public string LinksChannelName { get; set; }
        public string VideosChannelName { get; set; }
        public string CreateRoomChannelName { get; set; }
        public string ConsoleChannelName { get; set; }        
    }
}
