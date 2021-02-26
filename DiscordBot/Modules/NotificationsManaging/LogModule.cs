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
            Client.MessageDeleted += Client_MessageDeleted;
            Client.UserLeft += Client_UserLeft;
            Client.UserJoined += Client_UserJoined;
            Client.MessageUpdated += Client_MessageUpdated;            
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;            
        }

        private async Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            var mess = arg1.Value;
            if (mess != null)
            { 
                var content = mess.Content;            
            
                if (!arg1.Value.Author.IsBot && arg2 is SocketTextChannel channel)
                {
                    var user = arg1.Value.Author as SocketGuildUser;
                    await SendLog(user, new EmbedBuilder
                    {
                        Description = $"Участник {user.Mention} удалил сообщение из канала {channel.Mention}",
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Содержание сообщения:",
                                Value= $"`{content}`"
                            }
                        }
                    });
                }            
            }
        }

        private async Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (arg3.IsStreaming == arg2.IsStreaming &&                  
                (arg3.IsMuted == arg2.IsMuted) && 
                (arg2.IsSelfMuted == arg3.IsSelfMuted) &&
                (arg2.IsSelfDeafened == arg3.IsSelfDeafened) && 
                (arg2.IsDeafened == arg3.IsDeafened) && !arg1.IsBot && arg1 is SocketGuildUser user)
            {                
                if (arg2.VoiceChannel != null && arg3.VoiceChannel != null)
                    await SendLog(arg1, new EmbedBuilder
                    {
                        Description = $"Участник {user.Mention} покинул канал `{arg2.VoiceChannel.Name}` и присоединился к каналу `{arg3.VoiceChannel.Name}`."
                    });
                else if (arg2.VoiceChannel != null && arg3.VoiceChannel == null)
                    await SendLog(arg1, new EmbedBuilder
                    {
                        Description = $"Участник {user.Mention} покинул канал `{arg2.VoiceChannel.Name}`."
                    });
                else if (arg3.VoiceChannel != null && arg2.VoiceChannel == null)
                    await SendLog(arg1, new EmbedBuilder
                    {
                        Description = $"Участник {user.Mention} присоединился к каналу `{arg3.VoiceChannel.Name}`."
                    });
            }            
        }        

        private async Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (!arg2.Author.IsBot && arg3 is SocketTextChannel channel)
                await SendLog((arg2 as SocketUserMessage).Author, new EmbedBuilder
                {
                    Description = $"Участник {(arg2.Author as SocketGuildUser).Mention} отредактировал сообщение в канале {channel.Mention}.",
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Старое содержание сообщения:",
                            Value = $"`{arg1.Value.Content}`",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Новое содержание сообщения:",
                            Value = $"`{arg2.Content}`",
                            IsInline = true
                        }
                    }
                });
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            await SendLog(arg, new EmbedBuilder
            {
                Description = $"Новый участник сервера {arg.Mention}"                
            });
        }

        private async Task Client_UserLeft(SocketGuildUser arg)
        {
            await SendLog(arg, new EmbedBuilder
            {
                Description = $"Участник {arg.Mention} покинул сервер."                
            });
        }       

        private async Task SendLog(SocketUser user, EmbedBuilder builder)
        {
            var guild = (user as SocketGuildUser).Guild;
            var serGuild = FilesProvider.GetGuild(guild);
            if (FilesProvider.GetGuild((user as SocketGuildUser).Guild).GuildNotifications && serGuild.LoggerId != default)
            {                                
                string time = $"Время: {DateTime.Now.ToShortTimeString()}";                

                builder.Footer = new EmbedFooterBuilder
                {
                    Text = $"{time}\nСервер: {(user as SocketGuildUser).Guild.Name}",
                    IconUrl = user.GetAvatarUrl()
                };
                builder.Color = ColorProvider.GetColorForCurrentGuild(serGuild);

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
