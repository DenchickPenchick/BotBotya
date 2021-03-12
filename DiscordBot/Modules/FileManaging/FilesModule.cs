//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
using Discord.WebSocket;
using System.IO;
using System.Text.Json;
using System.Linq;
using Console = Colorful.Console;
using DiscordBot.Modules;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using DiscordBot.Serializable;
using System;
using System.Text;

namespace DiscordBot.Providers.FileManaging
{
    /// <summary>
    /// Один из ключевых модулей, который отвечает за сохранность структуры данных бота.
    /// </summary>
    public class FilesModule : IModule
    {
        private Bot Bot { get;  }

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
            Console.WriteLine("Configuring bot started");

            if (!File.Exists("config.json"))
                using (FileStream fs = new FileStream("config.json", FileMode.Create))
                {
                    Console.WriteLine("Config not found. Creating config.json...", Color.Red);
                    fs.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new SerializableConfig())));
                    Console.WriteLine("Config created. App will be shoot down. You should set token and path to system category.", Color.Green);
                    fs.Close();
                    Environment.Exit(0);
                }
            using (StreamReader reader = new StreamReader("config.json"))
            {
                SerializableConfig config = JsonSerializer.Deserialize<SerializableConfig>(reader.ReadToEnd());
                Bot.TOKEN = config.Token;
                Bot.PathToBotDirectory = config.Path;
            }            
            Console.WriteLine("Configuring bot ended succesfully", Color.Green);
        }
      
        private void SetupBotDirectory()
        {            
            Console.WriteLine("Checking directories...", Color.Blue);
            
            Console.WriteLine("Checking directory BotDirectory...");
            if (!Directory.Exists(Bot.PathToBotDirectory))
            {
                Console.WriteLine("BotDirectory not found", Color.Red);                
                Directory.CreateDirectory(Bot.PathToBotDirectory);
                Console.WriteLine("BotDirectory created", Color.Green);
            }
            else            
                Console.WriteLine("BotDirectory found", Color.Green);
            
            Console.WriteLine("Checking directory BotGuilds...");
            if (!Directory.Exists($@"{Bot.PathToBotDirectory}/BotGuilds"))
            {
                Console.WriteLine("BotGuilds not found", Color.Red);
                Directory.CreateDirectory($@"{Bot.PathToBotDirectory}/BotGuilds");
                Console.WriteLine("BotGuilds created", Color.Green);
            }
            else
                Console.WriteLine("BotGuilds found", Color.Green);           

            Console.WriteLine("Checking directory ServerConnectorsHandlers...");
            if (!Directory.Exists(@$"{Bot.PathToBotDirectory}/ServerConnectorsHandlers"))
            {
                Console.WriteLine("ServerConnectorsHandlers not found", Color.Red);
                Directory.CreateDirectory($@"{Bot.PathToBotDirectory}/ServerConnectorsHandlers");
                Console.WriteLine("ServerConnectorsHandlers created", Color.Green);
            }
            else
                Console.WriteLine("ServerConnectorsHandlers found", Color.Green);

            Console.WriteLine("Checking directory EconomicGuilds...");
            if (!Directory.Exists(@$"{Bot.PathToBotDirectory}/EconomicGuilds"))
            {
                Console.WriteLine("ServerConnectorsHandlers not found", Color.Red);
                Directory.CreateDirectory($@"{Bot.PathToBotDirectory}/EconomicGuilds");
                Console.WriteLine("EconomicGuilds created", Color.Green);
            }
            else
                Console.WriteLine("EconomicGuilds found", Color.Green);

            Console.WriteLine("Checking files...");

            if (!File.Exists($"{Bot.PathToBotDirectory}/ReactRoleMessages.xml"))
            {
                Console.WriteLine("ReactRoleMessages.xml not found", Color.Red);
                using (FileStream stream = new FileStream($"{Bot.PathToBotDirectory}/ReactRoleMessages.xml", FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(SerializableReactRoleMessages));

                    serializer.Serialize(stream, new SerializableReactRoleMessages
                    {
                        ReactRoleMessages = new List<SerializableReactRoleMessage>()
                    });
                }
                Console.WriteLine("ReactRoleMessages.xml created", Color.Green);
            }
            else
                Console.WriteLine("ReactRoleMessages.xml found", Color.Green);

            if (!File.Exists($"{Bot.PathToBotDirectory}/GlobalOptions.xml"))
            {
                Console.WriteLine("GlobalOptions.xml not found", Color.Red);
                using (FileStream stream = new FileStream($"{Bot.PathToBotDirectory}/GlobalOptions.xml", FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(SerializableGlobalOptions));

                    serializer.Serialize(stream, new SerializableGlobalOptions());
                }
                Console.WriteLine("GlobalOptions.xml created", Color.Green);
            }
            else
                Console.WriteLine("ReactRoleMessages.xml found", Color.Green);
            Console.WriteLine("Files checked");
        }

        private async void SetupBotGuildData()
        {
            DiscordSocketClient client = Bot.Client;            

            Console.WriteLine("Checking guilds...");
            var guilds = client.Guilds;            
            string[] fileNames = Directory.GetFiles($@"{Bot.PathToBotDirectory}/BotGuilds");
            string[] fileNamesWithoutPath = new string[fileNames.Length];
            ulong[] GuildsId = new ulong[guilds.Count];

            for (int i = 0; i < fileNames.Length; i++)            
                fileNamesWithoutPath[i] = Path.GetFileNameWithoutExtension(fileNames[i]);

            for (int i = 0; i < GuildsId.Length; i++)
                GuildsId[i] = guilds.ToArray()[i].Id;

            foreach (SocketGuild guild in guilds)                            
                if (!fileNamesWithoutPath.Contains(guild.Id.ToString()))
                {
                    Console.WriteLine($"Guild({guild.Id}) not found. Serializing...");
                    FilesProvider.AddGuild(guild);
                    await new GuildProvider(guild).SendHelloMessageToGuild(client);                    
                }
            foreach (string fileName in fileNamesWithoutPath)
                if (!GuildsId.Contains(ulong.Parse(fileName)))
                    FilesProvider.DeleteGuild(ulong.Parse(fileName));

            Console.WriteLine("Guilds checked", Color.Green);
        }        

        private void AddGuild(SocketGuild guild)
        {            
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableGuild));
            SerializableGuild serializableGuild = new SerializableGuild 
            {
                GuildId = guild.Id
            };            
            using (FileStream stream = new FileStream($@"{Bot.PathToBotDirectory}/BotGuilds/{guild.Id}.xml", FileMode.Create))
                serializer.Serialize(stream, serializableGuild);         
            Console.WriteLine($"Guild({guild.Id}) serialized.", Color.Green);
        }
        
        private void DeleteGuild(ulong id)
        {            
            File.Delete($@"{FilesProvider.GetBotDirectoryPath()}/BotGuilds/{id}.xml");
        }
    }    
}
