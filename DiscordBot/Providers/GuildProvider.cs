using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DiscordBot.FileWorking;
using DiscordBot.Modules.FileManaging;

namespace DiscordBot.GuildManaging
{
    public class GuildProvider
    {
        public SocketGuild Guild { get; }
        private SerializableGuild SerializableGuild { get => FilesProvider.GetGuild(Guild);}

        public GuildProvider(SocketGuild guild)
        {            
            Guild = guild;
        }

        public List<SocketGuildChannel> GetSystemChannels()
        {
            List<SocketGuildChannel> list = new List<SocketGuildChannel>();

            list.AddRange(SystemCategories());
            if(LinksTextChannel() != null)
                 list.Add(LinksTextChannel());
            if (VideosTextChannel() != null)
                list.Add(VideosTextChannel());
            if (CreateRoomChannel() != null)
                list.Add(CreateRoomChannel());
            list.Add(ConsoleChannel());

            return list;
        }
        public List<SocketCategoryChannel> SystemCategories()
        {
            List<SocketCategoryChannel> socketCategoryChannels = new List<SocketCategoryChannel>
            {
                MainTextChannelsCategory()
            };
            if (ContentCategoryChannel() != null)
                socketCategoryChannels.Add(ContentCategoryChannel());
            socketCategoryChannels.Add(MainVoiceChannelsCategory());
            if (RoomsCategoryChannel() != null)
                socketCategoryChannels.Add(RoomsCategoryChannel());
            socketCategoryChannels.Add(BotChannelsCategory());

            return socketCategoryChannels;
        }
        public List<SocketVoiceChannel> MainVoiceChannels()
        {
            List<SocketVoiceChannel> channels = new List<SocketVoiceChannel>();
            var category = Guild.GetCategoryChannel(GetCategoryId(GetCategoryIdEnum.MainVoiceChannelsCategory)).Channels;
            var voiceChannels = Guild.VoiceChannels;

            foreach (var channel in voiceChannels)
                if (channel.Category == category)
                    channels.Add(channel);

            return channels;
        }
        public SocketVoiceChannel CreateRoomChannel()
        {            
            var voiceChannels = Guild.VoiceChannels;
            var systemChannelsName = SerializableGuild.SystemChannels;
            SocketVoiceChannel voiceChannel = null;

            foreach (var channel in voiceChannels)
                if (channel.Name == systemChannelsName.CreateRoomChannelName)
                    voiceChannel = channel;
            return voiceChannel;
        }
        public SocketTextChannel LinksTextChannel()
        {            
            var textChannels = Guild.TextChannels;
            var systemChannelsName = SerializableGuild.SystemChannels;
            SocketTextChannel textChannel = null;

            foreach (var channel in textChannels)
                if (channel.Name == systemChannelsName.LinksChannelName)
                    textChannel = channel;
            return textChannel;
        }
        public SocketTextChannel VideosTextChannel()
        {            
            var textChannels = Guild.TextChannels;
            var systemChannelsName = SerializableGuild.SystemChannels;
            SocketTextChannel textChannel = null;

            foreach (var channel in textChannels)
                if (channel.Name == systemChannelsName.VideosChannelName)
                    textChannel = channel;          
                return textChannel;
        }
        public SocketTextChannel ConsoleChannel()
        {            
            var textChannels = Guild.TextChannels;
            var systemChannelsName = SerializableGuild.SystemChannels;
            SocketTextChannel textChannel = null;

            foreach (var channel in textChannels)
                if (channel.Name == systemChannelsName.ConsoleChannelName)
                    textChannel = channel;            
                return textChannel;
        }
        public SocketCategoryChannel RoomsCategoryChannel() => Guild.GetCategoryChannel(GetCategoryId(GetCategoryIdEnum.RoomVoiceChannelsCategory));
        public SocketCategoryChannel ContentCategoryChannel() => Guild.GetCategoryChannel(GetCategoryId(GetCategoryIdEnum.ContentChannelsCategory));
        public SocketCategoryChannel MainTextChannelsCategory() => Guild.GetCategoryChannel(GetCategoryId(GetCategoryIdEnum.MainTextChannelsCategory));        
        public SocketCategoryChannel MainVoiceChannelsCategory() => Guild.GetCategoryChannel(GetCategoryId(GetCategoryIdEnum.MainVoiceChannelsCategory));        
        public SocketCategoryChannel BotChannelsCategory() => Guild.GetCategoryChannel(GetCategoryId(GetCategoryIdEnum.BotChannelsCategory));
        public ulong GetCategoryId(GetCategoryIdEnum idEnum)
        {
            ulong id = 0;
            var systemCategoriesName = SerializableGuild.SystemCategories;
            var categories = Guild.CategoryChannels;

            foreach (var category in categories)
            {
                switch (idEnum)
                {
                    case GetCategoryIdEnum.MainVoiceChannelsCategory:
                        if (category.Name == systemCategoriesName.MainVoiceCategoryName)
                            id = category.Id;
                        break;
                    case GetCategoryIdEnum.RoomVoiceChannelsCategory:
                        if (category.Name == systemCategoriesName.VoiceRoomsCategoryName)
                            id = category.Id;
                        break;
                    case GetCategoryIdEnum.MainTextChannelsCategory:
                        if (category.Name == systemCategoriesName.MainTextCategoryName)
                            id = category.Id;
                        break;
                    case GetCategoryIdEnum.ContentChannelsCategory:
                        if (category.Name == systemCategoriesName.ContentCategoryName)
                            id = category.Id;
                        break;
                    case GetCategoryIdEnum.BotChannelsCategory:
                        if (category.Name == systemCategoriesName.BotCategoryName)
                            id = category.Id;
                        break;
                }
            }
            return id;
        }

