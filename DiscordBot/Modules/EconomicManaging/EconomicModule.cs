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
using DiscordBot.Providers;
using System.Threading.Tasks;

namespace DiscordBot.Modules.EconomicManaging
{
    public class EconomicModule : IModule
    {        
        private DiscordSocketClient Client { get; set; }

        public EconomicModule(DiscordSocketClient client)
        {
            Client = client;
            client.MessageReceived += Client_MessageReceived;
            client.Ready += Client_Ready;
        }

        public void RunModule()
        {
            
        }

        private Task Client_Ready()
        {
            foreach (var guild in Client.Guilds)            
                FilesProvider.AddEconomicGuild(guild);                            
            return Task.CompletedTask;
        }

        private Task Client_MessageReceived(SocketMessage arg)
        {
            var user = arg.Author as SocketGuildUser;
            var guild = user.Guild;
            var provider = new EconomicProvider(guild);
            if (provider.EconomicGuild != null)            
                if (provider.EconomicGuild.RewardForMessage > 0)
                    provider.AddBalance(user, provider.EconomicGuild.RewardForMessage);            

            return Task.CompletedTask;
        }
    }
}
