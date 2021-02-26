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

using System;
using System.Reflection;
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
using Victoria;
using DiscordBot.MusicOperations;
using DiscordBot.Modules.MusicManaging;
using System.Runtime;
using DiscordBot.Providers;
using DiscordBot.Modules.ServersConnectingManaging;
using DiscordBot.Providers.FileManaging;
using System.Threading;
using DiscordBot.Modules.EconomicManaging;
using DiscordBot.TypeReaders;
using System.Linq;
using DiscordBot.TextReaders;
using System.Collections.Generic;

namespace DiscordBot
{
    public class Bot
    {        
        public string TOKEN = null;
        public string PathToBotDirectory = null;
        
        public DiscordSocketClient Client;
        public CommandService Commands;        
        public IServiceProvider Services;        

        public async Task RunBotAsync()
        {
            var assembly = Assembly.GetEntryAssembly();
            Console.WriteAscii("Discord Bot Console", Color.Blue);
            Console.WriteLine($"{assembly.GetCustomAttribute<AssemblyProductAttribute>().Product}\nVersion: {assembly.GetName().Version}\nGitHub: {assembly.GetCustomAttribute<AssemblyMetadataAttribute>().Value}\n{assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}\n", Color.Orange);

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 1000
            });

            Commands = new CommandService();            
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
                ContentModule = new ContentModule(Client, Commands),
                FileModule = new FilesModule(this),
                GuildModule = new GuildModule(Client),
                NotificationsModule = new LogModule(Client),
                MusicModule = new MusicModule(Services),
                ServersConnector = new ServersConnector(Client),
                EconomicModule = new EconomicModule(Client)
            }).RunModule();

            Client.Ready += Client_Ready;            

            await RegisterCommandsAsync();           

            await Client.LoginAsync(TokenType.Bot, TOKEN);

            await Client.StartAsync();
                                           
            UpdateStatus();
            
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
                var roles = (context.User as SocketGuildUser).Roles.Select(x => x.Id).ToList();

                if (message.Author.IsBot || 
                    message is null || 
                    (!serGuild.CommandsChannels.Contains(arg.Channel.Id) && serGuild.CommandsChannels.Count > 0) ||
                    roles.Exists(x => serGuild.IgnoreRoles.Contains(x))) return;                
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
                                if (serGuild.UnknownCommandMessage)
                                {
                                    string messCommand = arg.Content.Split(' ').First().Remove(0, serGuild.Prefix.Length);
                                    string predicateCommand = null;
                                    string parameters = null;
                                    List<(string, int, string)> distances = new List<(string, int, string)>();
                                    int min = 0;
                                    int index = 0;

                                    foreach (var command in Commands.Commands)
                                    {                                        
                                        string p = null;
                                        foreach (var param in command.Parameters)
                                            p += $" `{param.Name}`";
                                        distances.Add((command.Name, Filter.Distance(messCommand, command.Name), p));
                                        foreach (var alias in command.Aliases)                                        
                                            distances.Add((alias, Filter.Distance(messCommand, alias), p));
                                    }

                                    for (int i = 0; i < distances.Count; i++)                                    
                                        if ((distances[i].Item2 < min && min > 0) || min == 0)
                                        {
                                            min = distances[i].Item2;
                                            index = i;
                                        }                                    

                                    predicateCommand = distances[index].Item1;
                                    parameters = distances[index].Item3;

                                    await context.Channel.SendMessageAsync($"Неизвестная команда. Пропиши команду `{serGuild.Prefix}Хелп`.{(predicateCommand != null && predicateCommand != "Хелп"? $"\nМожет быть ты это имел ввиду команду `{serGuild.Prefix}{predicateCommand}`{parameters}?" : null)}");
                                }                                
                                break;
                            case CommandError.ParseFailed:
                                await context.Channel.SendMessageAsync("Наверное ты неправильно ввел данные.");
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
            Commands.AddTypeReader<Emoji>(new EmojiTypeReader());            
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);                                   
            Client.MessageReceived += HandleCommandAsync;
        }

        private void UpdateStatus()
        {
            new Thread(async x =>
            {
                int index = 0;
                while (true)
                {
                    try
                    {
                        switch (index)
                        {
                            case 0:
                                await Client.SetGameAsync("https://botbotya.ru", "https://discord.com/oauth2/authorize?client_id=749991391639109673&scope=bot&permissions=1573583991", ActivityType.CustomStatus);
                                index++;
                                break;
                            case 1:
                                await Client.SetGameAsync($"{Client.Guilds.Count} серверах", "https://discord.com/oauth2/authorize?client_id=749991391639109673&scope=bot&permissions=1573583991", ActivityType.CustomStatus);
                                index = 0;
                                break;
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(30));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ex: {ex}");
                    }
                }
            }).Start();            
        }
    }
}
