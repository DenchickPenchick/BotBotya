﻿//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Collections;
using DiscordBot.GuildManaging;
using DiscordBot.Interactivities;
using DiscordBot.Modules.AdvertisingManaging;
using DiscordBot.Modules.ContentManaging;
using DiscordBot.Modules.EconomicManaging;
using DiscordBot.Modules.MusicManaging;
using DiscordBot.Modules.NotificationsManaging;
using DiscordBot.Modules.ProcessManage;
using DiscordBot.Modules.ServersConnectingManaging;
using DiscordBot.MusicOperations;
using DiscordBot.Providers;
using DiscordBot.Providers.FileManaging;
using DiscordBot.RoomManaging;
using DiscordBot.TextReaders;
using DiscordBot.TypeReaders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Console = Colorful.Console;

namespace DiscordBot
{
    public class Bot
    {            
        public DiscordSocketClient Client { get; set; }
        public CommandService Commands { get; private set; }
        public IServiceProvider Services { get; set; }

        public async Task RunBotAsync()
        {
            var assembly = Assembly.GetEntryAssembly();
            Console.WriteAscii("Discord Bot Console", Color.Blue);            

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 1024
            });

            Commands = new CommandService();
            InteractiveService interactiveService = new(Client, new InteractiveServiceConfig
            {
                DefaultTimeout = TimeSpan.FromMinutes(5)
            });

            Client.Ready += Client_Ready;

            Services = new ServiceCollection()                
                .AddSingleton(this)
                .AddSingleton(Client)                                
                .AddSingleton<InteractiveService>()                
                .AddSingleton(Commands)
                .AddSingleton(new PaginatingService(Client, maxMessages: 200))
                .AddSingleton(new LavaNode(Client, new LavaConfig
                {
                    ResumeTimeout = TimeSpan.MaxValue
                }))
                .AddSingleton(new LavaConfig())                
                .AddSingleton<LavaOperations>()
                .BuildServiceProvider();

            var modulesForLogs = new ServiceCollection()
                .AddSingleton(new VoiceChannelsModule(Client))
                .AddSingleton(new ContentModule(Client, Commands))
                .BuildServiceProvider();

            var modulesCollection = new ModulesCollection()
                .AddModule(new FilesModule(this))
                .AddModule(new GuildModule(Client))                
                .AddModule(new AdvertisingModule(Client))
                .AddModule(modulesForLogs.GetRequiredService<VoiceChannelsModule>())
                .AddModule(modulesForLogs.GetRequiredService<ContentModule>())
                .AddModule(new LogModule(Client, modulesForLogs))
                .AddModule(new ServersConnector(Client))
                .AddModule(new EconomicModule(Client))
                .AddModule(new MusicModule(Services));

            new ProcessingModule(modulesCollection).RunModule();

            var config = FilesProvider.GetConfig();

            await RegisterCommandsAsync();

            await Client.LoginAsync(TokenType.Bot, config.Token);

            await Client.StartAsync();

            UpdateStatus();

            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {            
            var instanceOfLavaNode = Services.GetRequiredService<LavaNode>();
            if (!instanceOfLavaNode.IsConnected)
            {
                LogsProvider.Log("Connecting Lava node...");
                await instanceOfLavaNode.ConnectAsync();
                if (!instanceOfLavaNode.IsConnected)
                {
                    LogsProvider.ErrorLog(new Error
                    {
                        Description = "Lava node connecting failed. I'll try reconnect in 30 seconds.",
                        OccuredIn = "Bot.cs"
                    });

                    new Thread(async () =>
                    {
                        while (!instanceOfLavaNode.IsConnected)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(30));
                            await instanceOfLavaNode.ConnectAsync();

                            LogsProvider.ErrorLog(new Error
                            {
                                Description = "Lava node connecting failed. I'll try reconnect in 30 seconds.",
                                OccuredIn = "Bot.cs"
                            });
                        }                        
                    }).Start();
                }
                    
                else
                    LogsProvider.Log("Lava node connected");
            }
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            int argsPos = 0;

            try
            {
                if (arg is SocketUserMessage message)
                    if (message.Author is SocketGuildUser)
                    {
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
                                    var str = LogsProvider.ErrorLog(new Error
                                    {
                                        Description = $"Command failed.",
                                        OccuredIn = "Bot.cs"
                                    }, false);
                                    str.WriteLine($"Content: {message.Content}");
                                    str.WriteLine($"Command from: {context.User.Username}");
                                    str.WriteLine($"Reason: {result.ErrorReason}");
                                    str.EndStream();
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

                                            await context.Channel.SendMessageAsync($"Неизвестная команда. Пропиши команду `{serGuild.Prefix}Хелп`.{(predicateCommand != null && predicateCommand != "Хелп" ? $"\nМожет быть ты это имел ввиду команду `{serGuild.Prefix}{predicateCommand}`{parameters}?" : null)}");
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
            }
            catch (NullReferenceException)
            {

            }
            catch (Exception ex)
            {
                LogsProvider.ExceptionLog(ex);
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
                        LogsProvider.ExceptionLog(ex);
                    }
                }
            }).Start();
        }
    }
}
