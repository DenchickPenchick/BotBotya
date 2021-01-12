using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.FileWorking;
using DiscordBot.GuildManaging;
using DiscordBot.MusicOperations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Console = Colorful.Console;

namespace DiscordBot.Modules.MusicManaging
{
    public class MusicModule : IModule
    {
        private readonly DiscordSocketClient Client;
        private readonly LavaNode LavaNode;
        private LavaOperations LavaOperations;

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
            
        }        
        
        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            IMessage message = await arg1.GetOrDownloadAsync();
<<<<<<< HEAD
            var user = arg3.User.GetValueOrDefault() as SocketGuildUser;
            ValueTuple<SocketGuild, SocketUserMessage> touple = default;
            foreach (var t in LavaOperations.GuildsPlayers)
                if (user.Guild == t.Item1)
                    touple = t;            
            if (!user.IsBot && message.Id == touple.Item2.Id)
=======
            var user = arg3.User.GetValueOrDefault() as SocketGuildUser;           
            if (!user.IsBot && message.Id == LavaOperations.PlayersMessagesCollection[user.Guild].Id)
>>>>>>> dev
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
<<<<<<< HEAD
                        await touple.Item2.DeleteAsync();
                        LavaOperations.GuildsPlayers.Remove(touple);                        
                        touple.Item2 = null;
                        LavaOperations.GuildsPlayers.Add(touple);
=======
                        await ((RestUserMessage)LavaOperations.PlayersMessagesCollection[user.Guild]).DeleteAsync();
                        LavaOperations.PlayersMessagesCollection[user.Guild] = null;
>>>>>>> dev
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
                    await LavaNode.LeaveAsync(prevChannel);
            }
            else if (prevChannel != null && prevChannel.Users.Count == 1)            
                await LavaNode.LeaveAsync(prevChannel);            
        }

        private async Task LavaNode_OnTrackEnded(TrackEndedEventArgs arg)
        {                                           
            var guild = arg.Player.VoiceState.VoiceChannel.Guild;
<<<<<<< HEAD
            ValueTuple<SocketGuild, SocketUserMessage> touple = default;
            foreach (var t in LavaOperations.GuildsPlayers)
                if (guild == t.Item1)
                    touple = t;
            var playerMessage = touple.Item2;
=======
            var playerMessage = (RestUserMessage)LavaOperations.PlayersMessagesCollection[guild];
>>>>>>> dev
            await playerMessage.DeleteAsync();
        }        
    }
}
