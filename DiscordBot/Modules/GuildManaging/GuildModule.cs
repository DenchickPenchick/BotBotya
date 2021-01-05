using Discord.WebSocket;
using DiscordBot.FileWorking;
using DiscordBot.Modules;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Console = Colorful.Console;
using System;
using Discord;

namespace DiscordBot.GuildManaging
{
    /// <summary>
    /// Модуль, который отвечает за структурную целостность сервера (например, он котролирует наличие канала "🤖Бот").
    /// Если удалить этот модуль, тогда все сломается при первом же удалении или изменении какого-либо канала в какой-либо гильдии. А об добавлении на сервер я уже молчу...
    /// </summary>
    public class GuildModule : IModule
    {        
        private readonly DiscordSocketClient Client;
        IReadOnlyCollection<SocketGuild> Guilds { get => Client.Guilds; }        

        public GuildModule(DiscordSocketClient client)
        {
            Client = client;                    
        }        

        public void RunModule()
        {
            Client.JoinedGuild += Client_JoinedGuild;
            Client.ChannelDestroyed += ChannelDestroyed;
            Client.UserJoined += Client_UserJoined;
            Client.Ready += Client_Ready;            
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            var serGuild = FilesProvider.GetGuild(arg.Guild);
            var defaultRole = arg.Guild.GetRole(serGuild.DefaultRoleId);

            if(serGuild.DefaultRoleId != default)
                await arg.AddRoleAsync(defaultRole);
            if(serGuild.HelloMessageEnable && serGuild.HelloMessage != null)
                await arg.GetOrCreateDMChannelAsync().Result.SendMessageAsync(serGuild.HelloMessage);
            await arg.Guild.TextChannels.ToArray()[0].SendMessageAsync($"Поприветстуем малоуважемого {arg.Username}!");
        }

        private async Task Client_Ready()
        {
            await SetupGuilds();            
        }

        private async Task ChannelDestroyed(SocketChannel arg)
        {            
            await RebuildCurrentChannel(arg as SocketGuildChannel);
        }

        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            Console.WriteLine($"Bot joined new guild({arg.Id}).", Color.Blue);
            await new GuildProvider(arg).SendHelloMessageToGuild(Client);            
            await FirstGuildSetup(arg);
        }

        private async Task SetupGuilds()
        {
            Console.WriteLine("Guild(-s) setup started");
            var progress = new ProgressBar(Client.Guilds.Count);
            if (Guilds.Count > 0)
                foreach (SocketGuild guild in Guilds)
                {                    
                    await SetupGuild(guild);
                    progress.PlusVal(Console.CursorTop);
                }
                   
            FilesProvider.ChangeNewsAndPlansToFalse();            
            Console.WriteLine("Guild(-s) setup ended", Color.Green);
        }

        private async Task FirstGuildSetup(SocketGuild guild)
        {
            var serGuild = FilesProvider.GetGuild(guild);
            var provider = new GuildProvider(guild);

            if (guild.Channels.Contains(provider.MainTextChannelsCategory()) || guild.Channels.Contains(provider.MainVoiceChannelsCategory()) || guild.Channels.Contains(provider.BotChannelsCategory()))            
                await guild.DefaultChannel.SendMessageAsync("⚠️ВНИМАНИЕ⚠️\nНа Вашем сервере найдены каналы, которые сходственны по именам с системными каналами. Советуем удалить их, либо же есть риск возникновения ошибок.");                           
               
            var txtCat = await guild.CreateCategoryChannelAsync(serGuild.SystemCategories.MainTextCategoryName);
            await guild.CreateTextChannelAsync("💬основной", x => x.CategoryId = txtCat.Id);

            var voiceCat = await guild.CreateCategoryChannelAsync(serGuild.SystemCategories.MainVoiceCategoryName);
            await guild.CreateVoiceChannelAsync("🎤Основной", x => x.CategoryId = voiceCat.Id);
            
            var botCat = await guild.CreateCategoryChannelAsync(serGuild.SystemCategories.BotCategoryName);
            await guild.CreateTextChannelAsync(serGuild.SystemChannels.ConsoleChannelName, x => x.CategoryId = botCat.Id);
        }

