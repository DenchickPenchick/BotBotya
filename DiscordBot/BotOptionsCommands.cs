using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Addons.Interactive;
using DiscordBot.Modules.FileManaging;
using DiscordBot.FileWorking;
using DiscordBot.GuildManaging;
using Discord.WebSocket;

namespace DiscordBot
{
    public class BotOptionsCommands : InteractiveBase
    {        
        [Command("КонфигурацияСервера")]
        [Summary("получает конфигурацию сервера.")]
        public async Task GetGuildConfig()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = $"Конфигурация сервера {Context.Guild.Name}",
                Description = $"Приветственные сообщения: {(serGuild.HelloMessageEnable == true ? "Включены" : "Выключены")}\n" +
                $"Текст приветственного сообщения:\n{serGuild.HelloMessage}\n" +
                $"Режим приватных комнат: {(serGuild.RoomsEnable == true ? "Включен" : "Выключен")}\n" +
                $"Каналы контента: {(serGuild.ContentEnable == true ? "Включены" : "Выключены")}\n" +
                $"Проверка контента: {(serGuild.CheckingContent == true ? "Включена" : "Выключена")}\n" +
                $"Уведомления сервера: {(serGuild.GuildNotifications == true ? "Включены" : "Выключены")}",                
                Color = Color.Blue,
                ImageUrl = Context.Guild.IconUrl
            }.Build());
        }

        [Command("РасставитьСистемныеКатегории")]
        [Summary("сортирует \"системные\" категории.")]
        public async Task SortSystemCategories()
        {
            await new GuildProvider(Context.Guild).SortSystemCategories();
            await ReplyAsync("Сортировка завершена");
        }

        [Command("ПоменятьНикнеймБота")]
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

        [Command("ПоменятьНазваниеТекстовогоКанала", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("меняет название текстового канала.\nВНИМАНИЕ! Для категорий и голосовых каналов данная команда не подойдет.")]
        public async Task ChangeNameOfTextOrCategoryChannel()
        {
            var guild = FilesProvider.GetGuild(Context.Guild);
            await ReplyAsync("Упомяни канал, которому хочешь поменять название.");
            var replyChannel = await NextMessageAsync();
            if (replyChannel != null)
            {
                await ReplyAsync($"Введи название, которое ты хочешь дать каналу {replyChannel.MentionedChannels.First().Name}");
                var newNameReply = await NextMessageAsync();
                if (newNameReply != null)
                {
                    GuildProvider provider = new GuildProvider(Context.Guild);                    
                    var channel = replyChannel.MentionedChannels.First();
                    if (provider.GetSystemChannels().Contains(replyChannel.MentionedChannels.First()))
                    {
                        if (channel.Name == guild.SystemChannels.ConsoleChannelName)
                        {
                            guild.SystemChannels.ConsoleChannelName = newNameReply.Content.ToLower().Replace(' ', '-');
                        }
                        else if (channel.Name == guild.SystemChannels.LinksChannelName)
                        {
                            guild.SystemChannels.LinksChannelName = newNameReply.Content.ToLower().Replace(' ', '-');
                        }
                        else if (channel.Name == guild.SystemChannels.VideosChannelName)
                        {
                            guild.SystemChannels.VideosChannelName = newNameReply.Content.ToLower().Replace(' ', '-');
                        }
                    }
                    FilesProvider.RefreshGuild(guild);
                    await replyChannel.MentionedChannels.First().ModifyAsync(x => x.Name = newNameReply.Content);
                    await ReplyAsync($"Имя канала изменено с {replyChannel.MentionedChannels.First().Name} на {newNameReply.Content}");                    
                }
                else
                {
                    await ReplyAsync("Ответ не получен в течении 5 минут. Команда аннулированна.");
                    return;
                }
            }
            else
            {
                await ReplyAsync("Ответ не получен в течении 5 минут. Команда аннулированна.");
                return;
            }               
        }

        [Command("ПоменятьНазваниеСистемнойКатегории", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("меняет название \"системной\" категории.\nДля того чтобы выбратькатегорию, нужно правильно вписать тип категории (основные текстовые каналы: txt, контент: content, основные голосовые каналы: mainvoice, категория с комнатами: roomscat, категория бота: botcat)\nВНИМАНИЕ! Для текстовых каналов и голосовых каналов данная команда не подойдет (будет выдана ошибка).")]
        public async Task ChangeNameOfSystemCategoryChannel(string Type, params string[] NewName)
        {
            GuildProvider provider = new GuildProvider(Context.Guild);
            var guild = FilesProvider.GetGuild(Context.Guild);
            int argPos = 0;
            string name = null;
            SocketCategoryChannel category = null;            

            foreach (string partOfName in NewName)
            {
                name += argPos == 0 ? partOfName : $" {partOfName}";
                argPos++;
            }

            switch (Type.ToLower())
            {
                case "txt":                    
                    category = provider.MainTextChannelsCategory();                    
                    guild.SystemCategories.MainTextCategoryName = name;                    
                    break;
                case "content":
                    category = provider.ContentCategoryChannel();
                    guild.SystemCategories.ContentCategoryName = name;                    
                    break;
                case "mainvoice":
                    category = provider.MainVoiceChannelsCategory();
                    guild.SystemCategories.MainVoiceCategoryName = name;
                    break;
                case "roomscat":
                    category = provider.RoomsCategoryChannel();
                    guild.SystemCategories.VoiceRoomsCategoryName = name;                    
                    break;
                case "botcat":
                    category = provider.BotChannelsCategory();
                    guild.SystemCategories.BotCategoryName = name;
                    break;
                default:
                    await ReplyAsync("Неверный тип категории. Команда аннулированна");
                    return;
            }

            if (category != null)
            {
                string oldName = category.Name;
                await category.ModifyAsync(x => x.Name = name);
                await ReplyAsync($"Имя категории {oldName} изменено на {name}");
            }            
            
            FilesProvider.RefreshGuild(guild);            
        }

        [Command("ПоменятьИмяКаналаКомнат", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Summary("меняет название голосового канала, который создает новые комнаты.\nВНИМАНИЕ! Для текстовых каналов и категорий, и не \"ситемных\" голосовых каналов данная команда не подойдет.")]
        public async Task ChangeNameOfCreateRoomChannel(params string[] NewName)
        {
            var guild = FilesProvider.GetGuild(Context.Guild);
            int argPos = 0;
            string name = null;

            foreach (string partOfName in NewName)
            {
                name += argPos == 0 ? partOfName : $" {partOfName}";
                argPos++;
            }

            string oldName = new GuildProvider(Context.Guild).CreateRoomChannel().Name;            
            await new GuildProvider(Context.Guild).CreateRoomChannel().ModifyAsync(x => x.Name = name);

            guild.SystemChannels.CreateRoomChannelName = name;
            FilesProvider.RefreshGuild(guild);

            await ReplyAsync($"Имя канала {oldName} изменено на {name}");
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("ДобавитьРольПоумолчанию", RunMode = RunMode.Async)]
        [Summary("устанавливает роль по-умолчанию, которая будет выдаваться каждому пользователю (У тебя должно быть право на выполнение этой команды).\nДля того чтобы установить роль нужно ее отметить. Если отметить несколько ролей, то установлена будет только первая.")]
        public async Task AddDefaultRole(params string[] str)
        {
            if (Context.Message.MentionedRoles.Count > 0)
            {
                var role = Context.Message.MentionedRoles.ToArray()[0];                                
                SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);                

                serializableGuild.DefaultRoleId = role.Id;

                FilesProvider.RefreshGuild(serializableGuild);

                await ReplyAsync("Роль успешно задана. Она будет выдаваться всем пользователям по-умолчанию. Цвет и другие параметры ты можешь натроить сам.");

                foreach (var user in Context.Guild.Users)
                    await user.AddRoleAsync(role);
            }
            else
            {
                await ReplyAsync("Роль не найдена в сообщении. Ты хочешь ее создать? Пиши +(да) или -(нет).");
                var createRoleMessage = await NextMessageAsync();
                if (createRoleMessage != null)
                    switch (createRoleMessage.Content)
                    {
                        case "+":
                            await ReplyAsync("Введи название роли(обязательно)");
                            var roleMessage = await NextMessageAsync();
                            if (roleMessage != null)
                            {
                                var role = await Context.Guild.CreateRoleAsync(roleMessage.Content,
                                    new GuildPermissions().Modify(
                                        createInstantInvite: true,
                                        readMessageHistory: true,
                                        connect: true,
                                        sendMessages: true,
                                        sendTTSMessages: true,
                                        embedLinks: true,
                                        attachFiles: true,
                                        mentionEveryone: true,
                                        useExternalEmojis: true,
                                        speak: true,
                                        useVoiceActivation: true),
                                    Color.Default, false, null);
                                                               
                                SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);                                

                                serializableGuild.DefaultRoleId = role.Id;

                                FilesProvider.RefreshGuild(serializableGuild);

                                await ReplyAsync("Роль успешно задана. Она будет выдаваться всем пользователям по-умолчанию. Цвет и другие параметры ты можешь натроить сам.");

                                foreach (var user in Context.Guild.Users)
                                    await user.AddRoleAsync(role);
                            }
                            else
                                await ReplyAsync("Ответ не получен в течении 5 минут. Команда аннулированна.");
                            break;
                        case "-":
                            await ReplyAsync("Невозможно установить роль по-умолчанию. Команда аннулированна.");
                            return;
                    }
                else
                {
                    await ReplyAsync("Ответ не получен в течении 5 минут. Команда аннулированна.");
                    return;
                }

            }
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ВключитьПриветствие")]
        [Summary("выключает или включает приветственные сообщения. Отредактировать сообщение можно с помощью команды EditHelloMessage (У тебя должно быть право на выполнение этой команды).")]
        public async Task EnableHelloMessage()
        {                               
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);
            serializableGuild.HelloMessageEnable = !serializableGuild.HelloMessageEnable;
            FilesProvider.RefreshGuild(serializableGuild);

            if (serializableGuild.HelloMessageEnable)
                await ReplyAsync("Теперь приветственные сообщения будут присылаться каждому пользователю при входе на сервер. Изменить текст можно командой !EditHelloMessage");
            else
                await ReplyAsync("Теперь приветственные сообщения не будут присылаться каждому пользователю при входе на сервер.");          
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("РедактироватьПриветственноеСообщение", RunMode = RunMode.Async)]
        [Summary("редактирует приветственное сообщение (У тебя должно быть право на выполнение этой команды).")]
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
        [Summary("редактирует префикс (У тебя должно быть право на выполнение этой команды).")]
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
        [Summary("включает/выключает режим приватных комнат (У тебя должно быть право на выполнение этой команды).")]
        public async Task EnableRooms()
        {
            GuildProvider provider = new GuildProvider(Context.Guild);            
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);

            serializableGuild.RoomsEnable = !serializableGuild.RoomsEnable;

            FilesProvider.RefreshGuild(serializableGuild);

            if (serializableGuild.RoomsEnable)
                await ReplyAsync("Включен режим приватных комнат.");
            else
                await ReplyAsync("Выключен режим приватных комнат.");

            await provider.SyncGuildProperties();            
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("КаналыКонтента")]
        [Summary("включает/выключает каналы контента. У тебя должно быть право на выполнение этой команды.")]
        public async Task EnableContent()
        {
            GuildProvider provider = new GuildProvider(Context.Guild);            
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);

            serializableGuild.ContentEnable = !serializableGuild.ContentEnable;
            FilesProvider.RefreshGuild(serializableGuild);

            if (serializableGuild.ContentEnable)            
                await ReplyAsync("Каналы контента включены.");                                             
            else            
                await ReplyAsync("Каналы контента выключены.");                
            
            await provider.SyncGuildProperties();
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ПроверкаКонтента")]
        [Summary("включает/выключает проверку контента (сортировку видео, ссылок по нужным каналам). Работает только при включенных каналах контента. У тебя должно быть право на выполнение этой команды.")]
        public async Task EnableCheckingContent()
        {
            GuildProvider provider = new GuildProvider(Context.Guild);            
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);

            serializableGuild.CheckingContent = !serializableGuild.CheckingContent;
            FilesProvider.RefreshGuild(serializableGuild);

            if (serializableGuild.CheckingContent)
                await ReplyAsync("Проверка контента включена.");
            else
                await ReplyAsync("Проверка контента отключена.");

            await provider.SyncGuildProperties();
        }       

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("Уведомления", RunMode = RunMode.Async)]
        [Summary("включает/выключает уведомления сервера. При бане, кике, добавлении на сервер пользователя бот тебя уведомит")]
        public async Task EnableGuildNotifications()
        {
            GuildProvider provider = new GuildProvider(Context.Guild);
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);

            serializableGuild.GuildNotifications = !serializableGuild.GuildNotifications;
            
            if (serializableGuild.GuildNotifications)
            {
                await ReplyAsync("Упомяни канал куда нужно присылать логи.");
                var respondChannelMessage = await NextMessageAsync();
                if (respondChannelMessage != null)
                {
                    if (respondChannelMessage.MentionedChannels != null)
                        serializableGuild.LoggerId = respondChannelMessage.MentionedChannels.ToArray()[0].Id;
                    else
                    {
                        await ReplyAsync("Ни одного канала не найдено.");
                        return;
                    }
                }
                else
                {
                    await ReplyAsync("Ты не ответил в течении 5 минут. Команда аннулированна.");
                    return;
                }
                await ReplyAsync("Уведомления включены.");
            }
            else             
                await ReplyAsync("Уведомления выключены.");            

            FilesProvider.RefreshGuild(serializableGuild);
            await provider.SyncGuildProperties();
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ЭмодзиКомнаты")]
        [Summary("устанавливает значок комнат.")]
        public async Task SetRoomsEmoji(string emoji)
        {
            GuildProvider provider = new GuildProvider(Context.Guild);
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);

            serializableGuild.EmojiOfRoom = emoji;
            FilesProvider.RefreshGuild(serializableGuild);

            await ReplyAsync("Значок комнат изменен успешно");

            await provider.SyncGuildProperties();
        }
    }
}
