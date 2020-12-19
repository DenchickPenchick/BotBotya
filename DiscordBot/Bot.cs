using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using DiscordBot.Modules.ContentManaging;
using DiscordBot.Modules.ProcessManage;
using DiscordBot.RoomManaging;
using Microsoft.Extensions.DependencyInjection;
using Console = Colorful.Console;
using TestBot;
using DiscordBot.GuildManaging;
using DiscordBot.Modules.NotificationsManaging;
using System.Collections.Generic;
using DiscordBot.FileWorking;

namespace DiscordBot
{
    public class Bot
    {        
        public string TOKEN = null;
        public string PathToBotDirectory = null;
        
        public DiscordSocketClient Client;
        public CommandService Commands;
        public CommandService BotOptionsCommands;
        public IServiceProvider Services;                                      

        public async Task RunBotAsync()
        {            
            Console.WriteAscii("Discord Bot Console", Color.Blue);

            ActivityType activityType = ActivityType.Watching;            
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true
            });            

            Commands = new CommandService();
            BotOptionsCommands = new CommandService();
            InteractiveService interactiveService = new InteractiveService(Client, new InteractiveServiceConfig
            {
                DefaultTimeout = TimeSpan.FromMinutes(5)
            });
            Services = new ServiceCollection()
                .AddSingleton(Client)                
                .AddSingleton(interactiveService)
                .BuildServiceProvider();
            new ProcessingModule(new ProcessingConfiguration
            {
                DiscordSocketClient = Client,
                RoomModule = new RoomModule(Client),
                ContentModule = new ContentModule(Client),
                FileModule = new FilesModule(this),
                GuildModule = new GuildModule(Client),
                NotificationsModule = new LogModule(Client)                
            }).RunModule();            

            await RegisterCommandsAsync();           

            await Client.LoginAsync(TokenType.Bot, TOKEN);

            await Client.StartAsync();            

            await Client.SetGameAsync("https://botbotya.ru", "https://discord.com/oauth2/authorize?client_id=749991391639109673&scope=bot&permissions=1573583991", activityType);            

            await Task.Delay(-1);
        }   
        
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            int argsPos = 0;

            try
            {
                var message = arg as SocketUserMessage;
                var provider = new GuildProvider((message.Author as SocketGuildUser).Guild);
                var context = new SocketCommandContext(Client, message);
                var interactive = new InteractiveService(Client);
                var serGuild = FilesProvider.GetGuild((message.Author as SocketGuildUser).Guild);
                if (message.Author.IsBot || message is null) return;                    

                if (message.HasStringPrefix(serGuild.Prefix, ref argsPos))
                {
                    if (message.Content.ToLower() == "!help")
                    {
                        int pos = 0;
                        int posit = 1;

                        List<string> pages = new List<string>
                        {
                            null
                        };

                        foreach (var command in Commands.Commands)
                        {
                            if ((pages[pos] + $"\n{posit+1}. Команда {command.Name} {command.Summary}").Length <= 2048)
                                pages[pos] += $"\n{posit++}. Команда {command.Name} {command.Summary}";
                            else
                            {
                                pages.Add($"\n{posit++}. Команда {command.Name} {command.Summary}");
                                pos++;
                            }                               
                        }

                        foreach (var command in BotOptionsCommands.Commands)
                        {
                            if ((pages[pos] + $"\n{pos+1}. Консольная команда {command.Name} {command.Summary}").Length <= 2048)
                                pages[pos] += $"\n{posit++}. Консольная команда {command.Name} {command.Summary}";
                            else
                            {
                                pages.Add($"\n{posit++}. Консольная команда {command.Name} {command.Summary}");
                                pos++;
                            }
                        }

                        await interactive.SendPaginatedMessageAsync(context, new PaginatedMessage
                        {
                            Pages = pages,
                            Color = Color.Blue,
                            Title = "🤖 Функционал бота 🤖",
                            Author = new EmbedAuthorBuilder
                            {
                                Name = Client.CurrentUser.Username,
                                IconUrl = Client.CurrentUser.GetAvatarUrl(),
                                Url = "https://discord.com/oauth2/authorize?client_id=749991391639109673&scope=bot&permissions=1573583991"
                            },
                            AlternateDescription = "Здесь показан функционал бота."
                        });                                     
                    }
                    else
                    {
                        IResult result;
                        if (message.Channel == provider.ConsoleChannel())
                            result = await BotOptionsCommands.ExecuteAsync(context, argsPos, Services);
                        else
                            result = await Commands.ExecuteAsync(context, argsPos, Services);

                        if (!result.IsSuccess)
                        {
                            if (result.Error != CommandError.UnknownCommand)
                            {
                                Console.WriteLine(result.ErrorReason, Color.Red);
                                Console.WriteLine("Command from:", Color.Red);
                                Console.WriteLine(context.User.Username);
                                Console.WriteLine("Command:", Color.Red);
                                Console.WriteLine(message);
                                Console.WriteLine("Command Status: Failed", Color.Red);
                            }                            
                            switch (result.Error)
                            {
                                case CommandError.UnknownCommand:
                                    await context.Channel.SendMessageAsync("Неизвестная команда.");
                                    break;
                                case CommandError.ParseFailed:
                                    await context.Channel.SendMessageAsync("Навеверное ты неправильно ввел данные.");
                                    break;
                                case CommandError.BadArgCount:
                                    await context.Channel.SendMessageAsync("Ты указал либо больше, либо меньше параметров чем нужно.");
                                    break;
                                case CommandError.ObjectNotFound:
                                    await context.Channel.SendMessageAsync("Объект не найден");
                                    break;
                                case CommandError.MultipleMatches:
                                    await context.Channel.SendMessageAsync("Обнаружены множественные совпадения. Проверь данные и введи команду повторно.");
                                    break;
                                case CommandError.UnmetPrecondition:
                                    await context.Channel.SendMessageAsync("У тебя нет доступа к этой команде.");
                                    break;
                                case CommandError.Exception:
                                    await context.Channel.SendMessageAsync("В результате выполнения команды было сгенерировано исключение. Информация об исключении отправлена разработчику.");
                                    break;
                                case CommandError.Unsuccessful:
                                    await context.Channel.SendMessageAsync("Команда выполнена неудачно.");
                                    break;
                            }
                        }                        
                    }
                }
            }
            catch (NullReferenceException)
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ex: {0}", ex, Color.Red);
            }
        }

        private async Task RegisterCommandsAsync()
        {           
            await Commands.AddModuleAsync<Commands>(Services);
            await BotOptionsCommands.AddModuleAsync<BotOptionsCommands>(Services);
            Client.MessageReceived += HandleCommandAsync;
        }
    }
}
