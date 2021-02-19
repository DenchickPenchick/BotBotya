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

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Console = Colorful.Console;
using DiscordBot.MusicOperations;
using DiscordBot.Providers;
using System.Collections.Generic;
using DiscordBot.CustomCommands;
using System.IO;
using System.Xml.Serialization;
using Victoria;
using DiscordBot.Compiling;
using System.Net;
using System.Xml;
using DiscordBot;
using DiscordBot.Attributes;
using DiscordBot.Serializable;
using DiscordBot.Serializable.SerializableActions;

namespace TestBot
{
    public class Commands : InteractiveBase
    {
        private readonly Bot Bot;

        private readonly LavaOperations LavaOperations;

        public Commands(LavaOperations lavaOperations, Bot bot)
        {
            LavaOperations = lavaOperations;
            Bot = bot;
        }

        #region --СТАНДАРТНЫЕ КОМАНДЫ--
        [Command("Хелп", RunMode = RunMode.Async)]
        [Alias("Хэлп", "Помощь")]
        [StandartCommand]
        [Summary("позволяет узнать полный список команд")]
        public async Task Help(int page = 0)
        {
            int pos = 0;
            int posit = 1;
            int catpos = 0;
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            List<string> pages = new List<string>
            {
                null
            };

            CommandCategoryAttribute prevCategoryAttribute = new StandartCommandAttribute();

            foreach (var command in Bot.Commands.Commands)
            {
                CommandCategoryAttribute categoryAttribute;

                string aliases = "\nПсевдонимы:";
                string parameters = "\nПараметры";

                foreach (string alias in command.Aliases)
                    if (alias != command.Name)
                        aliases += $" `{alias}`";

                foreach (var param in command.Parameters)
                    parameters += $" `{param.Name}`";

                if (command.Attributes.Contains(new StandartCommandAttribute()))
                    categoryAttribute = new StandartCommandAttribute();
                else if (command.Attributes.Contains(new CustomisationCommandAttribute()))
                    categoryAttribute = new CustomisationCommandAttribute();
                else if (command.Attributes.Contains(new CustomCommandAttribute()))
                    categoryAttribute = new CustomCommandAttribute();
                else if (command.Attributes.Contains(new MusicCommandAttribute()))
                    categoryAttribute = new MusicCommandAttribute();
                else if (command.Attributes.Contains(new RolesCommandAttribute()))
                    categoryAttribute = new RolesCommandAttribute();
                else
                    categoryAttribute = new StandartCommandAttribute();

                if (prevCategoryAttribute.CategoryName != categoryAttribute.CategoryName || $"{pages[pos]}\n{posit + 1}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}{(command.Parameters.Count > 0 ? parameters : null)}{(command.Aliases.Count > 0 ? aliases : null)}".Length >= 1024)
                {
                    if (prevCategoryAttribute.CategoryName != categoryAttribute.CategoryName)
                        posit = 1;

                    pages.Add($"\n**{categoryAttribute.CategoryName}**\n{posit++}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}{(command.Parameters.Count > 0 ? parameters : null)}{(command.Aliases.Count > 0 ? aliases : null)}");
                    pos++;
                }
                else if (catpos == 0)
                {
                    pages[0] += $"\n**{categoryAttribute.CategoryName}**\n{posit++}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}{(command.Parameters.Count > 0 ? parameters : null)}{(command.Aliases.Count > 0 ? aliases : null)}";
                    catpos++;
                }
                else
                    pages[pos] += $"\n{posit++}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}{(command.Parameters.Count > 0 ? parameters : null)}{(command.Aliases.Count > 0 ? aliases : null)}";
                prevCategoryAttribute = categoryAttribute;
            }
            if (page > 0)
            {
                if (page > pages.Count)
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "Справка по командам",
                        Description = pages.Last(),
                        Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                    }.Build());
                else
                    await ReplyAsync(embed: new EmbedBuilder
                    {
                        Title = "Справка по командам",
                        Description = pages[page - 1],
                        Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                    }.Build());
            }
            else
                await PagedReplyAsync(pager: new PaginatedMessage
                {
                    Title = "Справка по командам",
                    Pages = pages,
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild),
                    Options = new PaginatedAppearanceOptions
                    {
                        Jump = null,
                        Info = null,
                        Stop = null
                    }
                });
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
                try
                {
                    await ReplyAsync("Начинаю удаление сообщений...");
                    var deleteMessagesThread = new Thread(new ParameterizedThreadStart(ClearMessages));
                    deleteMessagesThread.Start(count);
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("Error while cleaning", Color.Red);
                    var errMess = await ReplyAsync("Сообщения двухнедельной давности, поэтому не могу удалить.");
                    await Task.Delay(1000);
                    await errMess.DeleteAsync();
                }
            }
            else
            {
                Console.WriteLine("Error while cleaning", Color.Red);
                var errMess = await ReplyAsync("Ты не можешь удалять более 100 сообщений за раз");
                await Task.Delay(1000);
                await errMess.DeleteAsync();
            }

        }

        [Command("СерверПоддержки")]
        [Alias("Поддержка")]
        [StandartCommand]
        [Summary("получает приглашение на мой сервер поддержки.")]
        public async Task MyServer()
        {
            await ReplyAsync("https://discord.gg/p6R4yk7uqK");
        }

        [Command("ДобавитьБота")]
        [StandartCommand]
        [Alias("Добавить")]
        [Summary("получает ссылку-приглашение меня на твой сервер")]
        public async Task InviteLink()
        {
            await ReplyAsync("*Перейди по ссылке и пригласи меня*\n https://discord.com/oauth2/authorize?client_id=749991391639109673&scope=bot&permissions=1573583991");
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
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                }.Build();

                var embedToReportedUser = new EmbedBuilder
                {
                    Title = $"На тебя поступила жалоба от {Context.User.Username}",
                    Description = $"**Причина:**\n`{reasonForm}`",
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
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
            var contextRoles = (Context.User as SocketGuildUser).Roles;
            int contextMaxPos = 0;

            foreach (var role in contextRoles)
                if (role.Position > contextMaxPos)
                    contextMaxPos = role.Position;

            var toKickRoles = user.Roles;
            int toKickMaxPos = 0;

            foreach (var role in toKickRoles)
                if (role.Position > toKickMaxPos)
                    toKickMaxPos = role.Position;

            if (contextMaxPos == toKickMaxPos)
            {
                await ReplyAsync("Ты не можешь его кикнуть, т.к. ты стоишь в ролевой иерархии вместе с ним/ее.");
                return;
            }

            if (contextMaxPos < toKickMaxPos)
            {
                await ReplyAsync("Ты не можешь его кикнуть, т.к. ты стоишь ниже него/его в ролевой иерархии.");
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
            var contextRoles = (Context.User as SocketGuildUser).Roles;
            int contextMaxPos = 0;

            foreach (var role in contextRoles)
                if (role.Position > contextMaxPos)
                    contextMaxPos = role.Position;

            var toBanRoles = user.Roles;
            int toBanMaxPos = 0;

            foreach (var role in toBanRoles)
                if (role.Position > toBanMaxPos)
                    toBanMaxPos = role.Position;

            if (contextMaxPos == toBanMaxPos)
            {
                await ReplyAsync("Ты не можешь его забанить, т.к. ты стоишь в ролевой иерархии вместе с ним/ее.");
                return;
            }

            if (contextMaxPos < toBanMaxPos)
            {
                await ReplyAsync("Ты не можешь его забанить, т.к. ты стоишь ниже него/его в ролевой иерархии.");
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
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            string message = null;
            string noMessageToUsers = null;

            for (int i = 0; i < mess.Length; i++)
                message += i == 0 ? mess[i] : $" {mess[i]}";

            foreach (var user in Context.Guild.Users)
            {
                if (!user.IsBot)
                {

                    try
                    {
                        var ch = await user.GetOrCreateDMChannelAsync();
                        if (ch != null)
                            await ch.SendMessageAsync(message);
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

            await ReplyAsync($"Добавлено.");
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
        #endregion

        #region --КОММУНИКАЦИЯ--
        [Command("ДобавитьСоединение")]
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
        [Summary("удаляет соединение с каналами других серверов.")]
        [StandartCommand]
        public async Task DeleteConnection()
        {
            var connectors = FilesProvider.GetConnectors(Context.Guild);
            SerializableConnector connector = null;
            if (connectors != null)
                foreach (var conn in connectors.SerializableConnectorsChannels)
                    if (Context.Channel.Id == conn.HostId)
                        connector = conn;

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
            var reactRoleMessages = FilesProvider.GetReactRoleMessages();
            List<ulong> reactRoleMessagesId = new List<ulong>();

            if (channel.GetMessageAsync(id) == null)
            {
                await ReplyAsync($"Не могу найти сообщение в {channel.Mention}.");
                return;
            }

            foreach (var reactRoleMessage in reactRoleMessages.ReactRoleMessages)
                reactRoleMessagesId.Add(reactRoleMessage.Id);

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

        [Command("УдалитьРольЗаРеакцию")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("удаляет \"Роль за реакцию\" у сообщения")]
        [StandartCommand]
        public async Task DeleteReaction(ulong id, Emoji emoji)
        {
            var reactRoleMessages = FilesProvider.GetReactRoleMessages();
            List<ulong> reactRoleMessagesId = new List<ulong>();

            foreach (var reactRoleMessage in reactRoleMessages.ReactRoleMessages)
                reactRoleMessagesId.Add(reactRoleMessage.Id);

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
         
        [Command("КупитьРоль")]
        [RolesCommand]
        [Summary("покупает роль участнику. Ее нужно упомянуть.")]
        public async Task BuyRole(IRole role)
        {
            if (Context.Message.MentionedRoles.Count > 0)
            {
                var economProvider = new EconomicProvider(Context.Guild);
                var res = economProvider.BuyRole(role, Context.User as SocketGuildUser);
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

        [Command("Монетка")]
        [RolesCommand]
        [Summary("играй на удачу и получай валюту!")]
        public async Task Monet(int count)
        {
            var economProvider = new EconomicProvider(Context.Guild);
            Random random = new Random();
            int value = random.Next(0, 100);
            var economUser = economProvider.GetEconomicGuildUser(Context.User.Id);

            if (economUser.Item1.Balance >= count)
            {
                if (value <= 30)
                {
                    economProvider.AddBalance(Context.User, count);
                    await ReplyAsync("Ты выиграл!");
                }
                else
                {
                    economProvider.MinusBalance(Context.User, count);
                    await ReplyAsync("Ты проиграл!");
                }
            }
            else
                await ReplyAsync("Ты не можешь поставить ставку, т.к. у тебя недостаточно средств.");
        }
        #endregion

        #region --КАСТОМНЫЕ КОМАНДЫ--
        [Command("ВсеКоманды")]
        [CustomCommand]
        [Summary("выводит все кастомные команды.")]
        public async Task GetAllCustomCommands()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var guild = Context.Guild;
            var serial = new CustomCommandsSerial(guild);
            var commands = serial.GetCustomCommands();
            string allComm = null;
            int arg = 1;

            foreach (var command in commands.Commands)
            {
                allComm += $"\n{arg++}. {command.Name}. Алгоритм:";
                var actions = command.Actions;
                int argPos = 1;
                foreach (var action in actions)
                    switch (action.Item1)
                    {
                        case SerializableCommand.CommandActionType.Message:
                            allComm += $"\n{arg - 1}.{argPos++}) Отправляет сообщение ({(action.Item2 as SerializableMessage).Message}).";
                            break;
                        case SerializableCommand.CommandActionType.Kick:
                            allComm += $"\n{arg - 1}.{argPos++}) Кикает с сервера.";
                            break;
                        case SerializableCommand.CommandActionType.Ban:
                            allComm += $"\n{arg - 1}.{argPos++}) Банит на сервере.";
                            break;
                        case SerializableCommand.CommandActionType.Interactive:
                            allComm += $"\n{arg - 1}.{argPos++}) Ожидает нового сообщения.";
                            break;
                    }
            }


            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = $"Кастомные команды сервера {Context.Guild.Name}",
                Description = allComm,
                Color = ColorProvider.GetColorForCurrentGuild(serGuild)
            }.Build());
        }

        [Command("ПримерКоманды")]
        [CustomCommand]
        [Summary("отправляет XML файл с примером кастомной команды")]
        public async Task SendExample()
        {
            string name = "example.xml";

            try
            {
                var example = new SerializableCommand
                {
                    GuildId = Context.Guild.Id,
                    Name = "ПриветМир",
                    Actions = new List<ValueTuple<SerializableCommand.CommandActionType, object>>
                { new ValueTuple<SerializableCommand.CommandActionType, object>(SerializableCommand.CommandActionType.Message, new SerializableMessage { Message = "Привет мир!", DataFromBuffer = false }) }
                };
                using (FileStream fs = new FileStream(name, FileMode.Create))
                {
                    var xml = new XmlSerializer(typeof(SerializableCommand), new[] { typeof(SerializableBan), typeof(SerializableKick), typeof(SerializableMessage) });
                    xml.Serialize(fs, example);
                }

                await Context.Channel.SendFileAsync(name, "Пример кастомной команды");

                File.Delete(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
            }
            finally
            {
                if (File.Exists(name))
                    File.Delete(name);
            }
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ДобавитьКоманду")]
        [CustomCommand]
        [Summary("добавляет команду.")]
        public async Task AddCommand()
        {
            var provider = new CustomCommandsProvider(Context.Guild);

            await provider.AddCommand(Context);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("К")]
        [CustomCommand]
        [Summary("позволяет использовать кастомную команду.")]
        public async Task UseCommand(string name, params string[] args)
        {
            CustomCommandsCore core = new CustomCommandsCore(Context);

            await core.ExecuteCommand(name);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("УдалитьКоманду")]
        [CustomCommand]
        [Summary("удаляет команду.")]
        public async Task DeleteCommand(string name)
        {
            var serial = new CustomCommandsSerial(Context.Guild);
            serial.DeleteCommand(name);
            await ReplyAsync($"Команда {name} удалена успешно");
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
            SerializableGuild serializableGuild = FilesProvider.GetGuild(Context.Guild);

            serializableGuild.DefaultRoleId = role.Id;

            FilesProvider.RefreshGuild(serializableGuild);

            await ReplyAsync("Роль успешно задана. Она будет выдаваться всем пользователям по-умолчанию.");

            foreach (var user in Context.Guild.Users)
                await user.AddRoleAsync(role);
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

        [Command("ДобавитьНежелательныеСлова")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("добавляет список нежелательных слов")]
        public async Task AddBadWords(params string[] wordsInMess)
        {
            if (wordsInMess.Length == 0)
            {
                if (Context.Message.Attachments.Count > 0)
                {
                    var attachment = Context.Message.Attachments.First();
                    if (Path.GetExtension(attachment.Filename) == ".txt")
                    {
                        WebClient client = new WebClient();
                        Stream stream = client.OpenRead(attachment.Url);
                        SerializableGuild guild = FilesProvider.GetGuild(Context.Guild);

                        using StreamReader reader = new StreamReader(stream);
                        string text = await reader.ReadToEndAsync();

                        string[] words = text.Split(' ');
                        List<string> filteredWords = new List<string>();

                        foreach (string word in words)
                            if (!filteredWords.Contains(word.ToLower()) && !guild.BadWords.Contains(word.ToLower()))
                                filteredWords.Add(word.ToLower());

                        guild.BadWords.AddRange(filteredWords);

                        FilesProvider.RefreshGuild(guild);

                        await ReplyAsync($"Добавлено {filteredWords.Count} слов. На данный момент в словаре есть {guild.BadWords.Count} слов.");
                    }
                    else
                        await ReplyAsync("Некорректное расширение файла. Файл должен быть текстовый (`.txt`)");
                }
                else
                    await ReplyAsync("Я не могу найти файл в сообщении.");
            }
            else
            {
                SerializableGuild guild = FilesProvider.GetGuild(Context.Guild);
                List<string> filteredWords = new List<string>();

                foreach (string word in wordsInMess)                
                    if (!filteredWords.Contains(word) && !guild.BadWords.Contains(word))
                        filteredWords.Add(word);

                guild.BadWords.AddRange(filteredWords);
                FilesProvider.RefreshGuild(guild);

                await ReplyAsync($"Добавлено {filteredWords.Count} слов. На данный момент в словаре есть {guild.BadWords.Count} слов.");
            }
        }

        [Command("УдалитьНежелательноеСлово")]
        [CustomisationCommand]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("удаляет нежелательные слова из списка.")]
        public async Task DeleteWord(params string[] words)
        {
            if (words.Length > 0)
            {
                SerializableGuild guild = FilesProvider.GetGuild(Context.Guild);

                foreach (string word in words)                
                    if (guild.BadWords.Contains(word))                    
                        guild.BadWords.Remove(word);    

                FilesProvider.RefreshGuild(guild);
                await ReplyAsync("Удаление слов произведено успешно.");
            }
            else
                await ReplyAsync("Не могу найти слова в сообщении.");
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
                await ReplyAsync("Теперь не я буду начислять предупреждения за нежелательные слова");

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
        [Command("Уведомления", RunMode = RunMode.Async)]
        [Alias("Логирование", "Лог")]
        [Summary("включает/выключает уведомления сервера. При бане, кике, добавлении на сервер пользователя бот тебя уведомит")]
        [CustomisationCommand]
        public async Task EnableGuildNotifications()
        {            
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
        
        #endregion

        private async void ClearMessages(object count)
        {
            try
            {
                var messages = await Context.Channel.GetMessagesAsync((int)count + 2).FlattenAsync();
                await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
                var delMess = await ReplyAsync("Удаление сообщений произведено успешно");
                Console.WriteLine("Cleared", Color.Green);
                Thread.Sleep(1000);
                await delMess.DeleteAsync();
                Thread.Sleep(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
            }
        }
    }
}