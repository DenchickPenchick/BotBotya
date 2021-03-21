//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.RoomManaging
{
    public class VoiceChannelsModule : IModule
    {
        private DiscordSocketClient Client { get; set; }

        public delegate void RoomEventsHandler(SocketGuildUser user, IVoiceChannel channel);
        public event RoomEventsHandler OnRoomCreated;
        public event RoomEventsHandler OnRoomDestroyed;

        private readonly Dictionary<ulong, ulong> txtChannelsForVoice = new Dictionary<ulong, ulong>();

        public VoiceChannelsModule(DiscordSocketClient client)
        {
            Client = client;                                   
        }

        public void RunModule()
        {
            Client.Ready += Client_Ready;
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
            Client.ChannelDestroyed += Client_ChannelDestroyed;
        }

        private async Task Client_Ready()
        {
            await CheckRooms();
        }

        private async Task Client_ChannelDestroyed(SocketChannel arg)
        {
            var channel = arg as SocketGuildChannel;
            var guild = channel.Guild;
            var provider = new GuildProvider(guild);
            var serGuild = FilesProvider.GetGuild(guild);

            if (serGuild.SystemChannels.CreateRoomChannelId == arg.Id && provider.RoomsCategoryChannel() != null)
            {
                var createRoomChannel = await guild.CreateVoiceChannelAsync("➕Создать комнату", x => x.CategoryId = provider.RoomsCategoryChannel().Id);
                serGuild.SystemChannels.CreateRoomChannelId = createRoomChannel.Id;
                FilesProvider.RefreshGuild(serGuild);
            }
        }

        private async Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            try
            {
                var channel = arg3.VoiceChannel;
                var prevchannel = arg2.VoiceChannel;
                SocketGuildUser socketGuildUser = arg1 as SocketGuildUser;
                var guild = socketGuildUser.Guild;
                GuildProvider provider = new(guild);
                var denyPerms = new OverwritePermissions(sendMessages: PermValue.Deny, viewChannel: PermValue.Deny);
                var allowPerms = new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow);


                if (provider.RoomsCategoryChannel() != null)
                {
                    if (channel != null && prevchannel != null && channel != prevchannel)
                    {
                        if (channel == provider.CreateRoomChannel())
                        {
                            if (prevchannel.Name.Contains(socketGuildUser.Nickname) || prevchannel.Name.Contains(socketGuildUser.Username))
                                await socketGuildUser.ModifyAsync(x => x.Channel = prevchannel);
                            else if (prevchannel.Users.Count == 0 && prevchannel.Category == provider.RoomsCategoryChannel())
                            {
                                OnRoomDestroyed?.Invoke(socketGuildUser, prevchannel);
                                await prevchannel.DeleteAsync();
                            }

                            else
                                await CreateRoom(socketGuildUser, provider);
                        }
                        else if (prevchannel != provider.CreateRoomChannel() && prevchannel.Category == provider.RoomsCategoryChannel() && prevchannel.Users.Count == 0)
                        {
                            OnRoomDestroyed?.Invoke(socketGuildUser, prevchannel);
                            await prevchannel.DeleteAsync();
                        }

                    }
                    else if (channel != null)//Пользователь подкючился
                    {
                        if (channel == provider.CreateRoomChannel())
                            await CreateRoom(socketGuildUser, provider);
                    }
                    else if (prevchannel != null)//Пользователь отключился
                        if (prevchannel.Users.Count == 0 && prevchannel.Category == provider.RoomsCategoryChannel() && prevchannel.Name != provider.CreateRoomChannel().Name)
                        {
                            OnRoomDestroyed?.Invoke(socketGuildUser, prevchannel);
                            await prevchannel.DeleteAsync();
                        }
                }

                await ChannelForMicroDisabled(arg1, arg2, arg3);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception throwed while creating room: {e}", Color.Red);
            }
        }

        private async Task CheckRooms()
        {
            foreach (var guild in Client.Guilds)
            {
                GuildProvider provider = new GuildProvider(guild);

                var RoomsCategory = provider.RoomsCategoryChannel();
                var CreateRoomChannel = provider.CreateRoomChannel();

                if (RoomsCategory != null)
                {
                    if (CreateRoomChannel == null)
                    {
                        var serGuild = FilesProvider.GetGuild(guild);
                        var channel = await guild.CreateVoiceChannelAsync("➕Создать комнату", x => x.CategoryId = RoomsCategory.Id);
                        serGuild.SystemChannels.CreateRoomChannelId = channel.Id;
                        FilesProvider.RefreshGuild(serGuild);
                    }

                    foreach (SocketVoiceChannel channel in provider.RoomsCategoryChannel().Channels)
                        if (channel.Users.Count == 0 && channel != provider.CreateRoomChannel())
                            await channel.DeleteAsync();

                    foreach (var roomChannel in RoomsCategory.Channels)
                        if (roomChannel.Users.Count != 0 && roomChannel == provider.CreateRoomChannel())
                        {
                            SocketGuildUser firstUser = roomChannel.Users.ToArray()[0];
                            var newChannel = await CreateRoom(firstUser, provider);

                            foreach (SocketGuildUser guildUser in roomChannel.Users)
                                await guildUser.ModifyAsync(x => { x.Channel = newChannel; });
                            continue;
                        }
                }
            }
            Console.WriteLine("Checked");
        }

        private async Task ChannelForMicroDisabled(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
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
                    if (currentChannel.Users.Count == 1 && serGuild.SystemChannels.CreateRoomChannelId != currentChannel.Id && !txtChannelsForVoice.ContainsKey(currentChannel.Id))
                    {
                        var chann = await contextGuild.CreateTextChannelAsync($"{currentChannel.Name}", x => { x.CategoryId = currentChannel.CategoryId; x.Position = currentChannel.Position + 1; });

                        txtChannelsForVoice.Add(currentChannel.Id, chann.Id);

                        await chann.AddPermissionOverwriteAsync(contextGuild.EveryoneRole, denyPerms);
                        foreach (var userInCh in currentChannel.Users)
                            await chann.AddPermissionOverwriteAsync(userInCh, allowPerms);
                    }
                    else
                    {
                        if (txtChannelsForVoice.ContainsKey(currentChannel.Id))
                        {                         
                            var channel = contextGuild.GetTextChannel(txtChannelsForVoice[currentChannel.Id]);

                            if (channel != null)
                                await channel.AddPermissionOverwriteAsync(user, allowPerms);
                        }
                    }
                }
                if (prevChannel != null)
                    if (prevChannel.Users.Count == 0)
                    {
                        if (txtChannelsForVoice.ContainsKey(prevChannel.Id))
                        {
                            var channel = contextGuild.GetTextChannel(txtChannelsForVoice[prevChannel.Id]);

                            if (channel != null)
                            {
                                txtChannelsForVoice.Remove(prevChannel.Id);
                                await channel.DeleteAsync();
                            }
                        }                        
                    }
            }
        }

        private async Task<SocketVoiceChannel> CreateRoom(SocketGuildUser user, GuildProvider provider)
        {
            bool haveChannel = false;
            SocketVoiceChannel socketVoiceChannel = null;
            var guild = user.Guild;
            var voiceChannels = guild.VoiceChannels;
            var serGuild = FilesProvider.GetGuild(guild);

            foreach (var oneofchannel in voiceChannels)
                if (user.Nickname != null)
                {
                    if (oneofchannel.Name.Contains(user.Nickname)) { socketVoiceChannel = oneofchannel; haveChannel = true; }
                    continue;
                }
                else
                {
                    if (oneofchannel.Name.Contains(user.Username)) { socketVoiceChannel = oneofchannel; haveChannel = true; }
                    continue;
                }

            if (!haveChannel)
            {
                var newChannel = await guild.CreateVoiceChannelAsync($"{serGuild.EmojiOfRoom}Комната {user.Nickname ?? user.Username}", x => x.CategoryId = provider.RoomsCategoryChannel().Id);
                OnRoomCreated?.Invoke(user, newChannel);
                await user.ModifyAsync(x =>
                {
                    x.Channel = newChannel;
                });
                return guild.GetVoiceChannel(newChannel.Id);
            }
            else
            {
                await user.ModifyAsync(x => { x.Channel = socketVoiceChannel; });
                return socketVoiceChannel;
            }
        }
    }
}
