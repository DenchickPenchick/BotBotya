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
using DiscordBot.Modules.FileManaging;
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
        [Command("Хелп")]
        [StandartCommand]
        [Summary("позволяет узнать полный список команд")]
        public async Task Help()
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

                if (prevCategoryAttribute.CategoryName != categoryAttribute.CategoryName || $"\n{posit + 1}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}".Length >= 2048)
                {
                    if (prevCategoryAttribute.CategoryName != categoryAttribute.CategoryName)
                        posit = 1;

                    pages.Add($"\n**{categoryAttribute.CategoryName}**\n{posit++}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}");
                    pos++;
                }
                else if (catpos == 0)
                {
                    pages[0] += $"\n**{categoryAttribute.CategoryName}**\n{posit++}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}";
                    catpos++;
                }
                else
                    pages[pos] += $"\n{posit++}. Команда `{serGuild.Prefix}{command.Name}` {command.Summary}";
                prevCategoryAttribute = categoryAttribute;
            }

            await PagedReplyAsync(pager: new PaginatedMessage
            {
                Title = "Справка по командам",
                Pages = pages,
                Color = Color.Blue,
                Options = new PaginatedAppearanceOptions
                {
                    Jump = null,
                    Info = null
                }
            });
        }

        [Command("Очистить", RunMode = RunMode.Async)]
        [StandartCommand]
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

        [Command("МойСервер")]
        [StandartCommand]
        [Summary("получает приглашение на мой сервер поддержки.")]
        public async Task MyServer()
        {
            await ReplyAsync("https://discord.gg/p6R4yk7uqK");
        }

        [Command("ДобавитьБота")]
        [StandartCommand]
        [Summary("получает ссылку-приглашение меня на твоей сервер")]
        public async Task InviteLink()
        {
            await ReplyAsync("*Перейди по ссылке и пригласи меня*\n https://discord.com/oauth2/authorize?client_id=749991391639109673&scope=bot&permissions=1573583991");
        }

        [Command("Жалоба", RunMode = RunMode.Async)]
        [StandartCommand]
        [Summary("отпраляет жалобу на участника сервера.")]
        public async Task ReportUser()
        {
            await ReplyAsync("Упомяни пользователя, на которого ты хочешь подать жалобу.");
            var replyUser = await NextMessageAsync();
            if (replyUser.MentionedUsers.Count == 0)
            {
                await ReplyAsync("Не найдено ни одного пользователя в сообщении");
                return;
            }

            if (replyUser != null)
            {
                await ReplyAsync("Введи причину жалобы.");
                var reasonReply = await NextMessageAsync();
                if (reasonReply != null)
                {
                    var userReport = replyUser.MentionedUsers.ToArray()[0];
                    if (userReport != null)
                    {
                        await ReplyAsync("Жалоба на участника отпралена на рассмотрение владельцу сервера.");
                        var embedToAdmin = new EmbedBuilder
                        {
                            Title = $"Поступила жалоба на участника {userReport.Username}",
                            Description = $"Причина:\n{reasonReply.Content}",
                            ImageUrl = userReport.GetAvatarUrl(),
                            Color = Color.Blue
                        }.Build();

                        var embedToReportedUser = new EmbedBuilder
                        {
                            Title = $"На тебя поступила жалоба от {Context.User.Username}",
                            Description = $"Причина:\n{reasonReply.Content}",
                            Color = Color.Blue
                        }.Build();

                        await Context.Guild.Owner.GetOrCreateDMChannelAsync().Result.SendMessageAsync(embed: embedToAdmin);
                        await userReport.GetOrCreateDMChannelAsync().Result.SendMessageAsync(embed: embedToReportedUser);
                    }
                    else
                        await ReplyAsync("Пользователь не найден.");
                }
                else
                    await ReplyAsync("Ответ не получен в течение 5 минут. Команда аннулированна");
            }
            else
                await ReplyAsync("Ответ не получен в течение 5 минут. Команда аннулированна");
        }

        [RequireUserPermission(GuildPermission.KickMembers)]
        [Command("Кик")]
        [StandartCommand]
        [Summary("позволяет кикнуть пользователя с сервера (У тебя должно быть право на эту команду).")]
        public async Task Kick(params string[] NameOfUser)
        {
            var contextRoles = (Context.User as SocketGuildUser).Roles;
            int contextMaxPos = 0;

            foreach (var role in contextRoles)
                if (role.Position > contextMaxPos)
                    contextMaxPos = role.Position;
            
            SocketGuildUser user;

            if (Context.Message.MentionedUsers.Count > 0)
                user = Context.Message.MentionedUsers.First() as SocketGuildUser;
            else
                user = GetSocketGuildUser(NameOfUser);

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
                await user.KickAsync();
            else
                await ReplyAsync($"Пользователь не найден.");
        }

        [RequireUserPermission(GuildPermission.BanMembers)]
        [Command("Бан")]
        [Summary("позволяет забанить пользователя на сервере (У тебя должно быть право на эту команду).")]
        public async Task Ban(params string[] NameOfUser)
        {
            var contextRoles = (Context.User as SocketGuildUser).Roles;
            int contextMaxPos = 0;

            foreach (var role in contextRoles)
                if (role.Position > contextMaxPos)
                    contextMaxPos = role.Position;

            SocketGuildUser user;

            if (Context.Message.MentionedUsers.Count > 0)
                user = Context.Message.MentionedUsers.First() as SocketGuildUser;
            else
                user = GetSocketGuildUser(NameOfUser);

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
                await user.BanAsync();
            else
                await ReplyAsync($"Пользователь не найден. Проверь данные.");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("МедленныйРежим")]
        [StandartCommand]
        [Summary("позволяет включить медленный режим в канале (У тебя должно быть право на эту команду).")]
        public async Task ChangePosOfSlowMode(int time = 0)
        {
            if (time >= 0 && time <= 21600)
            {
                var channel = Context.Channel as SocketTextChannel;
                await channel.ModifyAsync(x => x.SlowModeInterval = time);
                if (channel.SlowModeInterval == 0)
                    await ReplyAsync($"На канале {Context.Channel.Name} отключен медленный режим");
                else
                    await ReplyAsync($"На канале {Context.Channel.Name} включен медленный режим. Интервал: {time} секунд");
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
            string message = null;

            for (int i = 0; i < mess.Length; i++)
                message += i == 0 ? mess[i] : $" {mess[i]}";

            foreach (var user in Context.Guild.Users)
            {
                if (!user.IsBot)
                {
                    var ch = await user.GetOrCreateDMChannelAsync();
                    if (ch != null)
                        await ch.SendMessageAsync(message);
                }

            }
            await ReplyAsync("Рассылка произведена успешно.");
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
        [Summary("ставит на паузу трек")]
        public async Task PauseTrackAsync()
        {
            await LavaOperations.PauseTrackAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Воспроизведение")]
        [MusicCommand]
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
        public async Task AddEconomicRole(int price, params string[] str)
        {
            if (Context.Message.MentionedRoles.Count > 0)
            {
                var economProvider = new EconomicProvider(Context.Guild);
                var role = Context.Message.MentionedRoles.First();

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
        public async Task DeleteRole(params string[] str)
        {
            if (Context.Message.MentionedRoles.Count > 0)
            {
                var economProvider = new EconomicProvider(Context.Guild);
                var role = Context.Message.MentionedRoles.First();

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
        public async Task BuyRole(params string[] str)
        {
            if (Context.Message.MentionedRoles.Count > 0)
            {
                var economProvider = new EconomicProvider(Context.Guild);
                var role = Context.Message.MentionedRoles.First();
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
        public async Task AddBalance(int count, params string[] str)
        {
            var economProvider = new EconomicProvider(Context.Guild);

            string usersList = null;

            if (Context.Message.MentionedUsers.Count > 0)
            {
                foreach (var user in Context.Message.MentionedUsers)
                {
                    economProvider.AddBalance(user, count);
                    var economUser = FilesProvider.GetEconomicGuildUser(user as SocketGuildUser);

                    usersList += $"\n{(user as SocketGuildUser).Mention} Баланс: {economUser.Balance}";
                }

                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Новый баланс участников сервера",
                    Description = usersList,
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Color = Color.Blue
                }.Build());
            }
            else
                await ReplyAsync("Не могу найти участника(-ов) в сообщении.");
        }

        [Command("УменьшитьБаланс")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RolesCommand]
        [Summary("позволяет уменьшить баланс участника(-м) сервера (его(их) нужно упомянуть).")]
        public async Task MinusBalance(int count, params string[] str)
        {
            var economProvider = new EconomicProvider(Context.Guild);

            string usersList = null;

            if (Context.Message.MentionedUsers.Count > 0)
            {
                foreach (var user in Context.Message.MentionedUsers)
                {
                    economProvider.MinusBalance(user, count);
                    var economUser = FilesProvider.GetEconomicGuildUser(user as SocketGuildUser);

                    usersList += $"\n{(user as SocketGuildUser).Mention} Баланс: {economUser.Balance}";
                }

                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Новый баланс участников сервера",
                    Description = usersList,
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Color = Color.Blue
                }.Build());
            }
            else
                await ReplyAsync("Не могу найти участника(-ов) в сообщении.");
        }

        [Command("УстановитьБаланс")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RolesCommand]
        [Summary("позволяет установить баланс участника(-м) сервера (его(их) нужно упомянуть).")]
        public async Task SetBalance(int count, params string[] str)
        {
            var economProvider = new EconomicProvider(Context.Guild);

            string usersList = null;

            if (Context.Message.MentionedUsers.Count > 0)
            {
                foreach (var user in Context.Message.MentionedUsers)
                {
                    economProvider.SetBalance(user, count);
                    var economUser = FilesProvider.GetEconomicGuildUser(user as SocketGuildUser);

                    usersList += $"\n{(user as SocketGuildUser).Mention} Баланс: {economUser.Balance}";
                }

                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "Новый баланс участников сервера",
                    Description = usersList,
                    ThumbnailUrl = Context.Guild.IconUrl,
                    Color = Color.Blue
                }.Build());
            }
            else
                await ReplyAsync("Не могу найти участника(-ов) в сообщении.");
        }

        [Command("РолиНаПродажу")]
        [RolesCommand]
        [Summary("позволяет получить список ролей на продажу.")]
        public async Task RolesOnSale()
        {
            var economProvider = new EconomicProvider(Context.Guild);

            var roles = economProvider.EconomicGuild.RolesAndCostList;

            List<string> pages =new List<string>
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
                    Color = Color.Blue
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
                    Color = Color.Blue
                });
        }

        [Command("Баланс")]
        [RolesCommand]
        [Summary("показывает твой баланс.")]
        public async Task GetBalance()
        {
            var economUser = FilesProvider.GetEconomicGuildUser(Context.User as SocketGuildUser);

            if (economUser != null)
            {
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = $"Баланс {Context.User.Username}: {economUser.Balance}",
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    Color = Color.Blue
                }.Build());
            }
            else
                await ReplyAsync("У тебя нет собственного счета. Можно отправить **любое** сообщение, чтобы его создать.");
        }

        [Command("НаградаЗаСообщение")]
        [RolesCommand]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("позволяет установить награду за каждое отправленное пользователем сообщение.")]
        public async Task RewardForMessage(int count)
        {
            var economGuild = FilesProvider.GetEconomicGuild(Context.Guild);

            economGuild.RewardForMessage = count;

            FilesProvider.RefreshEconomicGuild(economGuild);

            await ReplyAsync($"Теперь награда за сообщение равна {economGuild.RewardForMessage}");
        }
        #endregion

        #region --КАСТОМНЫЕ КОМАНДЫ--
        [Command("ВсеКоманды")]
        [CustomCommand]
        [Summary("выводит все кастомные команды.")]
        public async Task GetAllCustomCommands()
        {
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
                Color = Color.Blue
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
        [Command("Конфигурация")]
        [CustomisationCommand]
        [Summary("получает конфигурацию сервера.")]
        public async Task GetGuildConfig()
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);
            var links = Context.Guild.GetTextChannel(serGuild.SystemChannels.LinksChannelId);
            var videos = Context.Guild.GetTextChannel(serGuild.SystemChannels.VideosChannelId);
            var rooms = Context.Guild.GetVoiceChannel(serGuild.SystemCategories.VoiceRoomsCategoryId);            

            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = $"Конфигурация сервера {Context.Guild.Name}",
                Description = $"Приветственные сообщения: {(serGuild.HelloMessageEnable == true ? "Включены" : "Выключены")}\n" +
                $"{(serGuild.HelloMessage != null && serGuild.HelloMessageEnable == true ? $"Текст приветственного сообщения: {serGuild.HelloMessage}\n" : null)}" +
                $"{(links == null && videos == null ? null : $"Каналы контента: {links?.Mention} {videos?.Mention}")}\n" +
                $"{(rooms == null ? null : $"Категория с комнатами: {rooms.Name}")}",                
                Color = Color.Blue,
                ImageUrl = Context.Guild.IconUrl
            }.Build());
        }

        [Command("ПоменятьНикнеймБота")]
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

        [Command("СконфигурироватьСервер")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [CustomisationCommand]
        [Summary("конфигурирует сервер с соответствующим XML файлом")]
        public async Task ConfigureGuild()
        {
            Compiler compiler = new Compiler(Compiler.CompilerTypeEnum.Guild);
            WebClient webClient = new WebClient();
            var attachedFile = Context.Message.Attachments.ToArray()[0];
            if (Path.GetExtension(attachedFile.Filename) == ".xml")
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializableGuild));
                bool deserializable = serializer.CanDeserialize(new XmlTextReader(webClient.OpenRead(attachedFile.Url)));
                if (deserializable)
                {
                    try
                    {
                        var serGuild = (SerializableGuild)serializer.Deserialize(webClient.OpenRead(attachedFile.Url));
                        var res = compiler.Result(Context.Guild, Context.Message);
                        await ReplyAsync(embed: res);
                        if (res.Color != Color.Red)
                        {
                            serGuild.GuildId = Context.Guild.Id;
                            FilesProvider.RefreshGuild(serGuild);
                            await ReplyAsync("Сервер успешно сконфигурирован");
                        }
                    }
                    catch
                    {
                        await ReplyAsync(embed: new CompilerEmbed
                        { 
                        Errors = new List<ErrorField>
                        { 
                        new ErrorField("Нарушена типовая структура сервера. Возможный вариант решения проблемы: проверьте написание команд, тегов и т.д.")
                        }}.Build());
                    }
                }
                else
                    await ReplyAsync("Неправильно сформирован файл");
            }
            else
                await ReplyAsync("Неверный формат файла");
        }

        [Command("ФайлКонфигурации")]
        [CustomisationCommand]
        [Summary("возвращает XML файл конфигурации сервера")]
        public async Task GetConfigFile()
        {
            await Context.Channel.SendFileAsync($"{FilesProvider.GetBotDirectoryPath()}/BotGuilds/{Context.Guild.Id}.xml", $"Файл конфигурации сервера {Context.Guild.Name}");
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [CustomisationCommand]
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
        [Summary("выключает или включает приветственные сообщения. Отредактировать сообщение можно с помощью команды РедактироватьПриветственноеСообщение (У тебя должно быть право на выполнение этой команды).")]
        [CustomisationCommand]
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
        [Summary("создает канал с возможностью создания комнат. Возможно удаление комнат. У тебя должно быть право на выполнение этой команды.")]
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
        [Summary("создает каналы контента. Возможно удаление каналов. У тебя должно быть право на выполнение этой команды.")]
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
        [Summary("включает/выключает проверку контента (сортировку видео, ссылок по нужным каналам). Работает только при включенных каналах контента. У тебя должно быть право на выполнение этой команды.")]
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

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("Уведомления", RunMode = RunMode.Async)]
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
        public async Task HelloChannel(params string[] str)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            if (Context.Message.MentionedChannels.Count > 0)
            {
                serGuild.SystemChannels.NewUsersChannelId = Context.Message.MentionedChannels.First().Id;
                FilesProvider.RefreshGuild(serGuild);
            }
            else
                await ReplyAsync("Не могу найти канал.");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("КаналДляПрощаний")]
        [CustomisationCommand]
        [Summary("устанавливает канал для прощаний.")]
        public async Task GoodbyeChannel(params string[] str)
        {
            var serGuild = FilesProvider.GetGuild(Context.Guild);

            if (Context.Message.MentionedChannels.Count > 0)
            {
                serGuild.SystemChannels.LeaveUsersChannelId = Context.Message.MentionedChannels.First().Id;
                FilesProvider.RefreshGuild(serGuild);
            }
            else
                await ReplyAsync("Не могу найти канал.");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("УведомлениеОНеправильнойКоманде")]
        [CustomisationCommand]
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
        #endregion        

        private SocketGuildUser GetSocketGuildUser(params string[] NameOfUser)
        {
            string name = null;
            int argPos = 0;
            var guild = Context.Guild;
            SocketGuildUser User = null;

            foreach (string partOfName in NameOfUser)
            {
                name += argPos == 0 ? partOfName : $" {partOfName}";
                argPos++;
            }

            foreach (var user in guild.Users)
                if ((user.Username == name || user.Nickname == name) && name != null)
                    User = user;

            return User;
        }

        private async void ClearMessages(object count)
        {            
            var messages = await Context.Channel.GetMessagesAsync((int)count + 2).FlattenAsync();            
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            var delMess = await ReplyAsync("Удаление сообщений произведено успешно");
            Console.WriteLine("Cleared", Color.Green);
            Thread.Sleep(1000);
            await delMess.DeleteAsync();
            Thread.Sleep(0);            
        }
    }
}