        private async Task SetupGuild(SocketGuild Guild)
        {
            var SerializableGuild = FilesProvider.GetGuild(Guild);
            var serNewsPlans = FilesProvider.GetNewsAndPlans();            

            var categories = Guild.CategoryChannels;
            var textChannels = Guild.TextChannels;
            var voiceChannels = Guild.VoiceChannels;

            List<string> listOfSystemCategories = new List<string>
            {
                { SerializableGuild.SystemCategories.MainTextCategoryName },
                { SerializableGuild.SystemCategories.ContentCategoryName },
                { SerializableGuild.SystemCategories.MainVoiceCategoryName},
                { SerializableGuild.SystemCategories.VoiceRoomsCategoryName },
                { SerializableGuild.SystemCategories.BotCategoryName }
            };
            List<string> listOfSytemTextChannels = new List<string>
            {
                { SerializableGuild.SystemChannels.LinksChannelName },
                { SerializableGuild.SystemChannels.VideosChannelName },
                { SerializableGuild.SystemChannels.ConsoleChannelName },                
            };
            List<string> listOfSystemVoiceChannels = new List<string>
            {
                { SerializableGuild.SystemChannels.CreateRoomChannelName }
            };

            List<string> NamesOfCategories = new List<string>();
            List<string> NamesOfTextChannels = new List<string>();
            List<string> NamesOfVoiceChannels = new List<string>();

            if (!SerializableGuild.ContentEnable)
            {
                listOfSystemCategories.Remove(SerializableGuild.SystemCategories.ContentCategoryName);
                listOfSytemTextChannels.Remove(SerializableGuild.SystemChannels.LinksChannelName);
                listOfSytemTextChannels.Remove(SerializableGuild.SystemChannels.VideosChannelName);
            }

            if (!SerializableGuild.RoomsEnable)
            {
                listOfSystemCategories.Remove(SerializableGuild.SystemCategories.VoiceRoomsCategoryName);
                listOfSystemVoiceChannels.Remove(SerializableGuild.SystemChannels.CreateRoomChannelName);
            }

            foreach (var category in categories)
                NamesOfCategories.Add(category.Name);
            foreach (var textChannel in textChannels)
                NamesOfTextChannels.Add(textChannel.Name);
            foreach (var voiceChannel in voiceChannels)
                NamesOfVoiceChannels.Add(voiceChannel.Name);

            for (int i = 0; i < listOfSystemCategories.Count; i++)
                if (!NamesOfCategories.Contains(listOfSystemCategories[i]))
                    await Guild.CreateCategoryChannelAsync(listOfSystemCategories[i]);
            for (int i = 0; i < listOfSytemTextChannels.Count; i++)
                if (!NamesOfTextChannels.Contains(listOfSytemTextChannels[i]))
                    await Guild.CreateTextChannelAsync(listOfSytemTextChannels[i]);
            for (int i = 0; i < listOfSystemVoiceChannels.Count; i++)
                if (!NamesOfVoiceChannels.Contains(listOfSystemVoiceChannels[i]))
                    await Guild.CreateVoiceChannelAsync(listOfSystemVoiceChannels[i]);


            GuildProvider provider = new GuildProvider(Guild);
            var mainTextChannelsCategory = provider.MainTextChannelsCategory();

            if (provider.MainTextChannelsCategory().Channels.Count == 0)
                await Guild.CreateTextChannelAsync("💬основной", x => x.CategoryId = provider.MainTextChannelsCategory().Id);
            if (provider.MainVoiceChannelsCategory().Channels.Count == 0)
                await Guild.CreateVoiceChannelAsync("🎤Основной", x => x.CategoryId = provider.MainVoiceChannelsCategory().Id);
            if (SerializableGuild.RoomsEnable)
                await provider.CreateRoomChannel().ModifyAsync(x => x.CategoryId = provider.RoomsCategoryChannel().Id);
            if (SerializableGuild.ContentEnable)
            {
                await provider.LinksTextChannel().ModifyAsync(x => x.CategoryId = provider.ContentCategoryChannel().Id);
                await provider.VideosTextChannel().ModifyAsync(x => x.CategoryId = provider.ContentCategoryChannel().Id);
            }
            await provider.ConsoleChannel().ModifyAsync(x => x.CategoryId = provider.BotChannelsCategory().Id);

            if (!SerializableGuild.RoomsEnable)
            {
                if (provider.RoomsCategoryChannel() != null)
                    await provider.RoomsCategoryChannel().DeleteAsync();
                if (provider.CreateRoomChannel() != null)
                    await provider.CreateRoomChannel().DeleteAsync();
            }

            if (!SerializableGuild.ContentEnable)
            {
                if (provider.ContentCategoryChannel() != null)
                    await provider.ContentCategoryChannel().DeleteAsync();
                if (provider.LinksTextChannel() != null)
                    await provider.LinksTextChannel().DeleteAsync();
                if (provider.VideosTextChannel() != null)
                    await provider.VideosTextChannel().DeleteAsync();
            }

            if (serNewsPlans.ShouldSend)
                await Guild.TextChannels.ToArray()[0].SendMessageAsync(embed: serNewsPlans.GetNewsAndPlansEmbed(Client));                        
        }        

