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
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using DiscordBot.Providers;

namespace DiscordBot.Modules.NotificationsManaging
{
    public class LogModule : IModule
    {
        private DiscordSocketClient Client { get; }

        public LogModule(DiscordSocketClient client)
        {
            Client = client;
        }

        public void RunModule()
        {            
            Client.UserLeft += Client_UserLeft;
            Client.UserJoined += Client_UserJoined;
            Client.MessageUpdated += Client_MessageUpdated;            
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;            
        }

        private async Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (arg3.IsStreaming == arg2.IsStreaming &&                  
                (arg3.IsMuted == arg2.IsMuted) && 
                (arg2.IsSelfMuted == arg3.IsSelfMuted) &&
                (arg2.IsSelfDeafened == arg3.IsSelfDeafened) && 
                (arg2.IsDeafened == arg3.IsDeafened))
            {
                var user = arg1 as SocketGuildUser;
                if (arg2.VoiceChannel != null && arg3.VoiceChannel != null)
                    await SendLog(arg1, new EmbedBuilder
                    {
                        Description = $"Участник {user.Username} покинул канал {arg2.VoiceChannel.Name} и присоединился к каналу {arg3.VoiceChannel.Name}."
                    });
                else if (arg2.VoiceChannel != null && arg3.VoiceChannel == null)
                    await SendLog(arg1, new EmbedBuilder
                    {
                        Description = $"Участник {user.Username} покинул канал {arg2.VoiceChannel.Name}."
                    });
                else if (arg3.VoiceChannel != null && arg2.VoiceChannel == null)
                    await SendLog(arg1, new EmbedBuilder
                    {
                        Description = $"Участник {user.Username} присоединился к каналу {arg3.VoiceChannel.Name}."
                    });
            }            
        }        

        private async Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            await SendLog((arg2 as SocketUserMessage).Author, new EmbedBuilder
            {
                Description = $"Участник {arg2.Author.Username} отредактировал сообщение.",
                Fields =  new List<EmbedFieldBuilder> 
                {                    
                    new EmbedFieldBuilder
                    {
                        Name = "Cодержание сообщения:",
                        Value = arg2.Content,
                        IsInline = false
                    }
                }
            });
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            await SendLog(arg, new EmbedBuilder
            {
                Description = $"Новый участник сервера {arg.Username}"                
            });
        }

        private async Task Client_UserLeft(SocketGuildUser arg)
        {
            await SendLog(arg, new EmbedBuilder
            {
                Description = $"Участник {arg.Username} покинул сервер."                
            });
        }       

        private async Task SendLog(SocketUser user, EmbedBuilder builder)
        {
            var guild = (user as SocketGuildUser).Guild;
            var serGuild = FilesProvider.GetGuild(guild);
            if (FilesProvider.GetGuild((user as SocketGuildUser).Guild).GuildNotifications && serGuild.LoggerId != default)
            {                                
                string time;
                if (DateTime.Now.Minute < 10)
                    time = $"Время: { DateTime.Now.Hour}:0{DateTime.Now.Minute}";
                else
                    time = $"Время: { DateTime.Now.Hour}:{DateTime.Now.Minute}";
                builder.Footer = new EmbedFooterBuilder
                {
                    Text = $"{time}\nСервер: {(user as SocketGuildUser).Guild.Name}",
                    IconUrl = user.GetAvatarUrl()
                };
                builder.Color = Color.Blue;

                var channel = guild.GetTextChannel(serGuild.LoggerId);
                if (channel != null)
                    await channel.SendMessageAsync(embed: builder.Build());
                else 
                {                   
                    serGuild.GuildNotifications = false;
                    serGuild.LoggerId = default;
                    FilesProvider.RefreshGuild(serGuild);
                }                    
            }            
        }
    }
}
