using System;

namespace DiscordBot.Modules.FileManaging
{
    [Serializable]
    public class SerializableGuild
    {
        public ulong GuildId { get; set; } = default;
        public ulong DefaultRoleId { get; set; } = default;
        public ulong LoggerId { get; set; } = default;        
        public bool HelloMessageEnable { get; set; } = default;        
        public bool CheckingContent { get; set; } = default;
        public bool GuildNotifications { get; set; } = default;
        public string HelloMessage { get; set; } = default;
        public string EmojiOfRoom { get; set; } = default;
        public string Prefix { get; set; } = "!";
        public SerializableCategories SystemCategories { get; set; } = new SerializableCategories();
        public SerializableChannels SystemChannels { get; set; } = new SerializableChannels();
    }
}
