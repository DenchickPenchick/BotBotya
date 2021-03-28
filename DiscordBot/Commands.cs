//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using DiscordBot.Attributes;
using DiscordBot.Interactivities;
using DiscordBot.MusicOperations;
using DiscordBot.Providers;
using DiscordBot.Providers.Entities;
using DiscordBot.Serializable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Victoria;
using Console = Colorful.Console;

namespace DiscordBot
{ 
    #region --СТРУКТУРЫ--
    public class SentenceStruct
    {
        public string prompt { get; set; }
        public int length { get; set; }
        public int num_samples { get; set; }
    }
    #endregion

    public class Commands : InteractiveBase
    {
        private readonly Bot Bot;

        private readonly LavaOperations LavaOperations;
        private readonly PaginatingService PaginatingService;
        private SerializableConfig Configuration { get => FilesProvider.GetConfig(); }

        private SocketGuild GetSupportGuild() => Context.Client.GetGuild(Configuration.SupportGuildId);

        public Commands(LavaOperations lavaOperations, Bot bot, PaginatingService paginating)
        {
            LavaOperations = lavaOperations;
            PaginatingService = paginating;
            Bot = bot;
        }

        public CommandService GetCommands() => Bot.Commands;

        #region --СТАНДАРТНЫЕ КОМАНДЫ--
        //[Command("Хелп", RunMode = RunMode.Async)]
        //[Alias("Хэлп", "Помощь")]
        //[StandartCommand]
        //[Summary("позволяет узнать полный список команд")]
        //public async Task Help(int page = 0)
        //{
        //    int pos = 0;
        //    int posit = 1;
        //    int catpos = 0;
        //    var serGuild = FilesProvider.GetGuild(Context.Guild);

        //    List<string> pages = new List<string>
        //    {
        //        null
        //    };

        //    CommandCategoryAttribute prevCategoryAttribute = new StandartCommandAttribute();

        //    var commandsArray = Bot.Commands.Commands.Select(x => x.Name).Distinct().ToList();

        //    foreach (var command in Bot.Commands.Commands)
        //    {
        //        CommandCategoryAttribute categoryAttribute;

        //        string aliases = "\nПсевдонимы:";
        //        string parameters = "\nПараметры:";

        //        foreach (string alias in command.Aliases)
        //            if (alias != command.Name.ToLower())
        //                aliases += $" `{alias}`";

        //        foreach (var param in command.Parameters)
        //            parameters += $" `{param.Name}`";

        //        if (command.Attributes.Contains(new StandartCommandAttribute()))
        //            categoryAttribute = new StandartCommandAttribute();
        //        else if (command.Attributes.Contains(new CustomisationCommandAttribute()))
        //            categoryAttribute = new CustomisationCommandAttribute();
        //        else if (command.Attributes.Contains(new MusicCommandAttribute()))
        //            categoryAttribute = new MusicCommandAttribute();
        //        else if (command.Attributes.Contains(new RolesCommandAttribute()))
        //            categoryAttribute = new RolesCommandAttribute();
        //        else if (command.Attributes.Contains(new ConsoleCommandsAttribute()))
        //            categoryAttribute = new ConsoleCommandsAttribute();
        //        else if (command.Attributes.Contains(new AutoPartnershipAttribute()))
        //            categoryAttribute = new AutoPartnershipAttribute();
        //        else
        //            categoryAttribute = new StandartCommandAttribute();

        //        if (prevCategoryAttribute.CategoryName != categoryAttribute.CategoryName || $"{pages[pos]}\n{posit + 1}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}{(command.Parameters.Count > 0 ? parameters : null)}{(command.Aliases.Count > 1 ? aliases : null)}".Length >= 1023)
        //        {
        //            if (prevCategoryAttribute.CategoryName != categoryAttribute.CategoryName)
        //                posit = 1;

        //            pages.Add($"\n**{categoryAttribute.CategoryName}**\n{posit++}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}{(command.Parameters.Count > 0 ? parameters : null)}{(command.Aliases.Count > 1 ? aliases : null)}");
        //            pos++;
        //        }
        //        else if (catpos == 0)
        //        {
        //            pages[0] += $"\n**{categoryAttribute.CategoryName}**\n{posit++}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}{(command.Parameters.Count > 0 ? parameters : null)}{(command.Aliases.Count > 1 ? aliases : null)}";
        //            catpos++;
        //        }
        //        else
        //            pages[pos] += $"\n{posit++}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}{(command.Parameters.Count > 0 ? parameters : null)}{(command.Aliases.Count > 1 ? aliases : null)}";
        //        prevCategoryAttribute = categoryAttribute;
        //    }
        //    if (page > 0)
        //    {
        //        if (page > pages.Count)
        //            await ReplyAsync(embed: new EmbedBuilder
        //            {
        //                Title = "Справка по командам",
        //                Description = pages.Last(),
        //                Color = ColorProvider.GetColorForCurrentGuild(serGuild)
        //            }.Build());
        //        else
        //            await ReplyAsync(embed: new EmbedBuilder
        //            {
        //                Title = "Справка по командам",
        //                Description = pages[page - 1],
        //                Color = ColorProvider.GetColorForCurrentGuild(serGuild)
        //            }.Build());
        //    }
        //    else
        //    {                
        //        await PaginatingService.SendPaginatedMessageAsync(new PaginatorEntity
        //        {
        //            Title = "Справка по командам",
        //            Color = ColorProvider.GetColorForCurrentGuild(serGuild),
        //            Pages = pages
        //        }, Context);                                        
        //    }                        
        //}

