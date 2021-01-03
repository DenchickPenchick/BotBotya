using Discord;
using Discord.WebSocket;
using System.IO;
using System.Text.Json;
using System.Linq;
using Console = Colorful.Console;
using DiscordBot.Modules;
using System.Xml.Serialization;
using DiscordBot.Modules.FileManaging;
using System.Threading.Tasks;
using DiscordBot.FileWorking;
using DiscordBot.GuildManaging;
using System.Collections.Generic;
using DiscordBot.Serializable;
using System;
using System.Text;

namespace DiscordBot
{    
    public class FilesModule : IModule
    {
        public Bot Bot { get; set; }

        public FilesModule(Bot bot)
        {
            Bot = bot;
        }        

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
            if (!Directory.Exists($"{Bot.PathToBotDirectory}/BotGuilds"))
            {
                Console.WriteLine("BotGuilds not found", Color.Red);
                Directory.CreateDirectory($"{Bot.PathToBotDirectory}/BotGuilds");
                Console.WriteLine("BotGuilds created", Color.Green);
            }
            else
                Console.WriteLine("BotGuilds found", Color.Green);

            Console.WriteLine("Checking directory CustomCommandsDirectory...");
            if (!Directory.Exists($"{Bot.PathToBotDirectory}/CustomCommandsDirectory"))
            {
                Console.WriteLine("CustomCommandsDirectory not found", Color.Red);
                Directory.CreateDirectory($"{Bot.PathToBotDirectory}/CustomCommandsDirectory");
                Console.WriteLine("CustomCommandsDirectory created", Color.Green);
            }
            else
                Console.WriteLine("CustomCommandsDirectory found", Color.Green);

            Console.WriteLine("Checking files...", Color.Blue);
           
            Console.WriteLine("Checking file UpdateNewsDescription.xml...");
            if (!File.Exists($"{Bot.PathToBotDirectory}/UpdateNewsAndPlans.xml"))
            {
                Console.WriteLine("UpdateNewsAndPlans.xml not found", Color.Red);
                using (FileStream fileStream = new FileStream($"{Bot.PathToBotDirectory}/UpdateNewsAndPlans.xml", FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SerializableNewsAndPlans));
                    serializer.Serialize(fileStream, new SerializableNewsAndPlans
                    {
                        ShouldSend = false,
                        News = new List<string>(),
                        Plans = new List<string>()
                    });
                                        
                }
                Console.WriteLine("UpdateNewsAndPlans.xml created", Color.Green);
            }
            else
                Console.WriteLine("UpdateNewsAndPlans.xml found", Color.Green);

            using (FileStream fs = new FileStream($"{Bot.PathToBotDirectory}/Example.xml", FileMode.Create))
            {
                new XmlSerializer(typeof(CustomCommands.CustomCommand)).Serialize(fs, new CustomCommands.CustomCommand
                { 
                    Name = "Test",
                    Actions = new List<CustomCommands.CustomCommand.Action>() { CustomCommands.CustomCommand.Action.Message },
                    Message = "Hello",
                    GuildId = 777618262074458202
                });
            }
        }

        private async void SetupBotGuildData()
        {
            DiscordSocketClient client = Bot.Client;            

            Console.WriteLine("Checking guilds...");
            var guilds = client.Guilds;            
            string[] fileNames = Directory.GetFiles($"{Bot.PathToBotDirectory}/BotGuilds");
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
                GuildId = guild.Id,
                DefaultRoleId = 0,
                HelloMessageEnable = false,
                RoomsEnable = false,
                ContentEnable = false,
                CheckingContent = false,
                HelloMessage = "Добро пожаловать на сервер.\nЕсли ты в первый раз на сервере и не знаешь что я умею, тогда напиши команду \"справка\".",
                SystemCategories = new SerializableCategories
                {
                    MainTextCategoryName = "💬Текстовые каналы",
                    ContentCategoryName = "⚡Контент",
                    MainVoiceCategoryName = "🎤Голосовые каналы",
                    VoiceRoomsCategoryName = "🏠Комнаты",
                    BotCategoryName = "🤖Бот"
                },
                SystemChannels = new SerializableChannels
                { 
                    LinksChannelName = "🌐ссылки",
                    VideosChannelName = "📹видео",
                    CreateRoomChannelName = "➕Создать комнату",
                    ConsoleChannelName = "🤖консоль-бота"                    
                }
            };
            using (FileStream stream = new FileStream($"{Bot.PathToBotDirectory}/BotGuilds/{guild.Id}.xml", FileMode.Create))
                serializer.Serialize(stream, serializableGuild);         
            Console.WriteLine($"Guild({guild.Id}) serialized.", Color.Green);
        }

        public void DeleteGuild(ulong id)
        {            
            File.Delete($"{FilesProvider.GetBotDirectoryPath()}/BotGuilds/{id}.xml");
        }
    }
}
