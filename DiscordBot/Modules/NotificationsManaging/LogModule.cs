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

using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using DiscordBot.Providers;
using DiscordBot.RoomManaging;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Modules.NotificationsManaging
{
    public class LogModule : IModule
    {
        private DiscordSocketClient Client { get; }
        private RoomModule RoomModuleInstance { get; set; }        

        public LogModule(DiscordSocketClient client, IServiceProvider modules)
        {
            Client = client;           
            RoomModuleInstance = modules.GetRequiredService<RoomModule>();
            Client.Ready += Client_Ready;            
        }

        private Task Client_Ready()
        {            
            RoomModuleInstance.OnRoomCreated += RoomModuleInstance_OnRoomCreated;
            RoomModuleInstance.OnRoomDestroyed += RoomModuleInstance_OnRoomDestroyed;
            return Task.CompletedTask;
        }

        public void RunModule()
        {            
            Client.MessageDeleted += Client_MessageDeleted;
            Client.UserLeft += Client_UserLeft;
            Client.UserJoined += Client_UserJoined;
            Client.MessageUpdated += Client_MessageUpdated;                                  
        }

        private async void RoomModuleInstance_OnRoomDestroyed(SocketGuildUser user, IVoiceChannel channel)
        {
            await SendLog(user, new EmbedBuilder
            { 
                Description = $"{user.Mention} удалил комнату {channel.Name}"
            });
        }

        private async void RoomModuleInstance_OnRoomCreated(SocketGuildUser user, IVoiceChannel channel)
        {
            await SendLog(user, new EmbedBuilder
            {
                Description = $"{user.Mention} создал комнату {channel.Name}"
            });
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

        private async Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (!arg2.Author.IsBot && arg3 is SocketTextChannel channel && arg2 is SocketUserMessage message)
                await SendLog(message.Author, new EmbedBuilder
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

        private async Task SendLog(IUser user, EmbedBuilder builder)
        {
            var guild = (user as SocketGuildUser).Guild;
            var serGuild = FilesProvider.GetGuild(guild);
            if (serGuild.SystemChannels.LogsChannelId != default)
            {                                
                string time = $"Время: {DateTime.Now.ToShortTimeString()}";                

                builder.Footer = new EmbedFooterBuilder
                {
                    Text = $"{time}\nСервер: {(user as SocketGuildUser).Guild.Name}",
                    IconUrl = user.GetAvatarUrl()
                };
                builder.Color = ColorProvider.GetColorForCurrentGuild(serGuild);

                var channel = guild.GetTextChannel(serGuild.SystemChannels.LogsChannelId);
                if (channel != null)
                    await channel.SendMessageAsync(embed: builder.Build());
                else 
                {                                       
                    serGuild.SystemChannels.LogsChannelId = default;
                    FilesProvider.RefreshGuild(serGuild);
                }                    
            }            
        }
    }
}
