using Discord;
using Discord.WebSocket;
using DiscordBot.FileWorking;
using DiscordBot.GuildManaging;
using DiscordBot.Modules;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.RoomManaging
{
    public class RoomModule : IModule
    {
        private DiscordSocketClient Client { get; set; }                

        public RoomModule(DiscordSocketClient client)
        {
            Client = client;
        }

        public void RunModule()
        {
            Client.Ready += Client_Ready;
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;           
        }

        private async Task Client_Ready()
        {
            await CheckRooms();
        }

        private async Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (FilesProvider.GetGuild((arg1 as SocketGuildUser).Guild).RoomsEnable)            
                try
                {
                    var channel = arg3.VoiceChannel;
                    var prevchannel = arg2.VoiceChannel;
                    SocketGuildUser socketGuildUser = arg1 as SocketGuildUser;
                    var guild = socketGuildUser.Guild;
                    GuildProvider provider = new GuildProvider(guild);
                
                    if (channel != null && prevchannel != null && channel != prevchannel)
                    {
                        if (channel == provider.CreateRoomChannel())
                        {
                            if (prevchannel.Category == provider.RoomsCategoryChannel())
                            {
                                if (prevchannel.Name.Contains(socketGuildUser.Nickname) || prevchannel.Name.Contains(socketGuildUser.Username))
                                    await socketGuildUser.ModifyAsync(x => x.Channel = prevchannel);
                                else if (prevchannel.Users.Count == 0)
                                    await prevchannel.DeleteAsync();
                                else
                                    await CreateRoom(socketGuildUser, provider);
                            }
                        }
                        else if (prevchannel != provider.CreateRoomChannel() && prevchannel.Category == provider.RoomsCategoryChannel() && prevchannel.Users.Count == 0)
                            await prevchannel.DeleteAsync();
                    }
                    else if (channel != null)//Пользователь подкючился
                    {
                        if (channel == provider.CreateRoomChannel())
                            await CreateRoom(socketGuildUser, provider);
                    }
                    else if (prevchannel != null)//Пользователь отключился
                        if (prevchannel.Users.Count == 0 && prevchannel.Category == provider.RoomsCategoryChannel() && prevchannel.Name != provider.CreateRoomChannel().Name)
                            await prevchannel.DeleteAsync();
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
                if (FilesProvider.GetGuild(guild).RoomsEnable)
                {
                    GuildProvider provider = new GuildProvider(guild);

                    var MainCategory = Client.GetGuild(guild.Id).GetCategoryChannel(provider.GetCategoryId(GuildProvider.GetCategoryIdEnum.MainVoiceChannelsCategory));
                    var RoomsCategory = Client.GetGuild(guild.Id).GetCategoryChannel(provider.GetCategoryId(GuildProvider.GetCategoryIdEnum.RoomVoiceChannelsCategory));

                    foreach (SocketVoiceChannel channel in provider.RoomsCategoryChannel().Channels)
                        if (channel.Users.Count == 0 && !provider.MainVoiceChannels().Contains(channel) && channel != provider.CreateRoomChannel())
                            await channel.DeleteAsync();
                    foreach (var offlineChannel in MainCategory.Channels)
                        if (offlineChannel.Users.Count != 0)
                        {
                            SocketGuildUser firstUser = offlineChannel.Users.ToArray()[0];
                            var newChannel = await CreateRoom(firstUser, provider);

                            foreach (SocketGuildUser guildUser in offlineChannel.Users)
                                await guildUser.ModifyAsync(x => { x.Channel = newChannel; });
                            continue;
                        }

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
