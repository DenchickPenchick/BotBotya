using System;

namespace DiscordBot.Modules.FileManaging
{
    [Serializable]
    public class SerializableGuild
    {
        public ulong GuildId { get; set; } = default;
        public ulong DefaultRoleId { get; set; } = default;
        public ulong LoggerId { get; set; } = default;
        public ulong UsersWidgetId { get; set; } = default;
        public ulong ChannelsWidgetId { get; set; } = default;
        public bool HelloMessageEnable { get; set; } = default;
        public bool ContentEnable { get; set; } = default;
        public bool RoomsEnable { get; set; } = default;
        public bool CheckingContent { get; set; } = default;
        public bool GuildNotifications { get; set; } = default;
        public string HelloMessage { get; set; } = default;
        public string EmojiOfRoom { get; set; } = default;
        public string Prefix { get; set; } = default;
        public SerializableCategories SystemCategories { get; set; } = default;
        public SerializableChannels SystemChannels { get; set; } = default;
    }
}
