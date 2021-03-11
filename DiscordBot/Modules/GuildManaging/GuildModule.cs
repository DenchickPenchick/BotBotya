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
|GitHub: https://github.com/DenVot/BotBotya                             |
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
using System.Collections.Generic;
using System.Collections;

namespace DiscordBot.GuildManaging
{
    /// <summary>
    /// Модуль, который отвечает за правильную работу с серверами.
    /// </summary>
    public class GuildModule : IModule
    {        
        private readonly DiscordSocketClient Client;

        private Dictionary<ulong, ulong> txtChannelsForVoice = new Dictionary<ulong, ulong>();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="GuildModule"/>
        /// </summary>
        /// <param name="client">Клиент (<see cref="DiscordSocketClient"/>)</param>
        public GuildModule(DiscordSocketClient client)
        {
            Client = client;
            Client.ReactionAdded += Client_ReactionAdded;
        }        

        /// <summary>
        /// Запускает модуль
        /// </summary>
        public void RunModule()
        {
            Client.JoinedGuild += Client_JoinedGuild;            
            Client.UserJoined += Client_UserJoined;
            Client.UserLeft += Client_UserLeft;
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
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

        private async Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (arg1 is SocketGuildUser user)
            {             
                var serGuild = FilesProvider.GetGuild(user.Guild);
                var currentChannel = arg3.VoiceChannel;
                var prevChannel = arg2.VoiceChannel;
                var contextGuild = user.Guild;

                var denyPerms = new OverwritePermissions(sendMessages: PermValue.Deny, viewChannel: PermValue.Deny);
                var allowPerms = new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow);

                if (currentChannel != null)
                {
                    if (currentChannel.Users.Count == 1 && serGuild.SystemChannels.CreateRoomChannelId != currentChannel.Id)
                    {
                        var chann = await contextGuild.CreateTextChannelAsync($"{currentChannel.Name}", x => { x.CategoryId = currentChannel.CategoryId; x.Position = currentChannel.Position + 1; });

                        txtChannelsForVoice.Add(currentChannel.Id, chann.Id);

                        await chann.AddPermissionOverwriteAsync(contextGuild.EveryoneRole, denyPerms);
                        foreach (var userInCh in currentChannel.Users)
                            await chann.AddPermissionOverwriteAsync(userInCh, allowPerms);
                    }
                    else
                    {
                        var channel = contextGuild.GetTextChannel(txtChannelsForVoice[currentChannel.Id]);

                        if (channel != null)                        
                            await channel.AddPermissionOverwriteAsync(user, allowPerms);                        
                    }
                }
                if(prevChannel != null)
                    if (prevChannel.Users.Count == 0)
                    {
                        var channel = contextGuild.GetTextChannel(txtChannelsForVoice[prevChannel.Id]);

                        if (channel != null)
                            await channel.DeleteAsync();
                    }
            }
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