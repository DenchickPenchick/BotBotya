using Discord;
using Discord.WebSocket;
using DiscordBot.FileWorking;
using DiscordBot.GuildManaging;
using DiscordBot.MusicOperations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
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
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
        }

        private async Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            var prevChannel = arg2.VoiceChannel;
            var channel = arg3.VoiceChannel;            

            if (prevChannel != null && channel == null && arg1.Id == Client.CurrentUser.Id)            
                await LavaNode.LeaveAsync(prevChannel);                        
        }

        public void RunModule()
        {
            
        }        
        
        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            IMessage message = await arg1.GetOrDownloadAsync();
            var user = arg3.User.GetValueOrDefault() as SocketGuildUser;           
            if (!user.IsBot && message.Id == ((IMessage)LavaOperations.GuildsPlayers[user.Guild]).Id)
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
                    case "⏮":
                        var track = player.Track;
                        int index = player.Queue.ToList().IndexOf(track);
                        int backIndex = index == 0 ? 0 : index - 1;
                        await player.PlayAsync(player.Queue.ToList()[backIndex]);
                        break;
                    case "⏭":
                        await player.SkipAsync();
                        break;
                }
                await message.RemoveReactionAsync(arg3.Emote, user);
            }
        }
    }
}
