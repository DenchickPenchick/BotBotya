using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Console = Colorful.Console;
using DiscordBot.FileWorking;
using DiscordBot.MusicOperations;
using DiscordBot.Providers;
using System.Collections.Generic;
using DiscordBot.CustomCommands;
using System.IO;
using System.Xml.Serialization;
using Victoria;

namespace TestBot
{
    public class Commands : InteractiveBase
    {
        private readonly LavaOperations LavaOperations;

        public Commands(LavaOperations lavaOperations)
        {
            LavaOperations = lavaOperations;
        }

        #region --СТАНДАРТНЫЕ КОМАНДЫ--
        [Command("Новости")]
        [Summary("позволяет узнать последние новости")]
        public async Task UpdateNews()
        {
            await ReplyAsync(embed: FilesProvider.GetNewsAndPlans().GetNewsAndPlansEmbed(Context.Client));
        }

        [Command("Очистить", RunMode = RunMode.Async)]
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

        [Command("Жалоба", RunMode = RunMode.Async)]
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
        [Summary("позволяет кикнуть пользователя с сервера (У тебя должно быть право на эту команду).")]
        public async Task Kick(params string[] NameOfUser)
        {
            SocketGuildUser user = GetSocketGuildUser(NameOfUser);
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
            SocketGuildUser user = GetSocketGuildUser(NameOfUser);
            if (user != null)
                await user.BanAsync();
            else
                await ReplyAsync($"Пользователь не найден. Проверь данные.");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("МедленныйРежим")]
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
        #endregion

        #region --МУЗЫКАЛЬНЫЕ КОМАНДЫ--
        [Command("Подключиться")]
        [Summary("подключает бота к голосовому каналу")]
        public async Task JoinAsync()
        {
            await LavaOperations.JoinAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Покинуть")]
        [Summary("отключает бота от канала")]
        public async Task LeaveAsync()
        {
            await LavaOperations.LeaveAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Играть")]
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
        [Summary("включает трек, который задан url")]
        public async Task StopTrackAsync()
        {
            await LavaOperations.StopTrackAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Пауза")]
        [Summary("ставит на паузу трек")]
        public async Task PauseTrackAsync()
        {
            await LavaOperations.PauseTrackAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Воспроизведение")]
        [Summary("продолжает трек, который стоит на паузе")]
        public async Task ResumeTrackAsync()
        {
            await LavaOperations.ResumeTrackAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel);
        }

        [Command("Громкость")]
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

        #region --КАСТОМНЫЕ КОМАНДЫ--
        [Command("ВсеКоманды")]
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
                    switch (action)
                    {
                        case CustomCommand.Action.Message:
                            allComm += $"\n{arg - 1}.{argPos++}) Отправляет сообщение ({command.Message}).";
                            break;
                        case CustomCommand.Action.Kick:
                            allComm += $"\n{arg - 1}.{argPos++}) Кикает с сервера.";
                            break;
                        case CustomCommand.Action.Ban:
                            allComm += $"\n{arg - 1}.{argPos++}) Банит на сервере.";
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

        [Command("СконфигурироватьКоманду", RunMode = RunMode.Async)]
        [Summary("конфигурирует команду и возвращает XML файл.")]
        public async Task ConfigureCommand()
        {
            List<CustomCommand.Action> actions = new List<CustomCommand.Action>();
            string name;
            string message = null;
            bool actionsEntering = false;
            await ReplyAsync("Введи название команды");
            var replyForName = await NextMessageAsync();

            if (replyForName != null)
                name = replyForName.Content.Replace(" ", string.Empty);
            else
            {
                await ReplyAsync("Ответ не получен в течение 5 минут. Команда аннулированна");
                return;
            }

            while (!actionsEntering)
            {
                await ReplyAsync("Добавить действие (Сообщение (отправить сообщение), Кик(Кикнуть пользователя), Бан(Забанить пользователя)). Если все действия добавлены введи \"Стоп\". Если нужно остановить процесс введи \"Остановить\"");
                var replyForAction = await NextMessageAsync();
                if (replyForAction != null)
                {
                    switch (replyForAction.Content.ToLower())
                    {
                        case "сообщение":
                            actions.Add(CustomCommand.Action.Message);
                            await ReplyAsync("Действие добавлено");
                            break;
                        case "кик":
                            actions.Add(CustomCommand.Action.Kick);
                            await ReplyAsync("Действие добавлено");
                            break;
                        case "бан":
                            actions.Add(CustomCommand.Action.Ban);
                            await ReplyAsync("Действие добавлено");
                            break;
                        case "стоп":
                            actionsEntering = true;
                            break;
                        case "остановить":
                            return;
                        default:
                            await ReplyAsync("Неверное действие");
                            break;
                    }                    
                }                
                else
                {
                    await ReplyAsync("Ответ не получен в течение 5 минут. Команда аннулированна");
                    return;
                }
            }

            if (actions.Contains(CustomCommand.Action.Message))
            {
                await ReplyAsync("Введи сообщение, которое ты хочешь отправить");
                var replyForMessage = await NextMessageAsync();
                if (replyForName != null)
                    message = replyForMessage.Content;
            }                        

            var command = new CustomCommand
            {
                Name = name,
                Actions = actions,
                GuildId = Context.Guild.Id,
                Message = message
            };
            using FileStream stream = new FileStream($"{Context.Guild.Id}.xml", FileMode.CreateNew);
            XmlSerializer serializer = new XmlSerializer(typeof(CustomCommand));
            serializer.Serialize(stream, command);
            stream.Close();
            await Context.Channel.SendFileAsync($"{Context.Guild.Id}.xml", "Твой файл. Если ты хочешь добавить данную команду, тогда скачай файл и пропиши соответствующую команду.");
            File.Delete($"{Context.Guild.Id}.xml");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("ДобавитьКоманду")]
        [Summary("добавляет команду.")]
        public async Task AddCommand()
        {
            var provider = new CustomCommandsProvider(Context.Guild);

            var res = await provider.AddCommand(Context);
            if (res == CustomCommandsProvider.Result.Error)
                await ReplyAsync("Возможно ты не прикрепил файл или же файл сконфигурирован неправильно.");
            else
                await ReplyAsync("Команда создана успешно");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("К")]
        [Summary("позволяет использовать кастомную команду.")]
        public async Task UseCommand(string name, params string[] args)
        {
            var provider = new CustomCommandsProvider(Context.Guild);

            var res = await provider.ExecuteCustomCommandAsync(Context, name);

            if (res == CustomCommandsProvider.Result.Error)
                await ReplyAsync("Возможно ты указал не все параметры, которые нужны команде. Либо же в команде стоят две (или более) несовместимых действий (например, кик и бан). Также команды может вообще не существовать!");
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("УдалитьКоманду")]
        [Summary("удаляет команду.")]
        public async Task DeleteCommand(string name)
        {
            var serial = new CustomCommandsSerial(Context.Guild);
            serial.DeleteCommand(name);
            await ReplyAsync($"Команда {name} удалена успешно");
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