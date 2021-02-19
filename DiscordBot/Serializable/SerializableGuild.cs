/*
_________________________________________________________________________
|                                                                       |
|██████╗░░█████╗░████████╗  ██████╗░░█████╗░████████╗██╗░░░██╗░█████╗░  |
|██╔══██╗██╔══██╗╚══██╔══╝  ██╔══██╗██╔══██╗╚══██╔══╝╚██╗░██╔╝██╔══██╗  |
|██████╦╝██║░░██║░░░██║░░░  ██████╦╝██║░░██║░░░██║░░░░╚████╔╝░███████║  |
|██╔══██╗██║░░██║░░░██║░░░  ██╔══██╗██║░░██║░░░██║░░░░░╚██╔╝░░██╔══██║  |
|██████╦╝╚█████╔╝░░░██║░░░  ██████╦╝╚█████╔╝░░░██║░░░░░░██║░░░██║░░██║  |
|╚═════╝░░╚════╝░░░░╚═╝░░░  ╚═════╝░░╚════╝░░░░╚═╝░░░░░░╚═╝░░░╚═╝░░╚═╝  |
|______________________________________________________________________ |
|Author: Denis Voitenko.                                                |
|GitHub: https://github.com/DenchickPenchick                            |
|DEV: https://dev.to/denchickpenchick                                   |
|_____________________________Project__________________________________ |
|GitHub: https://github.com/DenchickPenchick/BotBotya                   |
|______________________________________________________________________ |
|© Copyright 2021 Denis Voitenko                                        |
|© Copyright 2021 All rights reserved                                   |
|License: http://opensource.org/licenses/MIT                            |
_________________________________________________________________________
*/

using System.Collections.Generic;

namespace DiscordBot.Serializable
{    
    public class SerializableGuild
    {
        public ulong GuildId { get; set; } = 0;
        public ulong DefaultRoleId { get; set; } = 0;
        public ulong LoggerId { get; set; } = 0;
        public int MaxWarns { get; set; } = 5;
        public bool KickForWarns { get; set; } = false;
        public bool BanForWarns { get; set; } = false;
        public bool MuteForWarns { get; set; } = false;
        public bool WarnsForBadWords { get; set; } = false;
        public bool WarnsForInviteLink { get; set; } = false;
        public bool HelloMessageEnable { get; set; } = false;        
        public bool CheckingContent { get; set; } = false;
        public bool UnknownCommandMessage { get; set; } = false;
        public bool GuildNotifications { get; set; } = false;
        public bool CheckingBadWords { get; set; } = false;
        public string HelloMessage { get; set; } = null;        
        public string EmojiOfRoom { get; set; } = "🎤";
        public string Prefix { get; set; } = "!";
        public string EmbedColor { get; set; } = "Синий";
        public List<ulong> CommandsChannels { get; set; } = new List<ulong>();
        public List<string> BadWords { get; set; } = new List<string>();
        public List<(ulong, int)> BadUsers { get; set; } = new List<(ulong, int)>();        
        public SerializableCategories SystemCategories { get; set; } = new SerializableCategories();
        public SerializableChannels SystemChannels { get; set; } = new SerializableChannels();
    }
}
