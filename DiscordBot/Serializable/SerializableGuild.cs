using System;

namespace DiscordBot.Modules.FileManaging
{
    [Serializable]
    public class SerializableGuild
    {
        public ulong GuildId { get; set; } = 0;
        public ulong DefaultRoleId { get; set; } = 0;
        public ulong LoggerId { get; set; } = 0;        
        public bool HelloMessageEnable { get; set; } = false;        
        public bool CheckingContent { get; set; } = false;
        public bool GuildNotifications { get; set; } = false;
        public string HelloMessage { get; set; } = null;
        public string EmojiOfRoom { get; set; } = "🎤";
        public string Prefix { get; set; } = "!";
        public SerializableCategories SystemCategories { get; set; } = new SerializableCategories();
        public SerializableChannels SystemChannels { get; set; } = new SerializableChannels();
    }
}
