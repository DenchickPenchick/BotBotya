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
using DiscordBot.GuildManaging;
using DiscordBot.Modules.ContentManaging;
using DiscordBot.Modules.EconomicManaging;
using DiscordBot.Modules.MusicManaging;
using DiscordBot.Modules.NotificationsManaging;
using DiscordBot.Modules.ServersConnectingManaging;
using DiscordBot.Providers.FileManaging;
using DiscordBot.RoomManaging;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Modules.ProcessManage
{
    public class ProcessingModule : IModule
    {
        private readonly IServiceProvider Modules;

        public ProcessingModule(IServiceProvider modules) 
        {
            Modules = modules;
        }

        public void RunModule()
        {
            Modules.GetRequiredService<FilesModule>().RunModule();
            Modules.GetRequiredService<GuildModule>().RunModule();
            Modules.GetRequiredService<RoomModule>().RunModule();

            var logMod = Modules.GetRequiredService<LogModule>();
            logMod.SetInstanceOfRoomModule(Modules.GetRequiredService<RoomModule>());
            logMod.RunModule();

            Modules.GetRequiredService<MusicModule>().RunModule();
            Modules.GetRequiredService<ServersConnector>().RunModule();
            Modules.GetRequiredService<EconomicModule>().RunModule();
            Modules.GetRequiredService<ContentModule>().RunModule();            
        }
    }
}
