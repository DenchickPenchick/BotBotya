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
using DiscordBot.Modules;
using System.Threading.Tasks;
using Console = Colorful.Console;
using Discord;
using DiscordBot.Providers;
using System.Linq;

namespace DiscordBot.GuildManaging
{
    public class GuildModule : IModule
    {        
        private readonly DiscordSocketClient Client;        

        public GuildModule(DiscordSocketClient client)
        {
            Client = client;
            Client.ReactionAdded += Client_ReactionAdded;
        }        

        public void RunModule()
        {
            Client.JoinedGuild += Client_JoinedGuild;            
            Client.UserJoined += Client_UserJoined;
            Client.UserLeft += Client_UserLeft;            
        }

        private async Task Client_UserLeft(SocketGuildUser arg)
        {
            var serGuild = FilesProvider.GetGuild(arg.Guild);
            var leaveChannel = arg.Guild.GetTextChannel(serGuild.SystemChannels.LeaveUsersChannelId);

            if (leaveChannel != null)
                await leaveChannel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = $"Прощай, {arg.Username}! Ты был нам другом...",
                    ThumbnailUrl = arg.GetAvatarUrl(),
                    Color = Color.Blue
                }.Build());
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            var serGuild = FilesProvider.GetGuild(arg.Guild);
            var defaultRole = arg.Guild.GetRole(serGuild.DefaultRoleId);
            var newUsers = arg.Guild.GetTextChannel(serGuild.SystemChannels.NewUsersChannelId);

            if (serGuild.DefaultRoleId != default)
                await arg.AddRoleAsync(defaultRole);
            if(serGuild.HelloMessageEnable && serGuild.HelloMessage != null)
                await arg.GetOrCreateDMChannelAsync().Result.SendMessageAsync(serGuild.HelloMessage);
            if (newUsers != null)
                await arg.Guild.GetTextChannel(serGuild.SystemChannels.NewUsersChannelId).SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = $"Привет, {arg.Username}!",
                    ThumbnailUrl = arg.GetAvatarUrl(),
                    Color = Color.Blue
                }.Build());
        }

        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            Console.WriteLine($"Bot joined new guild({arg.Id}).", Color.Blue);
            await new GuildProvider(arg).SendHelloMessageToGuild(Client);                        
        }        

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var userMessage = await arg1.DownloadAsync();
            var message = FilesProvider.GetReactRoleMessage(userMessage.Id);

            if (message != null)
            {                
                if (arg3.User.Value is SocketGuildUser user)
                {
                    if (!user.IsBot)
                    { 
                        var reactRoleMess = message.EmojiesRoleId;
                        int indexOf = reactRoleMess.Select(x => x.Item1).ToList().IndexOf(arg3.Emote.Name);
                    
                        await user.AddRoleAsync(user.Guild.GetRole(reactRoleMess[indexOf].Item2));
                        await userMessage.RemoveReactionAsync(arg3.Emote, arg3.User.Value);                    
                    }
                }
            }            
        }
    }
}