        private async Task RebuildCurrentChannel(SocketGuildChannel channel)
        {
            var serGuild = FilesProvider.GetGuild(channel.Guild);
            if (!new GuildProvider(channel.Guild).ExistChannelByName(channel))
            {                
                List<string> CategoriesName = new List<string>
                {
                    serGuild.SystemCategories.BotCategoryName,
                    serGuild.SystemCategories.ContentCategoryName,
                    serGuild.SystemCategories.MainTextCategoryName,
                    serGuild.SystemCategories.MainVoiceCategoryName,
                    serGuild.SystemCategories.VoiceRoomsCategoryName
                };

                if (CategoriesName.Contains(channel.Name))
                {
                    if (channel.Name == serGuild.SystemCategories.MainTextCategoryName)
                    {
                        var catText = await channel.Guild.CreateCategoryChannelAsync("💬Текстовые каналы");
                        await channel.Guild.CreateTextChannelAsync("💬основной", x => x.CategoryId = catText.Id);
                    }
                    else if (channel.Name == serGuild.SystemCategories.ContentCategoryName)
                    {
                        if (serGuild.ContentEnable)
                        {
                            var catCont = await channel.Guild.CreateCategoryChannelAsync(serGuild.SystemCategories.ContentCategoryName);
                            await channel.Guild.CreateTextChannelAsync("🌐ссылки", x => x.CategoryId = catCont.Id);
                            await channel.Guild.CreateTextChannelAsync("📹видео", x => x.CategoryId = catCont.Id);
                        }
                    }
                    else if (channel.Name == serGuild.SystemCategories.MainVoiceCategoryName)
                    {
                        var catVoice = await channel.Guild.CreateCategoryChannelAsync(serGuild.SystemCategories.MainVoiceCategoryName);
                        await channel.Guild.CreateVoiceChannelAsync("🎤Основной", x => x.CategoryId = catVoice.Id);
                    }
                    else if (channel.Name == serGuild.SystemCategories.VoiceRoomsCategoryName)
                    {
                        if (serGuild.RoomsEnable)
                        {
                            var catRoom = await channel.Guild.CreateCategoryChannelAsync(serGuild.SystemCategories.VoiceRoomsCategoryName);
                            await channel.Guild.CreateVoiceChannelAsync(serGuild.SystemChannels.CreateRoomChannelName, x => x.CategoryId = catRoom.Id);
                        }
                    }
                    else if (channel.Name == serGuild.SystemCategories.BotCategoryName)
                    {
                        var catBot = await channel.Guild.CreateCategoryChannelAsync(serGuild.SystemCategories.BotCategoryName);
                        await channel.Guild.CreateTextChannelAsync(serGuild.SystemChannels.ConsoleChannelName, x => x.CategoryId = catBot.Id);
                    }
                }
                else
                    await SetupGuild(channel.Guild);
            }
            
        }
    }
}
