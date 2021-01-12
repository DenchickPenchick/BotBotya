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
using DiscordBot.FileWorking;
using Victoria;
using DiscordBot.MusicOperations;
using DiscordBot.Modules.MusicManaging;
using System.Reflection;
using DiscordBot.Providers;

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
                .AddSingleton(this)
                .AddSingleton(Client)
                .AddSingleton(interactiveService)
                .AddSingleton<CommandService>()
                .AddSingleton<Commands>()
                .AddSingleton(new LavaNode(Client, new LavaConfig
                {
                    ResumeTimeout = TimeSpan.MaxValue
                }))
                .AddSingleton(new LavaConfig())
                .AddLavaNode()
                .AddSingleton<LavaOperations>()
                .BuildServiceProvider();                                  

            new ProcessingModule(new ProcessingConfiguration
            {
                DiscordSocketClient = Client,
                RoomModule = new RoomModule(Client),
                ContentModule = new ContentModule(Client),
                FileModule = new FilesModule(this),
                GuildModule = new GuildModule(Client),
                NotificationsModule = new LogModule(Client),
                MusicModule = new MusicModule(Services)
            }).RunModule();

            Client.Ready += Client_Ready;

            await RegisterCommandsAsync();           

            await Client.LoginAsync(TokenType.Bot, TOKEN);

            await Client.StartAsync();            

            await Client.SetGameAsync("https://botbotya.ru", "https://discord.com/oauth2/authorize?client_id=749991391639109673&scope=bot&permissions=1573583991", activityType);                                  

            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {
            var instanceOfLavaNode = Services.GetRequiredService<LavaNode>();            
            if (!instanceOfLavaNode.IsConnected)
            {
                Console.WriteLine("Connecting Lava node..");                
                await instanceOfLavaNode.ConnectAsync();
                if (!instanceOfLavaNode.IsConnected)
                    Console.WriteLine("WARN Lava node connecting failed", Color.Red);
                else
                    Console.WriteLine("Lava node connected", Color.Green);
            }                        
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            int argsPos = 0;

            try
            {
                var message = arg as SocketUserMessage;
                var provider = new GuildProvider((message.Author as SocketGuildUser).Guild);
                var context = new SocketCommandContext(Client, message);                
                var serGuild = FilesProvider.GetGuild((message.Author as SocketGuildUser).Guild);

                if (message.Author.IsBot || message is null) return;                
                if (message.HasStringPrefix(serGuild.Prefix, ref argsPos))
                {                                            
                    IResult result = await Commands.ExecuteAsync(context, argsPos, Services);

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
                                await context.Channel.SendMessageAsync($"Неизвестная команда. У нас недавно прошла русификация команд. Поэтому пропиши команду {serGuild.Prefix}Справка");
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
                                await context.Channel.SendMessageAsync("В результате выполнения команды было сгенерировано исключение.");
                                break;
                            case CommandError.Unsuccessful:
                                await context.Channel.SendMessageAsync("Команда выполнена неудачно.");
                                break;
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
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);                                   
            Client.MessageReceived += HandleCommandAsync;
        }
    }
}
