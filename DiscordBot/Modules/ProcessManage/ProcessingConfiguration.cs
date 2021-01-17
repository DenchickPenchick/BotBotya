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
|© Denis Voitenko                                                       |
_________________________________________________________________________
 */

using Discord.WebSocket;
using DiscordBot.GuildManaging;
using DiscordBot.Modules.ContentManaging;
using DiscordBot.Modules.MusicManaging;
using DiscordBot.Modules.NotificationsManaging;
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
        public List<IModule> GetAllModules() => new List<IModule> { FileModule, GuildModule, RoomModule, ContentModule, NotificationsModule, MusicModule };        
    }
}