        public enum GetCategoryIdEnum { MainVoiceChannelsCategory, RoomVoiceChannelsCategory, MainTextChannelsCategory, ContentChannelsCategory, BotChannelsCategory }

        public async Task SetupGuild()
        {                
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
                { SerializableGuild.SystemChannels.ConsoleChannelName }
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
        }

        public async Task SendHelloMessageToGuild(DiscordSocketClient client)
        {
            await Guild.DefaultChannel.SendMessageAsync(embed: new EmbedBuilder
            {
                Title = $"Спасибо, что пригласили меня на сервер {Guild.Name}",
                Description = $"Меня зовут {client.CurrentUser.Username}. Я бот, который постоянно обновляется из этого следует четыре вещи:\n" +
                $"1. Новые функции добавляются достаточно часто\n" +
                $"2. Но во мне есть немного багов. Они будут исправляться по мере возможности\n" +
                $"3. Я - тематичный бот. У меня есть определенная тематика (Я эту тематику встраиваю в сервер), например новогодняя тема.\n" +
                $"3. Я работую непостоянно, поэтому положиться на меня невозможно (по крайней мере сейчас)... Но как только я включусь, все встанет на свои места.\n" +
                $"Напиши !Help, чтобы узнать, что я умею (пиши в консольный канал, чтобы узнать мои консольные команды).\n" +
                $"Как только появится возможность, я проведу настройку сервера.\n" +
                $"Категории, которые я создаю, удалять нельзя (я их все равно создам заново). Также нельзя удалять каналы: 🌐ссылки, 📹видео, 🤖консоль-бота.\n" +
                $"Ну что! Приятного пользования мной!",
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Если хочешь поддержать автора, тогда кидай это своим друзьям: https://discord.com/oauth2/authorize?client_id=749991391639109673&scope=bot&permissions=1573583991",
                    IconUrl = client.CurrentUser.GetAvatarUrl()
                }               
            }.Build());
        }