        [Command("Хелп")]
        [Alias("Хэлп", "Помощь")]
        [StandartCommand]
        [Summary("позволяет узнать полный список команд")]
        public async Task Help()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);            

            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Хелп",
                Description = "Ты можешь конкретней написать, по чему ты хочешь получить справку.",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Стандартные команды",
                        Value = $"`{serGuild.Prefix}Хелп Стандарт`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Взаимопиар",
                        Value = $"`{serGuild.Prefix}Хелп Пиар`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Кастомизация",
                        Value = $"`{serGuild.Prefix}Хелп Кастомизация`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Музыка",
                        Value = $"`{serGuild.Prefix}Хелп Музыка`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Магазин ролей",
                        Value = $"`{serGuild.Prefix}Хелп Магазин`"
                    },
                    new EmbedFieldBuilder
                    { 
                        Name = "Консольные команды",
                        Value = $"`{serGuild.Prefix}Хелп Консоль`"
                    }
                },
                Color = ColorProvider.GetColorForCurrentGuild(Context.Guild),
                Author = new EmbedAuthorBuilder
                {
                    Name = "Ботя",
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                    Url = "https://botbotya.ru/Pages/CommandsPage/index.html"
                }
            }.Build());
        }

        [Command("Хелп")]
        [Alias("Хэлп", "Помощь")]
        [StandartCommand]
        [Summary("позволяет узнать полный список команд")]
        public async Task Help(string query)
        {
            var allCommands = GetCommands().Commands;
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            string help = null;
            var pages = new PaginatorEntity
            {
                Color = ColorProvider.GetColorForCurrentGuild(Context.Guild),
                Title = "Хелп"
            };

            IEnumerable<CommandInfo> enumerableOfCommands = null;

            switch (query.ToLower())
            {
                case "стандарт":
                    enumerableOfCommands = allCommands.Where(x => x.Attributes.Contains(new StandartCommandAttribute()));                   
                    break;
                case "пиар":
                    enumerableOfCommands = allCommands.Where(x => x.Attributes.Contains(new AutoPartnershipAttribute()));
                    break;
                case "кастомизация":
                    enumerableOfCommands = allCommands.Where(x => x.Attributes.Contains(new CustomisationCommandAttribute()));
                    break;
                case "музыка":
                    enumerableOfCommands = allCommands.Where(x => x.Attributes.Contains(new MusicCommandAttribute()));
                    break;
                case "магазин":
                    enumerableOfCommands = allCommands.Where(x => x.Attributes.Contains(new RolesCommandAttribute()));
                    break;
                case "консоль":
                    enumerableOfCommands = allCommands.Where(x => x.Attributes.Contains(new ConsoleCommandsAttribute()));
                    break;
                default:
                    await ReplyAsync("Такого типа команд не существует.");
                    return;
            }

            if (enumerableOfCommands != null)
            {
                enumerableOfCommands
                    .ToList()
                    .ForEach(x =>
                    {
                        StringBuilder commandBuilder = new(100, maxCapacity: 1024);
                        var parameters = x.Parameters;

                        commandBuilder.Append($"\n`{serGuild.Prefix}{x.Name}`");
                        parameters.ToList().ForEach(k => commandBuilder.Append($" *`{(k.IsOptional ? "|" : null)}{k.Name}{(k.IsOptional ? "|" : null)}`*"));
                        commandBuilder.Append("\n*Описание:*\n" + x.Summary.First().ToString().ToUpper() + x.Summary.Remove(0, 1));
                        commandBuilder.Append("\n────────────────────────");

                        pages += commandBuilder.ToString();
                    });

                if (pages.Pages.Count() > 0)                
                    await PaginatingService.SendPaginatedMessageAsync(pages, Context);                
                else
                    await ReplyAsync(embed: new EmbedBuilder
                    { 
                        Title = "Хелп",
                        Description = help,
                        Color = ColorProvider.GetColorForCurrentGuild(Context.Guild)
                    }.Build());
            }
            else
                await ReplyAsync("Произошла ошибка");
        }        

        [Command("Статистика")]
        [StandartCommand]
        [Alias("Инфо")]
        [Summary("позволяет узнать мои статистику")]
        public async Task Info()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var client = Bot.Client;

            int members = 0;

            foreach (var guild in client.Guilds)
                members += guild.Users.Count;

            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Статистика бота",
                Description = $"Я сейчас нахожусь на {client.Guilds.Count} серверах.\nМною пользуются {members} человек.",
                Color = ColorProvider.GetColorForCurrentGuild(serGuild),
                ThumbnailUrl = client.CurrentUser.GetAvatarUrl()
            }.Build());
        }

        [Command("СерверПоддержки")]
        [Alias("Поддержка")]
        [StandartCommand]
        [Summary("получает приглашение на мой сервер поддержки.")]
        public async Task MyServer() => await ReplyAsync("https://discord.gg/p6R4yk7uqK");

        [Command("Апгрейднуть")]
        [Alias("Прокачать")]
        [StandartCommand]
        [Summary("позволяет апргейднуть бота")]
        public async Task RepositoryAd() => await ReplyAsync(embed: new EmbedBuilder
        {
            Title = "Хочешь прокачать меня?",
            Description = "Ты можешь это очень легко сделать. Но как? Есть два варианта:",
            Fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    Name = "Предлагать свои идеи напрямую разработчику",
                    Value = "Ты можешь зайти на [сервер поддержки](https://discord.gg/p6R4yk7uqK) и рассказать о своей идее разработчику бота!"
                },
                new EmbedFieldBuilder
                {
                    Name = "Стать контрибутором",
                    Value = "Если ты умеешь программировать на *C#* и знаешь как работать с *[Discord.NET](https://github.com/discord-net/Discord.Net)*, тогда ты можешь попробовать создать свой модуль или команду!\nКак это сделать?\n" +
                    "Зайди в [репозиторий бота](https://github.com/DenVot/BotBotya), придумай какую-нибудь крутую вещь для бота и сделай Pull-request в репозиторий бота! И все! Если тебе будет что-то непонятно, ты можешь прочесть [wiki](https://github.com/DenVot/BotBotya/wiki) или же можешь зайти на [сервер поддержки](https://discord.gg/p6R4yk7uqK) и задать вопрос."
                }
            },
            Color = ColorProvider.GetColorForCurrentGuild(Context.Guild),
            ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
        }.Build());

        [Command("ДобавитьБота")]
        [StandartCommand]
        [Alias("Добавить")]
        [Summary("получает ссылку-приглашение меня на твой сервер")]
        public async Task InviteLink() => await ReplyAsync("*Перейди по ссылке и пригласи меня*\n https://discord.com/oauth2/authorize?client_id=749991391639109673&scope=bot&permissions=1573583991");

        [Command("БыстрыйСтарт")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [StandartCommand]
        [Alias("Шаблон", "Конструктор")]
        [Summary("создает каналы в соответствии с шаблоном. Возможные шаблоны:\n1. Игровой (подойдет для игровых серверов)\n2. Группа (подойдет для сообщества)\n3. Учебная (подойдет для групп для одноклассников/однокурсников)\n4. Стандарт (создет дефолтный сервер)\nЕсли ты хочешь удалить каналы, тогда введи токен** `r` **после типа")]
        public async Task FastStart(string start, string token = null)
        {
            start = start.ToLower();
            if (start != "игровой" && start != "группа" && start != "учебная" && start != "стандарт")
            {
                await ReplyAsync("Ты указал неверный тип сервера. Повтори попытку.");
                return;
            }
            if (token == "r")
                Context.Guild.Channels.ToList().ForEach(async x => await x.DeleteAsync());

            var guild = Context.Guild;
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            switch (start.ToLower())
            {
                case "игровой":
                    var talkingCat = await guild.CreateCategoryChannelAsync("ОБЩЕНИЕ");
                    var mediaCat = await guild.CreateCategoryChannelAsync("МЕДИА");
                    var gamingCat = await guild.CreateCategoryChannelAsync("GAMES");
                    var audioCat = await guild.CreateCategoryChannelAsync("ГОЛОСОВЫЕ КАНАЛЫ");
                    var defaultTextChannel = await guild.CreateTextChannelAsync("💬┋Основной", x => x.CategoryId = talkingCat.Id);
                    var contentChannel = await guild.CreateTextChannelAsync("🌐┋Контент", x => x.CategoryId = mediaCat.Id);
                    await guild.CreateTextChannelAsync("CS:GO", x => x.CategoryId = gamingCat.Id);
                    await guild.CreateTextChannelAsync("Dota", x => x.CategoryId = gamingCat.Id);
                    await guild.CreateTextChannelAsync("SoT", x => x.CategoryId = gamingCat.Id);
                    await guild.CreateTextChannelAsync("DST", x => x.CategoryId = gamingCat.Id);
                    await guild.CreateTextChannelAsync("Другие", x => x.CategoryId = gamingCat.Id);
                    var createRoomChannel = await guild.CreateVoiceChannelAsync("➕Войти в игру", x => x.CategoryId = audioCat.Id);

                    serGuild.SystemChannels.CreateRoomChannelId = createRoomChannel.Id;
                    serGuild.SystemChannels.LinksChannelId = serGuild.SystemChannels.VideosChannelId = contentChannel.Id;
                    serGuild.SystemCategories.VoiceRoomsCategoryId = audioCat.Id;
                    serGuild.CheckingContent = true;
                    serGuild.EmojiOfRoom = "🎮";
                    FilesProvider.RefreshGuild(serGuild);
                    await defaultTextChannel.SendMessageAsync($"Я успешно завершил настройку сервера {guild.Name}.");
                    break;
                case "группа":
                    var liveCat = await guild.CreateCategoryChannelAsync("LIVE");
                    var audCat = await guild.CreateCategoryChannelAsync("Обсуждения");
                    var voiceChats = await guild.CreateCategoryChannelAsync("Голосовые");
                    var roomsChats = await guild.CreateCategoryChannelAsync("Комнаты");

                    var rulesChat = await guild.CreateTextChannelAsync("📕┋rules", x => x.CategoryId = liveCat.Id);
                    var anonChat = await guild.CreateTextChannelAsync("📣┋уведомления", x => x.CategoryId = liveCat.Id);
                    var logsChat = await guild.CreateTextChannelAsync("🔎┋логи", x => x.CategoryId = liveCat.Id);
                    var mainAudChat = await guild.CreateTextChannelAsync("💬┋Основной", x => x.CategoryId = audCat.Id);
                    await guild.CreateVoiceChannelAsync("ГС #1", x => x.CategoryId = voiceChats.Id);
                    await guild.CreateVoiceChannelAsync("ГС #2", x => x.CategoryId = voiceChats.Id);
                    await guild.CreateVoiceChannelAsync("ГС #3", x => x.CategoryId = voiceChats.Id);
                    var createRoomChannelForGroup = await guild.CreateVoiceChannelAsync("➕Создать комнату", x => x.CategoryId = roomsChats.Id);

                    serGuild.SystemChannels.CreateRoomChannelId = createRoomChannelForGroup.Id;
                    serGuild.SystemCategories.VoiceRoomsCategoryId = roomsChats.Id;
                    serGuild.SystemChannels.LogsChannelId = logsChat.Id;
                    serGuild.EmojiOfRoom = "🖌";
                    FilesProvider.RefreshGuild(serGuild);
                    await mainAudChat.SendMessageAsync($"Я успешно завершил настройку сервера {guild.Name}.");
                    break;
                case "учебная":
                    var studentsCat = await guild.CreateCategoryChannelAsync("Общение");
                    var schoolCat = await guild.CreateCategoryChannelAsync("Учеба");
                    var chillCat = await guild.CreateCategoryChannelAsync("Отдых");
                    var roomsCatForStudents = await guild.CreateCategoryChannelAsync("Голосовые каналы");

                    var defChannel = await guild.CreateTextChannelAsync("💬┋Основной", x => x.CategoryId = studentsCat.Id);
                    await guild.CreateTextChannelAsync("📚┋дз", x => x.CategoryId = schoolCat.Id);
                    await guild.CreateTextChannelAsync("📝┋расписание", x => x.CategoryId = schoolCat.Id);
                    await guild.CreateTextChannelAsync("💬┋Основной", x => x.CategoryId = chillCat.Id);
                    var createRoomChForSt = await guild.CreateVoiceChannelAsync("➕Создать комнату", x => x.CategoryId = roomsCatForStudents.Id);

                    serGuild.SystemChannels.CreateRoomChannelId = createRoomChForSt.Id;
                    serGuild.SystemCategories.VoiceRoomsCategoryId = roomsCatForStudents.Id;
                    serGuild.EmojiOfRoom = "📚";
                    FilesProvider.RefreshGuild(serGuild);
                    await defChannel.SendMessageAsync($"Я успешно завершил настройку сервера {guild.Name}.");
                    break;
                case "стандарт":
                    var generalText = await guild.CreateCategoryChannelAsync("Текст");
                    var generalVoice = await guild.CreateCategoryChannelAsync("Голосовые");

                    var defCh = await guild.CreateTextChannelAsync("💬┋Основной", x => x.CategoryId = generalText.Id);
                    var createDefChannel = await guild.CreateVoiceChannelAsync("➕Создать комнату", x => x.CategoryId = generalVoice.Id);

                    serGuild.SystemChannels.CreateRoomChannelId = createDefChannel.Id;
                    serGuild.SystemCategories.VoiceRoomsCategoryId = generalVoice.Id;
                    serGuild.EmojiOfRoom = "🎤";
                    FilesProvider.RefreshGuild(serGuild);
                    await defCh.SendMessageAsync($"Я успешно завершил настройку сервера {guild.Name}.");
                    break;
            }
        }

        [Command("Очистить", RunMode = RunMode.Async)]
        [StandartCommand]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [Summary("позволяет очистить сообщений (до 100). Если сообщения отправлены более двух недель назад, то эти сообщения не удалятся.")]
        public async Task Clear(int count)
        {
            if (count <= 100 && count > 0)
            {
                if (count == 100 || count == 99)
                    count = 98;

                var deleteMessagesThread = new Thread(new ParameterizedThreadStart(ClearMessages));
                deleteMessagesThread.Start(count);
            }
            else
            {
                var errMess = await ReplyAsync("Ты не можешь удалять более **100 сообщений** за раз");
                await Task.Delay(1000);
                await errMess.DeleteAsync();
            }
        }

        [Command("Жалоба", RunMode = RunMode.Async)]
        [StandartCommand]
        [Summary("отпраляет жалобу на участника сервера.")]
        public async Task ReportUser(SocketGuildUser user, params string[] reason)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            if (Context.User.Id != user.Id)
            {
                string reasonForm = null;

                for (int i = 0; i < reason.Length; i++)
                {
                    if (i > 0)
                        reasonForm += $" {reason[i]}";
                    else if (i == 0)
                        reasonForm += reason[i];
                }

                var embedToAdmin = new EmbedBuilder
                {
                    Title = $"Поступила жалоба на участника {user.Username}",
                    Description = $"**Причина:**\n`{reasonForm}`",
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = Context.Guild.Name,
                        IconUrl = Context.Guild.IconUrl
                    }
                }.Build();

                var embedToReportedUser = new EmbedBuilder
                {
                    Title = $"На тебя поступила жалоба от {Context.User.Username}",
                    Description = $"**Причина:**\n`{reasonForm}`",
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = Context.Guild.Name,
                        IconUrl = Context.Guild.IconUrl
                    }
                }.Build();

                await Context.Guild.Owner.GetOrCreateDMChannelAsync().Result.SendMessageAsync(embed: embedToAdmin);
                await user.GetOrCreateDMChannelAsync().Result.SendMessageAsync(embed: embedToReportedUser);
            }
            else
                await ReplyAsync("Ты не можешь отправить сам себе жалобу");
        }

        [RequireUserPermission(GuildPermission.KickMembers)]
        [Command("Кик")]
        [StandartCommand]
        [Summary("позволяет кикнуть пользователя с сервера.")]
        public async Task Kick(SocketGuildUser user)
        {
            var maxContextRole = (Context.User as SocketGuildUser).Roles.OrderBy(x => x.Position).Last();
            var userMaxRole = user.Roles.OrderBy(x => x.Position).Last();

            if (maxContextRole.Position == userMaxRole.Position)
            {
                await ReplyAsync("Ты не можешь его/ее кикнуть, т.к. ты стоишь в ролевой иерархии вместе с ним/ней.");
                return;
            }

            if (maxContextRole.Position < userMaxRole.Position)
            {
                await ReplyAsync("Ты не можешь его/ее кикнуть, т.к. ты стоишь ниже него/ее в ролевой иерархии.");
                return;
            }

            if (user != null)
            {
                await user.KickAsync();
                await ReplyAsync($"Я кикнул {user.Username}.");
            }
            else
                await ReplyAsync($"Пользователь не найден.");
        }

        [RequireUserPermission(GuildPermission.BanMembers)]
        [Command("Бан")]
        [Alias("Б")]
        [Summary("позволяет забанить пользователя на сервере.")]
        public async Task Ban(SocketGuildUser user)
        {
            var maxContextRole = (Context.User as SocketGuildUser).Roles.OrderBy(x => x.Position).Last();
            var userMaxRole = user.Roles.OrderBy(x => x.Position).Last();

            if (maxContextRole.Position == userMaxRole.Position)
            {
                await ReplyAsync("Ты не можешь его забанить, т.к. ты стоишь в ролевой иерархии вместе с ним/ней.");
                return;
            }

            if (maxContextRole.Position < userMaxRole.Position)
            {
                await ReplyAsync("Ты не можешь его забанить, т.к. ты стоишь ниже него/ее в ролевой иерархии.");
                return;
            }

            if (user != null)
            {
                await user.BanAsync();
                await ReplyAsync($"Я забанил {user.Username}.");
            }
            else
                await ReplyAsync($"Пользователь не найден. Проверь данные.");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("МедленныйРежим")]
        [StandartCommand]
        [Summary("позволяет включить медленный режим в канале.")]
        public async Task ChangePosOfSlowMode(int time = 0)
        {
            if (time >= 0 && time <= 21600)
            {
                var channel = Context.Channel as SocketTextChannel;
                await channel.ModifyAsync(x => x.SlowModeInterval = time);
                if (channel.SlowModeInterval == 0)
                    await ReplyAsync($"В канале {Context.Channel.Name} отключен медленный режим");
                else
                    await ReplyAsync($"В канале {Context.Channel.Name} включен медленный режим. Интервал: {time} секунд");
            }
            else if (time < 0)
                await ReplyAsync("Интервал не может быть отрицательным");
            else if (time > 21600)
                await ReplyAsync("Интервал не может быть больше 21600 секунд.");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("Рассылка")]
        [StandartCommand]
        [Summary("делает рассылку сообщений всем участникам сервера")]
        public async Task SendMessages(params string[] mess)
        {
            var content = Context.Message.Content;
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            string message = content.Remove(0, serGuild.Prefix.Length + "рассылка".Length);
            string noMessageToUsers = null;

            foreach (var user in Context.Guild.Users)
            {
                if (!user.IsBot)
                {
                    try
                    {
                        var ch = await user.GetOrCreateDMChannelAsync();
                        if (ch != null)
                            await ch.SendMessageAsync(embed: new EmbedBuilder
                            {
                                Description = message,
                                Color = ColorProvider.GetColorForCurrentGuild(Context.Guild),
                                Footer = new EmbedFooterBuilder
                                {
                                    Text = Context.Guild.Name,
                                    IconUrl = Context.Guild.IconUrl
                                },
                                Author = new EmbedAuthorBuilder
                                {
                                    Name = Context.User.Username,
                                    IconUrl = Context.User.GetAvatarUrl()
                                }
                            }.Build());
                    }
                    catch (Exception)
                    {
                        noMessageToUsers += $"\n{user.Username}";
                    }
                }

            }
            await ReplyAsync("Рассылка произведена успешно.");
            if (noMessageToUsers != null)
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Список кому не дошло сообщение",
                    Description = noMessageToUsers,
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                }.Build());
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("СнятьПредупреждения")]
        [Alias("Амнистия")]
        [Summary("снимает предупреждения с участника сервера")]
        public async Task NoWarns(SocketGuildUser user)
        {
            var provider = new GuildProvider(Context.Guild);

            provider.SetWarns(user, 0);

            await ReplyAsync($"Предупреждения с {user.Mention} сняты.");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("УстановитьПредупреждения")]
        [Summary("установливает предупреждения у участника сервера")]
        public async Task SetWarns(SocketGuildUser user, int warns)
        {
            var provider = new GuildProvider(Context.Guild);

            provider.SetWarns(user, warns);

            await ReplyAsync($"Установлено.");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("ДобавитьПредупреждения")]
        [Summary("добавляет предупреждения участнику сервера.")]
        public async Task PlusWarns(SocketGuildUser user, int count)
        {
            var provider = new GuildProvider(Context.Guild);

            provider.PlusWarns(user, count);

            await ReplyAsync($"Добавлено.");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("УбратьПредупреждения")]
        [Summary("добавляет предупреждения участнику сервера.")]
        public async Task MinusWarns(SocketGuildUser user, int count)
        {
            var provider = new GuildProvider(Context.Guild);

            provider.MinusWarns(user, count);

            await ReplyAsync($"Убрано.");
        }

        [Command("МоиПредупреждения")]
        [Summary("показывает количество твоих предупреждений.")]
        public async Task MyWarns()
        {
            if (Context.User is SocketGuildUser user)
            {
                var provider = new GuildProvider(Context.Guild);
                var bad = provider.GetBadUser(user);

                if (bad.Item1 > 0)
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "Предупреждения:",
                        Description = bad.Item2.ToString(),
                        Color = ColorProvider.GetColorForCurrentGuild(Context.Guild)
                    }.Build());
                else
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "Предупреждения:",
                        Description = 0.ToString(),
                        Color = ColorProvider.GetColorForCurrentGuild(Context.Guild)
                    }.Build());
            }
        }

        [Command("БотДавай")]
        [Alias("БотПродолжай", "БотГо")]
        [StandartCommand]
        [Summary("попытается продолжить предложение с помощью [нейросети](https://porfirevich.ru).")]
        public async Task ContinueSentence(params string[] sentence)
        {
            if (sentence.Length == 0)
                await ReplyAsync("Не указан текст сообщения для продолжения.");
            else
            {
                // Длина сообщения по умолчанию.
                int length = 30;

                string sentence_str = string.Join(" ", sentence);

                if (sentence_str.Length < 30)
                    length = sentence_str.Length;

                // Структура объекта JSON для отправки на сервер.
                var account = new SentenceStruct
                {
                    prompt = HttpUtility.HtmlEncode(sentence_str),
                    length = length,
                    num_samples = 1
                };

                var request = (HttpWebRequest)WebRequest.Create("https://pelevin.gpt.dobro.ai/generate/");

                // Наши данные.
                var data = JsonConvert.SerializeObject(account, Formatting.Indented);

                // Преобразуем данные в массив байтов.
                byte[] data_array = Encoding.UTF8.GetBytes(data);

                // Устанавливаем заголовок Content-Length.
                request.ContentLength = data_array.Length;

                // Автоматическая декомпрессия GZIP ответа.
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                request.Method = "POST";

                // Заголовки для отправки запроса.
                request.ContentType = "Content-Type: text/plain;charset=UTF-8";
                request.Headers.Add("Host: pelevin.gpt.dobro.ai");
                request.Headers.Add("Connection: keep-alive");
                request.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36");
                request.Headers.Add("Accept: */*");
                request.Headers.Add("Origin: https://porfirevich.ru");
                request.Headers.Add("Sec-Fetch-Site: cross-site");
                request.Headers.Add("Sec-Fetch-Mode: cors");
                request.Headers.Add("Sec-Fetch-Dest: empty");
                request.Headers.Add("Referer: https://porfirevich.ru/");
                request.Headers.Add("Accept-Language: en-US,en;q=0.9");
                request.Headers.Add("Accept-Encoding: gzip, deflate");

                using (var request_stream = request.GetRequestStream())
                {
                    request_stream.Write(data_array, 0, data_array.Length);
                }

                var response = await request.GetResponseAsync();

                using (var response_stream = response.GetResponseStream())
                {
                    using (var stream_reader = new StreamReader(response_stream))
                    {
                        var response_data = stream_reader.ReadToEnd();

                        if (string.IsNullOrEmpty(response_data))
                            await ReplyAsync("Ответ с сервера ничего не вернул.");
                        else
                        {
                            // Парсинг объекта из ответа.
                            var json_obj = JObject.Parse(response_data);

                            if (json_obj == null)
                                await ReplyAsync("Объект JSON вернул null.");
                            else
                            {
                                var included_data = (JArray)json_obj["replies"];
                                var text = included_data[0].Value<string>();

                                if (string.IsNullOrEmpty(text))
                                    await ReplyAsync("В ответе пусто.");
                                else
                                    await ReplyAsync($"{sentence_str.First().ToString().ToUpper() + sentence_str[1..]} {HttpUtility.HtmlDecode(text)}");
                            }
                        }
                    }
                }

                response.Close();
            }
        }

        //В разработке
        //[Command("ПройтиОпрос", RunMode = RunMode.Async)]
        //[StandartCommand]
        //[Summary("позволяет обучать бота для лучшего распознования ошибок")]
        //public async Task EducReq()
        //{
        //    await ReplyAsync("Сейчас я вам покажу предпологаемое слово и покажу это же слово, но с ошибкой. Ответьте, асоциируется ли неправильное слово с исходным?\nДля ответа поставьте **1 (да)** или **0 (нет)**");
        //    var options = FilesProvider.GetGlobalOptions();
        //    var badWords = options.GlobalBadWords;
        //    Random random = new Random();

        //    int index = random.Next(0, badWords.Count);
        //    var badWord = badWords[index];

        //    string content = badWord.Word;
        //    var builder = new EmbedBuilder
        //    {
        //        Title = $"Слово {content}",
        //        Color = ColorProvider.GetColorForCurrentGuild(Context.Guild)
        //    };

        //    //Меньше 50 - удаление
        //    //Больше 50 - добавление            
        //    int operType = new Random().Next(0, 100);
        //    char[] alpha = "абвгдеёжзиклмнопрстуфхцчшщъыьэюя".ToCharArray();

        //    string predException;
        //    if (operType <= 50)
        //        predException = badWord.Word.Remove(random.Next(0, badWord.Word.Length - 1), 1);
        //    else
        //        predException = badWord.Word.Insert(random.Next(0, badWord.Word.Length - 1), alpha[random.Next(0, alpha.Length - 1)].ToString());

        //    builder.WithDescription(predException);
        //    await ReplyAsync(embed: builder.Build());

        //    var nextMess = await NextMessageAsync();
        //    if (nextMess != null)
        //    {
        //        var isVariant = int.TryParse(nextMess.Content, out int variant);
        //        if (isVariant)
        //        {
        //            if (variant == 1)
        //            {
        //                options.GlobalBadWords[index].Exceptions.Add(predException);
        //                FilesProvider.RefreshGlobalOptions(options);
        //            }

        //            await ReplyAsync("Спасибо за участие в опросе!");
        //        }
        //        else
        //            await ReplyAsync("Некорректный формат ответа");
        //    }
        //    else
        //        await ReplyAsync("Ты не ответил в течении 5 минут");
        //}
        #endregion

        #region --ВЗАИМОПИАР--   
        [Command("КаналДляРекламы")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("устанавливает канал, куда будут присылаться чужие объявления. Если канала нет, или бот не может туда писать, или участники не видят его, тогда ты не можешь использовать функцию взаимопиара")]
        public async Task SetAdvChannel(SocketTextChannel channel)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var everyoneRole = Context.Guild.EveryoneRole;
            var overWritePermissionsForEveryone = channel.GetPermissionOverwrite(everyoneRole);

            if (overWritePermissionsForEveryone.HasValue)
            {
                var valueOfPerms = overWritePermissionsForEveryone.Value;

                if (valueOfPerms.ViewChannel == PermValue.Deny)
                {
                    await ReplyAsync($"Роль `everyone` не может просматривать канал {channel.Mention}.");
                    return;
                }

                if (valueOfPerms.ReadMessageHistory == PermValue.Deny)
                {
                    await ReplyAsync($"Роль `everyone` не может просматривать историю сообщений в канале {channel.Mention}.");
                    return;
                }
            }

            serGuild.SystemChannels.AdvertisingChannelId = channel.Id;

            FilesProvider.RefreshGuild(serGuild);

            await ReplyAsync($"Теперь я буду присылать чужие объявления в {channel.Mention}");
        }

        [Command("СоздатьКаналДляРекламы")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("автоматически создает и настраивает канал для чужих объявлений. Является аналогом команды `КаналДляРекламы`")]
        public async Task CreateAdvChannel()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            var advCategory = await Context.Guild.CreateCategoryChannelAsync("💼Партнерство");
            var advChannel = await Context.Guild.CreateTextChannelAsync("💼┋Партнерство", x => x.CategoryId = advCategory.Id);

            await advChannel.AddPermissionOverwriteAsync(Context.Client.CurrentUser, new OverwritePermissions(sendMessages: PermValue.Allow));
            await advChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(sendMessages: PermValue.Deny, viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow));

            serGuild.SystemChannels.AdvertisingChannelId = advChannel.Id;

            FilesProvider.RefreshGuild(serGuild);

            await ReplyAsync($"Я создал канал для рекламы {advChannel.Mention}.");
            await advChannel.SendMessageAsync("**СЮДА БУДУТ ПРИСЫЛАТЬСЯ ОБЪЯВЛЕНИЯ С ДРУГИХ DISCORD СЕРВЕРОВ**");
        }

        [Command("ОтправитьНаПроверку")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("отсылает на проверку твое объявление")]
        public async Task SendToModer()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var globalOptions = FilesProvider.GetGlobalOptions();
            var supportGuild = GetSupportGuild();

            var supportAdmin = supportGuild.GetUser(Configuration.AdminId);

            //Защита от спама
            if (serGuild.AdvertisingModerationSended)
            {
                await ReplyAsync("Твое объявление до сих пор рассматривается");
                return;
            }
            if (serGuild.AdvertisingAccepted)
            {
                await ReplyAsync("Твое объявление уже проверено");
                return;
            }
            if (supportGuild == null || supportAdmin == null)
            {
                await ReplyAsync("В данный момент функция автопиара не работает. Повтори попытку позже.");
                return;
            }

            if (serGuild.Advert.Title == null)
            {
                await ReplyAsync("У объявления должен быть заголовок");
                return;
            }

            if (serGuild.Advert.Description == null)
            {
                await ReplyAsync("У объявления должно быть описание");
                return;
            }

            if (serGuild.Advert.AdvColor == null)
            {
                await ReplyAsync("У объявления должен быть цвет");
                return;
            }

            if (serGuild.Advert.InviteUrl == "NONE")
            {
                await ReplyAsync("У объявления должно быть приглашение");
                return;
            }

            var subDate = DateTime.Now.ToUniversalTime().Subtract(serGuild.NextCheck);

            if (subDate.Hours < 0)
            {
                int hours = subDate.Hours * -1;
                int minutes = subDate.Minutes * -1;
                int seconds = subDate.Seconds * -1;
                char lastCharH = hours.ToString().Last();
                char lastCharM = minutes.ToString().Last();
                char lastCharS = seconds.ToString().Last();

                await ReplyAsync($"Вы не можете отсылать на проверку свое объявление больше чем одно в 3 часа с момента последней проверки.\n*Осталось:* `{hours} {(lastCharH == '1' ? "час" : "часа")} " +
                    $"{minutes} {(lastCharM == '1' ? "минута" : lastCharM == '2' || lastCharM == '3' || lastCharM == '4' ? "минуты" : "минут")} " +
                    $"{seconds} {(lastCharS == '1' ? "секунда" : lastCharS == '2' || lastCharS == '3' || lastCharS == '4' ? "секунды" : "секунд")}`");
                return;
            }

            var adminDM = await supportAdmin.GetOrCreateDMChannelAsync();

            var advBuilder = serGuild.Advert.GetBuilder(Context.Guild);

            advBuilder.WithFooter(new EmbedFooterBuilder
            {
                Text = Context.Guild.Id.ToString()
            });

            var mess = await adminDM.SendMessageAsync($"Поступило новое объявление на рассмотрение\n**Общая информация:**\n*Количество участников:* {Context.Guild.Users.Count}\n*Создан:* {Context.Guild.CreatedAt.DateTime.ToShortDateString()}\n`✅` - допущено\n`❌` - отказ", embed: advBuilder.Build());
            await mess.AddReactionsAsync(new List<IEmote>
            {
                new Emoji("✅"),
                new Emoji("❌")
            }.ToArray());
            globalOptions.MessagesToCheck.Add(mess.Id);
            serGuild.AdvertisingModerationSended = true;

            FilesProvider.RefreshGlobalOptions(globalOptions);
            FilesProvider.RefreshGuild(serGuild);

            await ReplyAsync("Отправлено, ожидай.");
        }

        [Command("Объявление")]
        [AutoPartnership]
        [Summary("показывает твое объявление")]
        public async Task GetAdv()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            await ReplyAsync(embed: serGuild.Advert.BuildAdvertise(Context.Guild));
        }

        [Command("Разослать")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("разошлет твое объявление по Discord серверам-партнерам")]
        public async Task SendAdv()
        {
            var allSerGuilds = FilesProvider.GetAllGuilds().ToList();
            var currentSerGuild = FilesProvider.GetGuild(Context.Guild);
            var client = Context.Client;

            if (!currentSerGuild.AdvertisingAccepted)
            {
                await ReplyAsync("Ты не допущен к рекламе.");
                return;
            }

            if (Context.Guild.GetTextChannel(currentSerGuild.SystemChannels.AdvertisingChannelId) == null)
            {
                await ReplyAsync("У тебя нет канала для отправки чужих объявлений.");
                return;
            }

            var subDate = DateTime
                .Now
                .ToUniversalTime()
                .Subtract(currentSerGuild.NextSend);

            if (subDate.Hours < 0)
            {
                int hours = subDate.Hours * -1;
                int minutes = subDate.Minutes * -1;
                int seconds = subDate.Seconds * -1;
                char lastCharH = hours
                    .ToString()
                    .Last();
                char lastCharM = minutes
                    .ToString()
                    .Last();
                char lastCharS = seconds
                    .ToString()
                    .Last();

                await ReplyAsync($"Ты не можешь рассылать объявления чаще чем раз в два часа\n*Осталось:* `{hours} {(lastCharH == '1' ? "час" : "часа")} " +
                    $"{minutes} {(lastCharM == '1' ? "минута" : lastCharM == '2' || lastCharM == '3' || lastCharM == '4' ? "минуты" : "минут")} " +
                    $"{seconds} {(lastCharS == '1' ? "секунда" : lastCharS == '2' || lastCharS == '3' || lastCharS == '4' ? "секунды" : "секунд")}`");
                return;
            }

            int count = allSerGuilds.Where(x => client.GetGuild(x.GuildId).GetTextChannel(x.SystemChannels.AdvertisingChannelId) != null).Count() - 1;

            if (count == 0)
            {
                await ReplyAsync("На данный момент нет открытых серверов. Повтори попытку позже");
                return;
            }
            new Thread(() =>
            {
                //DiscordHTTPException
                                
                allSerGuilds.ToList().ForEach(async x =>
                {
                    var advChannel = client.GetGuild(x.GuildId).GetTextChannel(x.SystemChannels.AdvertisingChannelId);

                    try
                    {                       
                        if (advChannel != null && x.GuildId != currentSerGuild.GuildId)
                            await advChannel.SendMessageAsync(embed: currentSerGuild.Advert.BuildAdvertise(Context.Guild));
                    }
                    catch (HttpException ex)
                    {
                        if (ex.DiscordCode == 5001)
                        {
                            var guild = advChannel.Guild;
                            var serGuild = x;

                            serGuild.SystemChannels.AdvertisingChannelId = default;

                            var adminDMChannel = await guild.Owner.GetOrCreateDMChannelAsync();

                            await adminDMChannel.SendMessageAsync(embed: new EmbedBuilder
                            {
                                Title = "Предупреждение",
                                Description = "Я не могу отправить объявление в Ваш канал для рекламы. Чтобы Вы могли рассылать объявления, создайте канал для объявлений.",
                                Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                            }.Build());
                        }
                    }
                });                               
            }).Start();

            char lastChar = count.ToString().ToCharArray().Last();

            currentSerGuild.NextSend = DateTime.Now.ToUniversalTime().AddHours(2);

            FilesProvider.RefreshGuild(currentSerGuild);

            await ReplyAsync($"Я разослал объявление на {count} {(lastChar == '1' ? "сервер" : lastChar == '2' || lastChar == '3' || lastChar == '4' ? "сервера" : "серверов")}.");
        }

        [Command("Заголовок")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("редактирует заголовок объявления")]
        public async Task EditTitle(params string[] titleArray)
        {
            if (titleArray.Length > 0)
            {
                var serGuild = FilesProvider.GetGuild(Context.Guild);
                string description = null;

                titleArray.ToList().ForEach(x => description += $" {x}");
                description = description.Remove(0, 1);
                serGuild.Advert.Title = description;

                serGuild.AdvertisingAccepted = false;

                FilesProvider.RefreshGuild(serGuild);

                await ReplyAsync("Заголовок объявления изменен.", embed: serGuild.Advert.BuildAdvertise(Context.Guild));
            }
            else
                await ReplyAsync("Заголовок объявления не может быть пустым!");
        }

        [Command("Текст")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("редактирует текст объявления")]
        public async Task EditDescription(params string[] descriptionArray)
        {
            if (descriptionArray.Length > 0)
            {
                var serGuild = FilesProvider.GetGuild(Context.Guild);
                string description;
                var content = Context.Message.Content;

                description = content.Remove(0, serGuild.Prefix.Length + "текст".Length);
                serGuild.Advert.Description = description;
                serGuild.AdvertisingAccepted = false;

                FilesProvider.RefreshGuild(serGuild);

                await ReplyAsync("Текст объявления изменен.", embed: serGuild.Advert.BuildAdvertise(Context.Guild));
            }
            else
                await ReplyAsync("Текст объявления не может быть пустым!");
        }

        [Command("Цвет")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("меняет цвет объявления")]
        public async Task EditColor(string color)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            serGuild.Advert.AdvColor = ColorProvider.GetColorFromName(color).ToString();

            FilesProvider.RefreshGuild(serGuild);
            await ReplyAsync("Цвет объявления изменен", embed: serGuild.Advert.BuildAdvertise(Context.Guild));
        }

        [Command("Цвет")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("меняет цвет объявления")]
        public async Task EditColor(int r, int g, int b)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            if (r > 255)
            {
                await ReplyAsync("Значение `r` не может быть больше 255");
                return;
            }

            if (g > 255)
            {
                await ReplyAsync("Значение `g` не может быть больше 255");
                return;
            }

            if (b > 255)
            {
                await ReplyAsync("Значение `b` не может быть больше 255");
                return;
            }

            serGuild.Advert.AdvColor = new Color(r, g, b).ToString();

            Console.WriteLine(new Color(r, g, b).ToString());

            FilesProvider.RefreshGuild(serGuild);
            await ReplyAsync("Цвет объявления изменен", embed: serGuild.Advert.BuildAdvertise(Context.Guild));
        }

        [Command("Картинка")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("меняет картинку у объявления. Если хочешь, чтобы вместо картинки была аватарка сервера, тогда введи букву `G` вместо ссылки. Если ты не хочешь, чтобы была картинка, тогда напиши букву `N` вместо ссылки.")]
        public async Task ChangeThumbnail(string url)
        {
            if (url.ToLower() != "g" && url.ToLower() != "n" && !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                await ReplyAsync("Некорректная ссылка");
                return;
            }
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            serGuild.Advert.ThumbnailUrl = url.ToLower() == "n" ? string.Empty : url;
            serGuild.AdvertisingAccepted = false;

            FilesProvider.RefreshGuild(serGuild);

            await ReplyAsync("Картинка изменена", embed: serGuild.Advert.BuildAdvertise(Context.Guild));
        }

        [Command("ОсновнаяКартинка")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("меняет основную картинку у объявления. Если хочешь, чтобы вместо картинки была аватарка сервера, тогда введи букву `G` вместо ссылки. Если ты не хочешь, чтобы была картинка, тогда напиши букву `N` вместо ссылки.")]
        public async Task ChangeImage(string url)
        {
            if (url.ToLower() != "g" && url.ToLower() != "n" && !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                await ReplyAsync("Некорректная ссылка");
                return;
            }
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            serGuild.Advert.ImageUrl = url.ToLower() == "n" ? string.Empty : url;
            serGuild.AdvertisingAccepted = false;

            FilesProvider.RefreshGuild(serGuild);

            await ReplyAsync("Картинка изменена", embed: serGuild.Advert.BuildAdvertise(Context.Guild));
        }

        [Command("ДобавитьПриглашение")]
        [AutoPartnership]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("добавляет ссылку-приглашение, которое будет вставляться в твое объявление")]
        public async Task InjectInvite(string inviteUrl)
        {
            var allInvites = await Context.Guild.GetInvitesAsync();

            var client = Context.Client;            

            if (allInvites.Any(x => x.Url == inviteUrl))
            {
                var serGuild = FilesProvider.GetGuild(Context.Guild);

                serGuild.Advert.InviteUrl = inviteUrl;

                FilesProvider.RefreshGuild(serGuild);

                await ReplyAsync("Приглашение добавлено", embed:  serGuild.Advert.BuildAdvertise(Context.Guild));
            }
            else
                await ReplyAsync("Я не нашел такого приглашения на твоем сервере");
        }
        #endregion

        #region --КОММУНИКАЦИЯ--
        [Command("ДобавитьСоединение")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("добаляет соединение с каналами других серверов.")]
        [StandartCommand]
        public async Task AddConnection(params ulong[] id)
        {
            FilesProvider.AddConnector(Context.Guild, new SerializableConnector
            {
                HostId = Context.Channel.Id,
                EndPointsId = id.ToList()
            });
            await ReplyAsync("Соединение добавлено.");
        }

        [Command("УдалитьСоединение")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("удаляет соединение с каналами других серверов.")]
        [StandartCommand]
        public async Task DeleteConnection()
        {
            var connectors = FilesProvider.GetConnectors(Context.Guild);
            SerializableConnector connector = null;
            if (connectors != null)
                connector = connectors.SerializableConnectorsChannels.Where(x => x.HostId == Context.Channel.Id).First();

            if (connector != null)
            {
                connectors.SerializableConnectorsChannels.Remove(connector);
                FilesProvider.RefreshConnectors(connectors);
                await ReplyAsync("Удаление произведено успешно");
            }
            else
                await ReplyAsync("В данном канале не существует соединения.");
        }
        #endregion

        #region --РОЛЬ РЕАКЦИЯ--
        [Command("ДобавитьРольЗаРеакцию")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("добавляет реакцию к сообщению. После нажатия на реакцию будет выдана соответствующая роль.")]
        [StandartCommand]
        public async Task AddReaction(ulong id, SocketTextChannel channel, Emoji emoji, IRole role)
        {
            var contextMaxRole = (Context.User as SocketGuildUser).Roles.OrderBy(x => x.Position).Last();

            if (contextMaxRole.Position > role.Position)
            {
                var reactRoleMessages = FilesProvider.GetReactRoleMessages();
                List<ulong> reactRoleMessagesId = reactRoleMessages.ReactRoleMessages.Select(x => x.Id).ToList();

                if (channel.GetMessageAsync(id) == null)
                {
                    await ReplyAsync($"Не могу найти сообщение в {channel.Mention}.");
                    return;
                }

                if (reactRoleMessagesId.Contains(id))
                {
                    FilesProvider.AddReactRoleToReactRoleMessage(id, emoji.Name, role.Id);
                }
                else
                    FilesProvider.AddReactRoleMessage(new SerializableReactRoleMessage
                    {
                        Id = id,
                        EmojiesRoleId = new List<(string, ulong)>
                        {
                            (emoji.Name, role.Id)
                        }
                    });

                await channel.GetMessageAsync(id).Result.AddReactionAsync(emoji);

                await ReplyAsync("Добавлено");
            }
            else
                await ReplyAsync("Ты стоишь либо ниже него/ее в ролевой иерархии, либо на одном с уровне с ним/ней");
        }

        [Command("УдалитьРольЗаРеакцию")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("удаляет \"Роль за реакцию\" у сообщения")]
        [StandartCommand]
        public async Task DeleteReaction(ulong id, Emoji emoji)
        {
            var reactRoleMessages = FilesProvider.GetReactRoleMessages();
            List<ulong> reactRoleMessagesId = reactRoleMessages.ReactRoleMessages.Select(x => x.Id).ToList();

            if (!reactRoleMessagesId.Contains(id))
            {
                await ReplyAsync("Не существует сообщения с данным `id`");
                return;
            }

            FilesProvider.RemoveReactRoleFromReactRoleMessage(id, emoji.Name);

            await ReplyAsync("Удалено");
        }
        #endregion

        #region --МУЗЫКАЛЬНЫЕ КОМАНДЫ--
        [Command("Подключиться")]
        [MusicCommand]
        [Summary("подключает бота к голосовому каналу")]
        public async Task JoinAsync()
        {
            await LavaOperations.JoinAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Покинуть")]
        [MusicCommand]
        [Summary("отключает бота от канала")]
        public async Task LeaveAsync()
        {
            await LavaOperations.LeaveAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Играть")]
        [MusicCommand]
        [Summary("включает трек, который задан url")]
        public async Task PlayTrackAsync(params string[] query)
        {
            if (query == null)
            {
                await ReplyAsync("Ты не указал название трека или ссылку на него");
                return;
            }
            await LavaOperations.PlayTrackAsync(Context.User as SocketGuildUser, query, Context.Channel as SocketTextChannel);
        }

        [Command("Остановить")]
        [MusicCommand]
        [Summary("выключает трек, который задан url")]
        public async Task StopTrackAsync()
        {
            await LavaOperations.StopTrackAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Пауза")]
        [MusicCommand]
        [Alias("Стоп")]
        [Summary("ставит на паузу трек")]
        public async Task PauseTrackAsync()
        {
            await LavaOperations.PauseTrackAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Воспроизведение")]
        [MusicCommand]
        [Alias("Плей", "Плэй")]
        [Summary("продолжает трек, который стоит на паузе")]
        public async Task ResumeTrackAsync()
        {
            await LavaOperations.ResumeTrackAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Громкость")]
        [MusicCommand]
        [Summary("устанавливает громкость бота")]
        public async Task SetVolumeAsync(ushort vol)
        {
            if (vol == 0)
            {
                await ReplyAsync("Ты не поставил значение");
                return;
            }
            await LavaOperations.SetVolumeAsync(Context.User as SocketGuildUser, vol, Context.Channel as SocketTextChannel);
        }

        [Command("Текст")]
        [MusicCommand]
        [Summary("выводит текст проигрываемой песни")]
        public async Task GetLyrics()
        {
            var hasPlayer = LavaOperations.LavaNode.TryGetPlayer(Context.Guild, out LavaPlayer player);
            if (!hasPlayer)
            {
                await ReplyAsync("Нет плеера. Пригласи меня в канал или включи трек.");
                return;
            }
            if (player.Track == null)
            {
                await ReplyAsync("Нет трека.");
                return;
            }

            await ReplyAsync(await player.Track.FetchLyricsFromGeniusAsync(), embed: new EmbedBuilder
            {
                Title = $"Текст песни {player.Track.Title}"
            }.Build());
        }
        #endregion        

        #region --РОЛЕВЫЕ КОМАНДЫ--
        [Command("ДобавитьРоль")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RolesCommand]
        [Summary("добавляет роль на продажу. Ее нужно упомянуть")]
        public async Task AddEconomicRole(int price, IRole role)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            if (!serGuild.BlaskListedRolesToSale.Contains(role.Id))
                if (Context.Message.MentionedRoles.Count > 0)
                {
                    var economProvider = new EconomicProvider(Context.Guild);

                    var res = economProvider.AddRole(role, price);
                    if (res != EconomicProvider.Result.RoleAlreadyAdded)
                        await ReplyAsync("Роль добавлена на продажу.");
                    else
                        await ReplyAsync("Роль уже добавлена на продажу.");
                }
                else
                    await ReplyAsync("Не могу найти роль в сообщении.");
            else
                await ReplyAsync("Я не могу добавить данную роль, т.к. она находится в черном списке.");
        }

        [Command("УбратьРоль")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RolesCommand]
        [Summary("снимает роль с продажи. Ее нужно упомянуть.")]
        public async Task DeleteRole(IRole role)
        {
            if (Context.Message.MentionedRoles.Count > 0)
            {
                var economProvider = new EconomicProvider(Context.Guild);

                var res = economProvider.DeleteRole(role.Id);
                if (res == EconomicProvider.Result.NoRole)
                    await ReplyAsync($"Роли {role.Mention} нет в списке на продажу.");
                else if (res == EconomicProvider.Result.Succesfull)
                    await ReplyAsync($"Роль {role.Mention} снята с продажи.");
            }
            else
                await ReplyAsync("Не могу найти роль в сообщении.");
        }

        [Command("Перевести")]
        [RolesCommand]
        [Summary("осуществляет перевод валюты другому участнику.")]
        public async Task TransferBalance(int count, SocketGuildUser user)
        {
            var economicProvider = new EconomicProvider(Context.Guild);
            var contextEconomUser = economicProvider.GetEconomicGuildUser(Context.User.Id);

            if (contextEconomUser.Item1.Balance - count >= 0)
            {
                economicProvider.MinusBalance(Context.User, count);
                economicProvider.AddBalance(user, count);
                await ReplyAsync("Операция прошла успешно.");
            }
            else
                await ReplyAsync("У тебя недостаточно средств.");
        }

        [Command("КупитьРоль")]
        [RolesCommand]
        [Summary("покупает роль участнику. Ее нужно упомянуть.")]
        public async Task BuyRole(IRole role)
        {
            if (Context.Message.MentionedRoles.Count > 0)
            {
                var economProvider = new EconomicProvider(Context.Guild);
                var res = economProvider.BuyRole(role, Context.User as SocketGuildUser);
                if (Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position > role.Position)
                {
                    if (res == EconomicProvider.Result.NoRole)
                        await ReplyAsync($"Роли {role.Mention} нет в каталоге.");
                    else if (res == EconomicProvider.Result.NoBalance)
                        await ReplyAsync($"У тебя не хватает средств на покупку роли {role.Mention}");
                    else if (res == EconomicProvider.Result.Error)
                        await ReplyAsync($"Произошла ошибка при покупке роли. Можешь обратиться на сервер поддержки.\n**Ссылка:** https://discord.gg/p6R4yk7uqK");
                    else if (res == EconomicProvider.Result.Succesfull)
                        await ReplyAsync($"Роль {role.Mention} куплена");
                }
                else
                    await ReplyAsync("Я не могу выдать тебе эту роль, так как я нахожусь в ролевой иерархии на одном уровне или ниже относительно данной роли.");
            }
            else
                await ReplyAsync("Не могу найти роль в сообщении.");
        }

        [Command("ДобавитьБаланс")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RolesCommand]
        [Summary("позволяет увеличить баланс участника(-м) сервера (его(их) нужно упомянуть).")]
        public async Task AddBalance(int count, params SocketGuildUser[] users)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var economProvider = new EconomicProvider(Context.Guild);

            string usersList = null;

            if (Context.Message.MentionedUsers.Count > 0)
            {
                foreach (var user in users)
                {
                    economProvider.AddBalance(user, count);
                    var economUser = FilesProvider.GetEconomicGuildUser(user);

                    usersList += $"\n{user.Mention} Баланс: {economUser.Balance}";
                }

                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Новый баланс участников сервера",
                    Description = usersList,
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                }.Build());
            }
            else
                await ReplyAsync("Не могу найти участника(-ов) в сообщении.");
        }

        [Command("УменьшитьБаланс")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RolesCommand]
        [Summary("позволяет уменьшить баланс участника(-м) сервера (его(их) нужно упомянуть).")]
        public async Task MinusBalance(int count, params SocketGuildUser[] users)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var economProvider = new EconomicProvider(Context.Guild);

            string usersList = null;

            if (Context.Message.MentionedUsers.Count > 0)
            {
                foreach (var user in users)
                {
                    economProvider.MinusBalance(user, count);
                    var economUser = FilesProvider.GetEconomicGuildUser(user);

                    usersList += $"\n{user.Mention} Баланс: {economUser.Balance}";
                }

                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Новый баланс участников сервера",
                    Description = usersList,
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                }.Build());
            }
            else
                await ReplyAsync("Не могу найти участника(-ов) в сообщении.");
        }

        [Command("УстановитьБаланс")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RolesCommand]
        [Summary("позволяет установить баланс участника(-м) сервера (его(их) нужно упомянуть).")]
        public async Task SetBalance(int count, params SocketGuildUser[] users)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var economProvider = new EconomicProvider(Context.Guild);

            string usersList = null;

            if (Context.Message.MentionedUsers.Count > 0)
            {
                foreach (var user in users)
                {
                    economProvider.SetBalance(user, count);
                    var economUser = FilesProvider.GetEconomicGuildUser(user);
                    usersList += $"\n{user.Mention} Баланс: {economUser.Balance}";
                }

                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Новый баланс участников сервера",
                    Description = usersList,
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                }.Build());
            }
            else
                await ReplyAsync("Не могу найти участника(-ов) в сообщении.");
        }

        [Command("РолиНаПродажу")]
        [RolesCommand]
        [Alias("Витрина")]
        [Summary("позволяет получить список ролей на продажу.")]
        public async Task RolesOnSale()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var economProvider = new EconomicProvider(Context.Guild);

            var roles = economProvider.EconomicGuild.RolesAndCostList;

            List<string> pages = new List<string>
            {
                null
            };

            int index = 0;

            foreach (var role in roles)
            {
                if (pages.Count == 1 || $"{pages[index]}\n{Context.Guild.GetRole(role.Item1).Mention} Цена: {role.Item2}".Length < 2048)
                    pages[0] += $"\n{Context.Guild.GetRole(role.Item1).Mention} Цена: {role.Item2}";
                else if ($"{pages[index]}\n{Context.Guild.GetRole(role.Item1).Mention} Цена: {role.Item2}".Length >= 2048)
                {
                    pages.Add($"\n{Context.Guild.GetRole(role.Item1).Mention} Цена: {role.Item2}");
                    index++;
                }
            }
            if (pages.Count == 1)
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Роли на продажу",
                    Description = pages.First(),
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                }.Build());
            else
                await PagedReplyAsync(new PaginatedMessage
                {
                    Title = "Роли на продажу",
                    Pages = pages,
                    Options = new PaginatedAppearanceOptions
                    {
                        Jump = null,
                        Info = null
                    },
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                });
        }

        [Command("Баланс")]
        [RolesCommand]
        [Summary("показывает твой баланс.")]
        public async Task GetBalance()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var economUser = FilesProvider.GetEconomicGuildUser(Context.User as SocketGuildUser);

            if (economUser != null)
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = $"Баланс {Context.User.Username}: {economUser.Balance}",
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                }.Build());
            }
            else
                await ReplyAsync("У тебя нет собственного счета. Можно отправить **любое** сообщение, чтобы его создать.");
        }

        [Command("НаградаЗаСообщение")]
        [RolesCommand]
        [Alias("Награда")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("позволяет установить награду за каждое отправленное пользователем сообщение.")]
        public async Task RewardForMessage(int count)
        {
            var economGuild = FilesProvider.GetEconomicGuild(Context.Guild);

            economGuild.RewardForMessage = count;

            FilesProvider.RefreshEconomicGuild(economGuild);

            await ReplyAsync($"Теперь награда за сообщение равна {economGuild.RewardForMessage}");
        }

        [Command("ИгровойАвтомат")]
        [Alias("Автомат", "Лотерея", "Сыграть", "Играть", "Казино", "Рулетка")]
        [RolesCommand]
        [Summary("позволяет выставить ставку и уйти либо в плюс, либо в минус.")]
        public async Task Monet(int count)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var economProvider = new EconomicProvider(Context.Guild);
            var economUser = economProvider.GetEconomicGuildUser(Context.User.Id);

            if (economUser.Item1.Balance >= count)
            {
                var val = LoteryEntity.CalculateWon(serGuild.LotsCount);

                string rep;
                if (val.Item2 == 0)
                {
                    rep = "Ты проиграл!";
                    economProvider.MinusBalance(Context.User, count);
                }
                else
                {
                    rep = $"Ты выиграл {count * val.Item2}!";
                    economProvider.AddBalance(Context.User, (int)(count * val.Item2));
                }

                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Игровой автомат",
                    Description = rep,
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Комбинация:",
                            Value = $"`{val.Item1}`"
                        }
                    },
                    Color = ColorProvider.GetColorForCurrentGuild(Context.Guild)
                }.Build());
            }
            else
                await ReplyAsync("Ты не можешь поставить ставку, т.к. у тебя недостаточно средств.");
        }

        [Command("РассчитатьВероятностьВыигрыша")]
        [Alias("Вероятность")]
        [RolesCommand]
        [Summary("рассчитывает вероятность выигрыша для определенного количества лотов.")]
        public async Task CalculateChances(int count)
        {
            if (count <= 10)
            {
                // Выигрыш в 50%
                float chanceOneToSix = 1F / 6F;
                double halfPartChance = Math.Pow(1, count / 2) * Math.Pow(5, count) / (Math.Pow(6, count / 2) * Math.Pow(6, count));
                // Джекпот
                double jackpotChance = Math.Pow(chanceOneToSix, count);

                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Вероятности выигрышей:",
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Выбить половину фишек:",
                            Value = halfPartChance,
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Сорвать джекпот:",
                            Value = jackpotChance,
                            IsInline = true
                        }
                    },
                    Color = ColorProvider.GetColorForCurrentGuild(Context.Guild)
                }.Build());
            }
            else
                await ReplyAsync("Количество лотов не может быть больше 10!");
        }

        [Command("ДобавитьРолиВЧерныйСписок")]
        [RolesCommand]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("добавляет роли в черный список на продажу. Когда роли находятся в черном списке на продажу, они не могут быть выставлены на продажу.")]
        public async Task AddRoleToBlackList(params IRole[] roles)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            serGuild.BlaskListedRolesToSale.AddRange(roles.Select(x => x.Id).ToList().Distinct());
            FilesProvider.RefreshGuild(serGuild);
            await ReplyAsync("Добавлено");
        }

        [Command("УбратьРолиИзЧерногоСписка")]
        [RolesCommand]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("убирает роли из черного списка.")]
        public async Task RemoveRolesFromBlackList(params IRole[] roles)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            roles.Distinct().Select(x => x.Id).ToList().ForEach(x => serGuild.BlaskListedRolesToSale.Remove(x));
            await ReplyAsync("Убрано");
        }
        #endregion        

        #region --КАСТОМИЗАЦИЯ--
        [Command("ПоменятьНикнеймБота")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [CustomisationCommand]
        [Summary("меняет никнейм бота")]
        public async Task ChangeBotNickname(params string[] NewNick)
        {
            SocketGuildUser bot = Context.Guild.GetUser(Context.Client.GetApplicationInfoAsync().Result.Id);
            string prevName = bot.Nickname;
            string name = null;
            int argPos = 0;
            var guild = Context.Guild;

            foreach (string partOfName in NewNick)
            {
                name += argPos == 0 ? partOfName : $" {partOfName}";
                argPos++;
            }

            await Context.Guild.GetUser(Context.Client.GetApplicationInfoAsync().Result.Id).ModifyAsync(x => x.Nickname = name);
            await ReplyAsync($"Никнейм бота изменен с {prevName} на {name}");
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [CustomisationCommand]
        [Command("ДобавитьРольПоумолчанию", RunMode = RunMode.Async)]
        [Alias("АвтоРоль")]
        [Summary("устанавливает роль по-умолчанию, которая будет выдаваться каждому пользователю.\nДля того чтобы установить роль нужно ее отметить.")]
        public async Task AddDefaultRole(IRole role)
        {
            if (Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position > role.Position)
            {
                SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);

                serializableGuild.DefaultRoleId = role.Id;

                FilesProvider.RefreshGuild(serializableGuild);

                await ReplyAsync("Роль успешно задана. Она будет выдаваться всем пользователям по-умолчанию.");

                foreach (var user in Context.Guild.Users)
                    await user.AddRoleAsync(role);
            }
            else
                await ReplyAsync("Я не могу выдавать роль, т.к. нахожусь ниже или на одном уровне в иерархии ролей.");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ВключитьПриветствие")]
        [Alias("Приветствие")]
        [Summary("выключает или включает приветственные сообщения. Отредактировать сообщение можно с помощью команды `РедактироватьПриветственноеСообщение`.")]
        [CustomisationCommand]
        public async Task EnableHelloMessage()
        {
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);
            serializableGuild.HelloMessageEnable = !serializableGuild.HelloMessageEnable;
            FilesProvider.RefreshGuild(serializableGuild);

            if (serializableGuild.HelloMessageEnable)
                await ReplyAsync($"Теперь приветственные сообщения будут присылаться каждому пользователю при входе на сервер. Изменить текст можно командой {serializableGuild.Prefix}РедактироватьПриветственноеСообщение");
            else
                await ReplyAsync("Теперь приветственные сообщения не будут присылаться каждому пользователю при входе на сервер.");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Alias("ПоменятьПриветствие")]
        [Command("РедактироватьПриветственноеСообщение", RunMode = RunMode.Async)]
        [Summary("редактирует приветственное сообщение.")]
        [CustomisationCommand]
        public async Task EditHelloMessage()
        {
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);
            await ReplyAsync("Введите текст сообщения");
            var message = await NextMessageAsync();

            if (message != null)
            {
                serializableGuild.HelloMessage = message.Content;
                FilesProvider.RefreshGuild(serializableGuild);
                await ReplyAsync("Приветственное сообщение отредактировано. Вот как оно выглядит:");
                await ReplyAsync(serializableGuild.HelloMessage);
            }
            else
                await ReplyAsync("Ответ не получен в течении 5 минут. Команда аннулированна.");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ПоменятьПрефикс")]
        [Alias("Префикс")]
        [Summary("редактирует префикс (У тебя должно быть право на выполнение этой команды).")]
        [CustomisationCommand]
        public async Task EditPrefix(string newPrefix)
        {
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);
            FilesProvider.RefreshGuild(serializableGuild);

            serializableGuild.Prefix = newPrefix;

            FilesProvider.RefreshGuild(serializableGuild);
            await ReplyAsync("Префикс сменен");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("РежимКомнат")]
        [Alias("Комнаты")]
        [Summary("создает канал с возможностью создания комнат. Возможно удаление комнат.")]
        [CustomisationCommand]
        public async Task EnableRooms()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var category = await Context.Guild.CreateCategoryChannelAsync("🏠Комнаты");
            var channel = await Context.Guild.CreateVoiceChannelAsync("➕Создать комнату", x => x.CategoryId = category.Id);

            serGuild.SystemCategories.VoiceRoomsCategoryId = category.Id;
            serGuild.SystemChannels.CreateRoomChannelId = channel.Id;

            FilesProvider.RefreshGuild(serGuild);
            await ReplyAsync("Режим комнат установлен");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("КаналыКонтента")]
        [Alias("Контент")]
        [Summary("создает каналы контента. Возможно удаление каналов.")]
        [CustomisationCommand]
        public async Task EnableContent()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var category = await Context.Guild.CreateCategoryChannelAsync("⚡Контент");
            var links = await Context.Guild.CreateTextChannelAsync("🌐ссылки", x => x.CategoryId = category.Id);
            var videos = await Context.Guild.CreateTextChannelAsync("📹видео", x => x.CategoryId = category.Id);

            serGuild.SystemCategories.ContentCategoryId = category.Id;
            serGuild.SystemChannels.LinksChannelId = links.Id;
            serGuild.SystemChannels.VideosChannelId = videos.Id;

            FilesProvider.RefreshGuild(serGuild);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ПроверкаКонтента")]
        [Summary("включает/выключает проверку контента (сортировку видео, ссылок по нужным каналам). Работает только при включенных каналах контента.")]
        [CustomisationCommand]
        public async Task EnableCheckingContent()
        {
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);

            serializableGuild.CheckingContent = !serializableGuild.CheckingContent;
            FilesProvider.RefreshGuild(serializableGuild);

            if (serializableGuild.CheckingContent)
                await ReplyAsync("Проверка контента включена.");
            else
                await ReplyAsync("Проверка контента отключена.");
        }

        [Command("УстановитьНежелательныеСлова")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("устанавливает список нежелательных слов")]
        public async Task SetBadWords(params string[] wordsInMess)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            serGuild.BadWords.Clear();
            await AddExceptOrBadWords(ExceptOrBad.Bad, serGuild, wordsInMess);
        }

        [Command("ОчиститьСписокНежелательныхСлов")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("очищает список нежелательных слов")]
        public async Task ClearBadWords()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            int count = serGuild.BadWords.Count;

            serGuild.BadWords.Clear();
            FilesProvider.RefreshGuild(serGuild);

            char lastCharOfNum = count.ToString().Last();
            await ReplyAsync($"Очищено {(lastCharOfNum == '1' ? "слово" : lastCharOfNum == '2' || lastCharOfNum == '3' || lastCharOfNum == '4' ? "слова" : "слов")} слов.");
        }

        [Command("ДобавитьНежелательныеСлова")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("добавляет список нежелательных слов")]
        public async Task AddBadWords(params string[] wordsInMess)
        {
            await AddExceptOrBadWords(ExceptOrBad.Bad, FilesProvider.GetGuild(Context.Guild), wordsInMess);
        }

        [Command("УдалитьНежелательноеСлова")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("удаляет нежелательные слова из списка.")]
        public async Task DeleteWord(params string[] words)
        {
            if (words.Length > 0)
            {
                SerializableGuild guild = FilesProvider.GetGuild(Context.Guild);

                foreach (string word in words)
                    if (guild.BadWords.Contains(word.ToLower()))
                        guild.BadWords.Remove(word.ToLower());

                FilesProvider.RefreshGuild(guild);
                await ReplyAsync("Удаление слов произведено успешно.");
            }
            else
                await ReplyAsync("Не могу найти слова в сообщении.");
        }

        [Command("ДобавитьСловаИсключения")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("добавляет слова, которые не будут считаться плохими. Их стоит добавлять только в случае, если есть плохое и какое-нибудь другое слово, которые очень похожи.")]
        public async Task AddExcept(params string[] wordsInMess)
        {
            await AddExceptOrBadWords(ExceptOrBad.Except, FilesProvider.GetGuild(Context.Guild), wordsInMess);
        }

        [Command("УдалитьСловаИсключения")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("удаляет слова-исключения из списка.")]
        public async Task DeleteExceptWord(params string[] words)
        {
            if (words.Length > 0)
            {
                SerializableGuild guild = FilesProvider.GetGuild(Context.Guild);

                foreach (string word in words)
                    if (guild.ExceptWords.Contains(word.ToLower()))
                        guild.ExceptWords.Remove(word.ToLower());

                FilesProvider.RefreshGuild(guild);
                await ReplyAsync("Удаление слов произведено успешно.");
            }
            else
                await ReplyAsync("Не могу найти слова в сообщении.");
        }

        [Command("УстановитьСловаИсключения")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("устанавливает список слов-исключений")]
        public async Task SetExceptWords(params string[] wordsInMess)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            serGuild.BadWords.Clear();
            await AddExceptOrBadWords(ExceptOrBad.Bad, serGuild, wordsInMess);
        }

        [Command("ОчиститьСписокСловИсключений")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("очищает список слов-исключений")]
        public async Task ClearExceptWords()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            int count = serGuild.ExceptWords.Count;

            serGuild.ExceptWords.Clear();
            FilesProvider.RefreshGuild(serGuild);

            await ReplyAsync($"Очищено {count} слов.");
        }

        [Command("ПроверкаСлов")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("включает/выключает проверку нежелательных слов.")]
        public async Task CheckingBadWords()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            serGuild.CheckingBadWords = !serGuild.CheckingBadWords;

            if (serGuild.CheckingBadWords)
                await ReplyAsync("Теперь я буду проверять нежелательные слова");
            else
                await ReplyAsync("Теперь не я буду проверять нежелательные слова");

            FilesProvider.RefreshGuild(serGuild);
        }

        [Command("ПредупрежденияЗаСлова")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("включает/выключает предупреждения за использование нежелательных слов")]
        public async Task WarnsBadWords()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            serGuild.WarnsForBadWords = !serGuild.WarnsForBadWords;

            if (serGuild.WarnsForBadWords)
                await ReplyAsync("Теперь я буду начислять предупреждения за нежелательные слова");
            else
                await ReplyAsync("Теперь я не буду начислять предупреждения за нежелательные слова");

            FilesProvider.RefreshGuild(serGuild);
        }

        [Command("ПредупрежденияЗаПриглашения")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("включает/выключает предупреждения за использование ссылок-приглашений")]
        public async Task WarnsForInvites()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            serGuild.WarnsForInviteLink = !serGuild.WarnsForInviteLink;

            if (serGuild.WarnsForInviteLink)
                await ReplyAsync("Теперь я буду начислять предупреждения за ссылки-приглашения");
            else
                await ReplyAsync("Теперь я не буду начислять предупреждения за ссылки-приглашения");

            FilesProvider.RefreshGuild(serGuild);
        }

        [Command("НаказаниеЗаНарушения")]
        [CustomisationCommand]
        [Summary("устанавливает наказание за превышение количества предупреждений.")]
        public async Task AddPunishmentForBadWords(string punishment)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            switch (punishment.ToLower())
            {
                case "кик":
                    serGuild.KickForWarns = true;
                    serGuild.BanForWarns = false;
                    serGuild.MuteForWarns = false;
                    break;
                case "бан":
                    serGuild.KickForWarns = false;
                    serGuild.BanForWarns = true;
                    serGuild.MuteForWarns = false;
                    break;
                case "мут":
                    serGuild.KickForWarns = false;
                    serGuild.BanForWarns = false;
                    serGuild.MuteForWarns = true;
                    break;
                case "нет":
                    serGuild.KickForWarns = false;
                    serGuild.BanForWarns = false;
                    serGuild.MuteForWarns = false;
                    break;
                default:
                    await ReplyAsync("Ты неверно указал наказание. Вот тебе 3 типа наказаний:\n1. `Кик`\n2. `Бан`\n3. `Мут`\nЕсли ты хочешь отменить наказания, тогда напиши `Нет` в качестве аргумента.");
                    return;
            }

            FilesProvider.RefreshGuild(serGuild);

            await ReplyAsync("Наказание успешно установлено.");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("Уведомления")]
        [Alias("Логирование", "Лог")]
        [Summary("включает/выключает уведомления сервера. При бане, кике, добавлении на сервер пользователя бот тебя уведомит")]
        [CustomisationCommand]
        public async Task EnableGuildNotifications(SocketTextChannel logChannel)
        {
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);

            serializableGuild.SystemChannels.LogsChannelId = logChannel.Id;
            await ReplyAsync($"Теперь я буду присылать логи в {logChannel.Mention}");
            await logChannel.SendMessageAsync(embed: new EmbedBuilder
            {
                Title = "В данный канал теперь будут присылаться логи",
                Color = ColorProvider.GetColorForCurrentGuild(Context.Guild),
                Description = "Если вы хотите отключить логирование, тогда удалите этот канал или сбросьте настройки бота."
            }.Build());

            FilesProvider.RefreshGuild(serializableGuild);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ЭмодзиКомнаты")]
        [CustomisationCommand]
        [Summary("устанавливает значок комнат.")]
        public async Task SetRoomsEmoji(string emoji)
        {
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);

            serializableGuild.EmojiOfRoom = emoji;
            FilesProvider.RefreshGuild(serializableGuild);

            await ReplyAsync("Значок комнат изменен успешно");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("КаналДляПриветствий")]
        [CustomisationCommand]
        [Summary("устанавливает канал для приветствий.")]
        public async Task HelloChannel(SocketTextChannel channel)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            if (Context.Message.MentionedChannels.Count > 0)
            {
                serGuild.SystemChannels.NewUsersChannelId = channel.Id;
                FilesProvider.RefreshGuild(serGuild);
                await ReplyAsync("Канал для приветствий установлен");
            }
            else
                await ReplyAsync("Не могу найти канал.");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("КаналДляПрощаний")]
        [CustomisationCommand]
        [Summary("устанавливает канал для прощаний.")]
        public async Task GoodbyeChannel(SocketTextChannel channel)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            if (Context.Message.MentionedChannels.Count > 0)
            {
                serGuild.SystemChannels.LeaveUsersChannelId = channel.Id;
                FilesProvider.RefreshGuild(serGuild);
                await ReplyAsync("Канал для прощаний установлен");
            }
            else
                await ReplyAsync("Не могу найти канал.");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("УведомлениеОНеправильнойКоманде")]
        [CustomisationCommand]
        [Alias("НеправильнаяКоманда", "Ошибки")]
        [Summary("включает/выключает уведомление о неправильной команде.")]
        public async Task CommandErrorMessage()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            serGuild.UnknownCommandMessage = !serGuild.UnknownCommandMessage;

            if (serGuild.UnknownCommandMessage)
                await ReplyAsync("Теперь я буду присылать уведомление о неправильной команде.");
            else
                await ReplyAsync("Теперь я не буду присылать уведомление о неправильной команде.");

            FilesProvider.RefreshGuild(serGuild);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ЦветЭмбеда")]
        [CustomisationCommand]
        [Summary("устанавливает цвет эмбеда")]
        public async Task ChangeEmbedColor(string color)
        {
            ColorProvider.SerializeColor(color, Context.Guild);

            await ReplyAsync("Цвет эмбеда сменен");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ЦветЭмбеда")]
        [CustomisationCommand]
        [Summary("устанавливает цвет эмбеда")]
        public async Task ChangeEmbedColor(int r, int g, int b)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            serGuild.ColorOfEmbed = new Color(r, g, b).ToString();

            FilesProvider.RefreshGuild(serGuild);

            await ReplyAsync("Цвет эмбеда сменен");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ОтвечатьТолькоВ")]
        [CustomisationCommand]
        [Summary("устанавливает каналы, в которых бот будет отвечать")]
        public async Task ChannelsToReply(params SocketTextChannel[] textChannels)
        {
            List<SocketTextChannel> sortedTextChannels = new List<SocketTextChannel>();
            List<ulong> textChannelsIds = new List<ulong>();
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            foreach (var channel in textChannels)
            {
                if (!textChannelsIds.Contains(channel.Id))
                    textChannelsIds.Add(channel.Id);
                if (!sortedTextChannels.Contains(channel))
                    sortedTextChannels.Add(channel);
            }

            serGuild.CommandsChannels = textChannelsIds;
            string mentions = null;

            for (int i = 0; i < sortedTextChannels.Count; i++)
                mentions += i + 1 == sortedTextChannels.Count ? $" {sortedTextChannels[i].Mention}." : $" {sortedTextChannels[i].Mention},";

            FilesProvider.RefreshGuild(serGuild);
            await ReplyAsync($"Теперь я буду отвечать только в следующих каналах:{mentions}");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("НеОтвечатьТолькоВ")]
        [CustomisationCommand]
        [Summary("устанавливает каналы, в которых бот не будет отвечать")]
        public async Task ChannelsToNoReply(params SocketTextChannel[] textChannels)
        {
            List<SocketTextChannel> sortedTextChannels = new List<SocketTextChannel>();
            List<ulong> textChannelsIds = new List<ulong>();
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            foreach (var channel in Context.Guild.TextChannels)
                if (!textChannels.Contains(channel))
                    textChannelsIds.Add(channel.Id);

            foreach (var channel in textChannels)
                if (!sortedTextChannels.Contains(channel))
                    sortedTextChannels.Add(channel);

            serGuild.CommandsChannels = textChannelsIds;
            string mentions = null;

            for (int i = 0; i < sortedTextChannels.Count; i++)
                mentions += i + 1 == sortedTextChannels.Count ? $" {sortedTextChannels[i].Mention}." : $" {sortedTextChannels[i].Mention},";

            if (textChannelsIds.Count != Context.Guild.TextChannels.Count)
            {
                FilesProvider.RefreshGuild(serGuild);

                await ReplyAsync($"Теперь я не буду отвечать только в следующих каналах:{mentions}");
            }
            else
                await ReplyAsync("Ты не можешь отключить все каналы");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ОтвечатьВезде")]
        [CustomisationCommand]
        [Summary("разрешает боту отвечать во всех каналах.")]
        public async Task AllChannelsToReply()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            serGuild.CommandsChannels.Clear();

            FilesProvider.RefreshGuild(serGuild);

            await ReplyAsync($"Теперь я буду отвечать во всех каналах.");
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("ИгнорироватьРоли")]
        [CustomisationCommand]
        [Summary("устанавливает роли, которым бот не будет отвечать")]
        public async Task IgnoreRoles(params IRole[] roles)
        {
            List<ulong> filteredIgnoreRoles = new List<ulong>();
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            string rolesMent = null;

            for (int i = 0; i < roles.Length; i++)
                if (!filteredIgnoreRoles.Contains(roles[i].Id))
                {
                    filteredIgnoreRoles.Add(roles[i].Id);
                    rolesMent += i == roles.Length - 1 ? $" {roles[i].Mention}." : $" {roles[i].Mention},";
                }

            serGuild.IgnoreRoles = filteredIgnoreRoles;
            FilesProvider.RefreshGuild(serGuild);
            await ReplyAsync($"Теперь я буду игнорировать следующие роли: {rolesMent}");
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("ОтвечатьВсем")]
        [CustomisationCommand]
        [Summary("разрешает боту отвечать всем ролям на сервере")]
        public async Task FullNoIgnoreRoles()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            serGuild.IgnoreRoles.Clear();

            FilesProvider.RefreshGuild(serGuild);
            await ReplyAsync("Теперь я буду отвечать всем пользователям на это сервере");
        }

        #endregion

        #region --КОНСОЛЬНЫЕ КОМАНДЫ--
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("СинхронизироватьРолиС")]
        [ConsoleCommands]
        [Summary("синхронизирует разрешения ролей относительно одной")]
        public async Task SyncRoles(IRole syncRole, params IRole[] toSyncRoles)
        {
            toSyncRoles = toSyncRoles.Distinct().ToArray().Where(x => x.Position < Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.OrderBy(a => a.Position).Last().Position).ToArray();

            foreach (var role in toSyncRoles)
                await role.ModifyAsync(x => x.Permissions = syncRole.Permissions);

            char lastCharOfNum = toSyncRoles.Length.ToString().Last();

            await ReplyAsync($"{(lastCharOfNum == '1' ? "Синхронизирована" : "Синхронизировано")} {toSyncRoles.Length} " +
                $"{(lastCharOfNum == '1' ? "роль" : lastCharOfNum == '2' || lastCharOfNum == '3' || lastCharOfNum == '4' ? "роли" : "ролей")}");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("СоздатьКатегорию")]
        [ConsoleCommands]
        [Summary("создает категорию.")]
        public async Task CreateCategory(params string[] name)
        {
            string fullName = null;
            name.ToList().ForEach(x => fullName += $" {x}");
            fullName.Remove(0);
            await Context.Guild.CreateCategoryChannelAsync(fullName);
            await ReplyAsync("Создал");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("СоздатьТекстовыйКанал")]
        [ConsoleCommands]
        [Summary("создает текстовый канал.")]
        public async Task CreateTextChannel(params string[] name)
        {
            string fullName = null;
            name.ToList().ForEach(x => fullName += $" {x}");
            fullName.Remove(0);
            await Context.Guild.CreateTextChannelAsync(fullName);
            await ReplyAsync("Создал");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("СоздатьГолосовойКанал")]
        [ConsoleCommands]
        [Summary("создает голосовой канал.")]
        public async Task CreateVoiceChannel(params string[] name)
        {
            string fullName = null;
            name.ToList().ForEach(x => fullName += $" {x}");
            fullName.Remove(0);
            await Context.Guild.CreateVoiceChannelAsync(fullName);
            await ReplyAsync("Создал");
        }
        #endregion

        private async void ClearMessages(object count)
        {
            try
            {
                var thrStMess = await ReplyAsync("Начинаю удаление сообщений...");

                var messages = await Context.Channel.GetMessagesAsync((int)count + 2).FlattenAsync();
                if (messages.Any(x => DateTimeOffset.Now.ToUniversalTime().Subtract(x.Timestamp) > TimeSpan.FromDays(14)))
                {
                    var mess = await ReplyAsync("Я не могу удалить сообщения двухнедельной давности");
                    Thread.Sleep(1000);
                    await mess.DeleteAsync();
                    await thrStMess.DeleteAsync();
                }
                else
                {
                    await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
                    var delMess = await ReplyAsync("Удаление сообщений произведено успешно");
                    Thread.Sleep(1000);
                    await delMess.DeleteAsync();
                }
                Thread.Sleep(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
            }
        }

        private enum ExceptOrBad { Except, Bad }
        private async Task AddExceptOrBadWords(ExceptOrBad type, SerializableGuild guild, params string[] wordsInMess)
        {
            bool added = false;
            int count = 0;
            if (wordsInMess.Length == 0)
            {
                if (Context.Message.Attachments.Count > 0)
                {
                    var attachment = Context.Message.Attachments.First();
                    if (Path.GetExtension(attachment.Filename) == ".txt")
                    {
                        WebClient client = new();
                        Stream stream = client.OpenRead(attachment.Url);

                        using StreamReader reader = new(stream);
                        string text = await reader.ReadToEndAsync();

                        List<string> filteredWords = text.Trim(' ', '/', '\\', '=', '-', '+', '_', '(', ')', '*', '&', '?', '^', ':', '%', '$', ';', '@', '"', '.')
                            .Split("\n")
                            .ToList()
                            .Select(x => x.ToLower())
                            .Distinct()
                            .Where(x => !guild.BadWords.Contains(x.ToLower()))
                            .ToList();

                        if (type == ExceptOrBad.Bad)
                            guild.BadWords.AddRange(filteredWords);
                        else
                            guild.ExceptWords.AddRange(filteredWords);

                        FilesProvider.RefreshGuild(guild);

                        count = filteredWords.Count;
                        added = true;
                    }
                    else
                        await ReplyAsync("Некорректное расширение файла. Файл должен быть текстовый (`.txt`)");
                }
                else
                    await ReplyAsync("Я не могу найти файл в сообщении.");
            }
            else
            {
                List<string> filteredWords = wordsInMess
                    .ToList()
                    .Select(x => x.ToLower())
                    .Distinct()
                    .Where(x => !guild.BadWords.Contains(x.ToLower()))
                    .ToList();

                if (type == ExceptOrBad.Bad)
                    guild.BadWords.AddRange(filteredWords);
                else
                    guild.ExceptWords.AddRange(filteredWords);

                FilesProvider.RefreshGuild(guild);

                count = filteredWords.Count;
                added = true;
            }

            if (added)
            {
                char lastCharOfNum = guild.BadWords.Count.ToString().Last();
                char lastCharOfCount = count.ToString().Last();
                await ReplyAsync($"Добавлено {count} {(lastCharOfCount == '1' ? "слово" : lastCharOfCount == '2' || lastCharOfCount == '3' || lastCharOfCount == '4' ? "слова" : "слов")}. На данный момент в словаре есть {(type == ExceptOrBad.Bad ? guild.BadWords.Count : guild.ExceptWords.Count)} {(lastCharOfNum == '1' ? "слово" : lastCharOfNum == '2' || lastCharOfNum == '3' || lastCharOfNum == '4' ? "слова" : "слов")}.");
            }
        }
    }
}