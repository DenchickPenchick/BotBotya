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

using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.MusicOperations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;

namespace DiscordBot.Modules.MusicManaging
{
    public class MusicModule : IModule
    {
        private readonly DiscordSocketClient Client;
        private readonly LavaNode LavaNode;
        private readonly LavaOperations LavaOperations;

        public MusicModule(IServiceProvider services)
        {
            LavaNode = services.GetRequiredService<LavaNode>();
            Client = services.GetRequiredService<DiscordSocketClient>();
            LavaOperations = services.GetRequiredService<LavaOperations>();
            Client.ReactionAdded += Client_ReactionAdded;
            LavaNode.OnTrackEnded += LavaNode_OnTrackEnded;            
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;            
        }                

        public void RunModule()
        {
            //Метод пустой, т.к. не нужно запускать setup методов для модулей.
        }        
        
        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            IMessage message = await arg1.GetOrDownloadAsync();
            var user = arg3.User.GetValueOrDefault() as SocketGuildUser;           
            if (!user.IsBot && message.Id == LavaOperations.PlayersMessagesCollection[user.Guild].Id)
            {
                LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);

                switch (arg3.Emote.Name)
                {
                    case "⏯️":
                        switch (player.PlayerState)
                        {                            
                            case Victoria.Enums.PlayerState.Playing:
                                await player.PauseAsync();
                                break;
                            case Victoria.Enums.PlayerState.Paused:
                                await player.ResumeAsync();
                                break;                            
                        }
                        break;
                    case "⏹":
                        await player.StopAsync();
                        break;                    
                    case "❌":
                        await ((RestUserMessage)LavaOperations.PlayersMessagesCollection[user.Guild]).DeleteAsync();
                        LavaOperations.PlayersMessagesCollection[user.Guild] = null;
                        break;
                    case "➕":
                        if(player.Volume + 100 <= ushort.MaxValue)
                            await player.UpdateVolumeAsync((ushort)(player.Volume + 10));                        
                        break;
                    case "➖":
                        if(player.Volume - 100 >= ushort.MaxValue)
                            await player.UpdateVolumeAsync((ushort)(player.Volume + 10));                        
                        break;
                }
                await message.RemoveReactionAsync(arg3.Emote, user);
            }
        }

        private async Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            var prevChannel = arg2.VoiceChannel;
            var channel = arg3.VoiceChannel;
            if (arg1.Id == Client.CurrentUser.Id)
            {
                if (prevChannel != null && channel == null)
                {
                    await LavaNode.LeaveAsync(prevChannel);
                    LavaOperations.PlayersMessagesCollection[prevChannel.Guild] = null;
                }

            }
            else if (prevChannel != null && prevChannel.Users.Count == 1)
            {
                await LavaNode.LeaveAsync(prevChannel);
                LavaOperations.PlayersMessagesCollection[prevChannel.Guild] = null;
            }
            
        }

        private async Task LavaNode_OnTrackEnded(TrackEndedEventArgs arg)
        {                                           
            var guild = arg.Player.VoiceState.VoiceChannel.Guild;
            var playerMessage = (RestUserMessage)LavaOperations.PlayersMessagesCollection[guild];
            await playerMessage.DeleteAsync();
            LavaOperations.PlayersMessagesCollection[guild] = null;
        }        
    }
}
