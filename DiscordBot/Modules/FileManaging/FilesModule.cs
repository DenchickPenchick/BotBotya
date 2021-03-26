//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Serializable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DiscordBot.Providers.FileManaging
{
    /// <summary>
    /// Один из ключевых модулей, который отвечает за сохранность структуры данных бота.
    /// </summary>
    public class FilesModule : IModule
    {
        private Bot Bot { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FilesModule"/>.
        /// </summary>
        /// <param name="bot"></param>
        public FilesModule(Bot bot)
        {
            Bot = bot;
        }

        /// <summary>
        /// Запускает модуль.
        /// </summary>
        public void RunModule()
        {
            ConfigureBot();
            SetupBotDirectory();
            Bot.Client.Ready += Client_Ready;
            Bot.Client.JoinedGuild += Client_JoinedGuild;
            Bot.Client.LeftGuild += Client_LeftGuild;
        }

        private Task Client_LeftGuild(SocketGuild arg)
        {
            DeleteGuild(arg.Id);
            return Task.CompletedTask;
        }

        private Task Client_JoinedGuild(SocketGuild arg)
        {
            AddGuild(arg);
            return Task.CompletedTask;
        }

        private Task Client_Ready()
        {
            SetupBotGuildData();            
            return Task.CompletedTask;
        }

        private void ConfigureBot()
        {            
            if (!File.Exists("config.json"))
                using (FileStream fs = new FileStream("config.json", FileMode.Create))
                {
                    LogsProvider.ErrorLog(new Error
                    { 
                        Description = "Config not found.",
                        OccuredIn = "FilesModule.cs"
                    });                    
                    fs.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new SerializableConfig())));

                    LogsProvider.Log("Config created. App will be shoot down. You should set token and path to system category.");
                    fs.Close();
                    Environment.Exit(0);
                }
            LogsProvider.Log("Configuring bot ended succesfully");
        }

        private void SetupBotDirectory()
        {
            string basePath = FilesProvider.GetConfig().Path;

            var logStream = LogsProvider.Log("Checking enviroment started", false);                        

            logStream.WriteLine("Checking directories");            
            
            if (!Directory.Exists(basePath))
            {
                logStream.WriteLine("BotDirectory not found");
                Directory.CreateDirectory(basePath);
                logStream.WriteLine("BotDirectory created");
            }            
            
            if (!Directory.Exists($@"{basePath}/BotGuilds"))
            {
                logStream.WriteLine("BotGuilds not found");
                Directory.CreateDirectory($@"{basePath}/BotGuilds");
                logStream.WriteLine("BotGuilds created");
            }            
            
            if (!Directory.Exists(@$"{basePath}/ServerConnectorsHandlers"))
            {
                logStream.WriteLine("ServerConnectorsHandlers not found");
                Directory.CreateDirectory($@"{basePath}/ServerConnectorsHandlers");
                logStream.WriteLine("ServerConnectorsHandlers created");
            }            

            if (!Directory.Exists(@$"{basePath}/EconomicGuilds"))
            {
                logStream.WriteLine("ServerConnectorsHandlers not found");
                Directory.CreateDirectory($@"{basePath}/EconomicGuilds");
                logStream.WriteLine("EconomicGuilds created");
            }

            logStream.WriteLine("Directories checked");
            logStream.WriteLine("Checking files");

            if (!File.Exists($"{basePath}/ReactRoleMessages.xml"))
            {
                logStream.WriteLine("ReactRoleMessages.xml not found");
                using (FileStream stream = new($"{basePath}/ReactRoleMessages.xml", FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(SerializableReactRoleMessages));

                    serializer.Serialize(stream, new SerializableReactRoleMessages
                    {
                        ReactRoleMessages = new List<SerializableReactRoleMessage>()
                    });
                }
                logStream.WriteLine("ReactRoleMessages.xml created");
            }            

            if (!File.Exists($"{basePath}/GlobalOptions.xml"))
            {
                logStream.WriteLine("GlobalOptions.xml not found");
                using (FileStream stream = new($"{basePath}/GlobalOptions.xml", FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(SerializableGlobalOptions));

                    serializer.Serialize(stream, new SerializableGlobalOptions());
                }
                logStream.WriteLine("GlobalOptions.xml created");
            }
            logStream.WriteLine("Files checked");
            logStream.EndStream();
        }

        private async void SetupBotGuildData()
        {
            var logStream = LogsProvider.Log("Guild checking started", false);

            string basePath = FilesProvider.GetConfig().Path;
            DiscordSocketClient client = Bot.Client;
            
            var guilds = client.Guilds;
            string[] fileNames = Directory.GetFiles($@"{basePath}/BotGuilds");
            string[] fileNamesWithoutPath = new string[fileNames.Length];
            ulong[] GuildsId = new ulong[guilds.Count];

            for (int i = 0; i < fileNames.Length; i++)
                fileNamesWithoutPath[i] = Path.GetFileNameWithoutExtension(fileNames[i]);

            for (int i = 0; i < GuildsId.Length; i++)
                GuildsId[i] = guilds.ToArray()[i].Id;

            foreach (SocketGuild guild in guilds)
            { 
                if (!fileNamesWithoutPath.Contains(guild.Id.ToString()))
                {
                    logStream.WriteLine($"Guild({guild.Id}) not found. Serializing...");
                    FilesProvider.AddGuild(guild);
                    await new GuildProvider(guild).SendHelloMessageToGuild(client);
                }            
            }
            foreach (string fileName in fileNamesWithoutPath)
                if (!GuildsId.Contains(ulong.Parse(fileName)))
                    FilesProvider.DeleteGuild(ulong.Parse(fileName));

            logStream.WriteLine("Guilds checked");
            logStream.EndStream();
        }        

        private void AddGuild(SocketGuild guild)
        {
            string basePath = FilesProvider.GetConfig().Path;
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableGuild));
            SerializableGuild serializableGuild = new SerializableGuild
            {
                GuildId = guild.Id
            };
            using FileStream stream = new FileStream($@"{basePath}/BotGuilds/{guild.Id}.xml", FileMode.Create);
            serializer.Serialize(stream, serializableGuild);
        }

        private void DeleteGuild(ulong id)
        {
            File.Delete($@"{FilesProvider.GetBotDirectoryPath()}/BotGuilds/{id}.xml");
        }
    }
}