        public async Task RebuildCurrentChannel(SocketGuildChannel channel)
        {
            var guildCategories = FilesProvider.GetGuild(channel.Guild);
            List<string> CategoriesName = new List<string>
            {
                guildCategories.SystemCategories.BotCategoryName,
                guildCategories.SystemCategories.ContentCategoryName,
                guildCategories.SystemCategories.MainTextCategoryName,
                guildCategories.SystemCategories.MainVoiceCategoryName,
                guildCategories.SystemCategories.VoiceRoomsCategoryName
            };

            if (CategoriesName.Contains(channel.Name))
            {
                if (channel.Name == guildCategories.SystemCategories.MainTextCategoryName)
                {
                    var catText = await channel.Guild.CreateCategoryChannelAsync("💬Текстовые каналы");
                    await channel.Guild.CreateTextChannelAsync("💬основной", x => x.CategoryId = catText.Id);
                }
                else if (channel.Name == guildCategories.SystemCategories.ContentCategoryName)
                {
                    if (FilesProvider.GetGuild(channel.Guild).ContentEnable)
                    {
                        var catCont = await channel.Guild.CreateCategoryChannelAsync("⚡Контент");
                        await channel.Guild.CreateTextChannelAsync("🌐ссылки", x => x.CategoryId = catCont.Id);
                        await channel.Guild.CreateTextChannelAsync("📹видео", x => x.CategoryId = catCont.Id);
                    }
                }
                else if (channel.Name == guildCategories.SystemCategories.MainVoiceCategoryName)
                {
                    var catVoice = await channel.Guild.CreateCategoryChannelAsync("🎤Голосовые каналы");
                    await channel.Guild.CreateVoiceChannelAsync("🎤Основной", x => x.CategoryId = catVoice.Id);
                }
                else if (channel.Name == guildCategories.SystemCategories.VoiceRoomsCategoryName)
                {
                    if (FilesProvider.GetGuild(channel.Guild).RoomsEnable)
                    {
                        var catRoom = await channel.Guild.CreateCategoryChannelAsync("🏠Комнаты");
                        await channel.Guild.CreateVoiceChannelAsync("➕Создать комнату", x => x.CategoryId = catRoom.Id);
                    }
                }
                else if (channel.Name == guildCategories.SystemCategories.BotCategoryName)
                {
                    var catBot = await channel.Guild.CreateCategoryChannelAsync("🤖Бот");
                    await channel.Guild.CreateTextChannelAsync("🤖консоль-бота", x => x.CategoryId = catBot.Id);
                }
            }
            else
                await SetupGuild();
        }

        public async Task SyncGuildProperties()
        {                        
            if (SerializableGuild.RoomsEnable && RoomsCategoryChannel() == null)
            {                
                var catRoom = await Guild.CreateCategoryChannelAsync(SerializableGuild.SystemCategories.VoiceRoomsCategoryName);
                await Guild.CreateVoiceChannelAsync(SerializableGuild.SystemChannels.CreateRoomChannelName, x => x.CategoryId = catRoom.Id);
            }
            else if(!SerializableGuild.RoomsEnable)
            {
                if (RoomsCategoryChannel() != null)
                    await RoomsCategoryChannel().DeleteAsync();
                if (CreateRoomChannel() != null)
                    await CreateRoomChannel().DeleteAsync();
            }

            if (SerializableGuild.ContentEnable && ContentCategoryChannel() == null)
            {
                var catCont = await Guild.CreateCategoryChannelAsync(SerializableGuild.SystemCategories.ContentCategoryName);
                await Guild.CreateTextChannelAsync(SerializableGuild.SystemChannels.LinksChannelName, x => x.CategoryId = catCont.Id);
                await Guild.CreateTextChannelAsync(SerializableGuild.SystemChannels.VideosChannelName, x => x.CategoryId = catCont.Id);
            }
            else if(!SerializableGuild.ContentEnable)
            {
                if (ContentCategoryChannel() != null)
                    await ContentCategoryChannel().DeleteAsync();
                if (VideosTextChannel() != null)
                    await VideosTextChannel().DeleteAsync();
                if (LinksTextChannel() != null)
                    await LinksTextChannel().DeleteAsync();
            }
        }

        public async Task SortSystemCategories()
        {            
            var systemCategories = SystemCategories();
            int argPos = 0;

            foreach (var category in systemCategories)
                await category.ModifyAsync(x => x.Position = argPos++);
        }        

        public bool ExistChannelByName(SocketGuildChannel channel)
        {
            string name = channel.Name;
            var guild = channel.Guild;
            bool val = false;

            foreach (var ch in guild.Channels)
            {
                if (ch.Name == name)
                { 
                    val = true;
                    continue;
                }                 
            }

            return val;
        }
    }
}