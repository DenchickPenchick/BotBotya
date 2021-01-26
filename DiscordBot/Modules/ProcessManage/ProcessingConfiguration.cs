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

using Discord.WebSocket;
using DiscordBot.GuildManaging;
using DiscordBot.Modules.ContentManaging;
using DiscordBot.Modules.EconomicManaging;
using DiscordBot.Modules.MusicManaging;
using DiscordBot.Modules.NotificationsManaging;
using DiscordBot.Modules.ServersConnectingManaging;
using DiscordBot.Providers.FileManaging;
using DiscordBot.RoomManaging;
using System.Collections.Generic;

namespace DiscordBot.Modules.ProcessManage
{
    public class ProcessingConfiguration
    {
        public DiscordSocketClient DiscordSocketClient { get; set; }
        public RoomModule RoomModule { get; set; }
        public ContentModule ContentModule { get; set; }
        public FilesModule FileModule { get; set; }
        public GuildModule GuildModule { get; set; }
        public LogModule NotificationsModule { get; set; }            
        public MusicModule MusicModule { get; set; }
        public ServersConnector ServersConnector { get; set; }
        public EconomicModule EconomicModule { get; set; }
        public List<IModule> GetAllModules() => new List<IModule> { FileModule, GuildModule, RoomModule, ContentModule, NotificationsModule, MusicModule, ServersConnector, EconomicModule };        
    }
}
