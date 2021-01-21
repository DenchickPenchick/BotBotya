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
