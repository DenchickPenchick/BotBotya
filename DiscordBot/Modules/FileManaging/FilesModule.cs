//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
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
using Console = Colorful.Console;

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
            ReserializeAllFiles();
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
            Console.WriteLine("Configuring bot ended succesfully", Color.Green);
        }

        private void SetupBotDirectory()
        {
            string basePath = FilesProvider.GetConfig().Path;

            Console.WriteLine("Checking directories...", Color.Blue);

            Console.WriteLine("Checking directory BotDirectory...");
            if (!Directory.Exists(basePath))
            {
                Console.WriteLine("BotDirectory not found", Color.Red);
                Directory.CreateDirectory(basePath);
                Console.WriteLine("BotDirectory created", Color.Green);
            }
            else
                Console.WriteLine("BotDirectory found", Color.Green);

            Console.WriteLine("Checking directory BotGuilds...");
            if (!Directory.Exists($@"{basePath}/BotGuilds"))
            {
                Console.WriteLine("BotGuilds not found", Color.Red);
                Directory.CreateDirectory($@"{basePath}/BotGuilds");
                Console.WriteLine("BotGuilds created", Color.Green);
            }
            else
                Console.WriteLine("BotGuilds found", Color.Green);

            Console.WriteLine("Checking directory ServerConnectorsHandlers...");
            if (!Directory.Exists(@$"{basePath}/ServerConnectorsHandlers"))
            {
                Console.WriteLine("ServerConnectorsHandlers not found", Color.Red);
                Directory.CreateDirectory($@"{basePath}/ServerConnectorsHandlers");
                Console.WriteLine("ServerConnectorsHandlers created", Color.Green);
            }
            else
                Console.WriteLine("ServerConnectorsHandlers found", Color.Green);

            Console.WriteLine("Checking directory EconomicGuilds...");
            if (!Directory.Exists(@$"{basePath}/EconomicGuilds"))
            {
                Console.WriteLine("ServerConnectorsHandlers not found", Color.Red);
                Directory.CreateDirectory($@"{basePath}/EconomicGuilds");
                Console.WriteLine("EconomicGuilds created", Color.Green);
            }
            else
                Console.WriteLine("EconomicGuilds found", Color.Green);

            Console.WriteLine("Checking files...");

            if (!File.Exists($"{basePath}/ReactRoleMessages.xml"))
            {
                Console.WriteLine("ReactRoleMessages.xml not found", Color.Red);
                using (FileStream stream = new($"{basePath}/ReactRoleMessages.xml", FileMode.Create))
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

            if (!File.Exists($"{basePath}/GlobalOptions.xml"))
            {
                Console.WriteLine("GlobalOptions.xml not found", Color.Red);
                using (FileStream stream = new($"{basePath}/GlobalOptions.xml", FileMode.Create))
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
            string basePath = FilesProvider.GetConfig().Path;
            DiscordSocketClient client = Bot.Client;

            Console.WriteLine("Checking guilds...");
            var guilds = client.Guilds;
            string[] fileNames = Directory.GetFiles($@"{basePath}/BotGuilds");
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

        //Должен вызываться в начале работы программы
        //Обновляет версии файлов конфигурации сервера до новейшей.
        private void ReserializeAllFiles()
        {
            string[] allGuildsFiles = Directory.GetFiles(FilesProvider.GetConfig().Path + "/BotGuilds", "*.xml");
            
            var newSerializer = new XmlSerializer(typeof(SerializableGuild));

            foreach (string path in allGuildsFiles)
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    if (IsDeserializable(fs, out ObsoleteSerializableGuild guild))
                    {
                        var instanceOfNewGuild = new SerializableGuild
                        {
                            GuildId = guild.GuildId,
                            DefaultRoleId = guild.DefaultRoleId,
                            MaxWarns = guild.MaxWarns,
                            LotsCount = guild.LotsCount,
                            KickForWarns = guild.KickForWarns,
                            BanForWarns = guild.BanForWarns,
                            MuteForWarns = guild.MuteForWarns,
                            WarnsForBadWords = guild.WarnsForBadWords,
                            WarnsForInviteLink = guild.WarnsForInviteLink,
                            HelloMessageEnable = guild.HelloMessageEnable,
                            CheckingContent = guild.CheckingContent,
                            UnknownCommandMessage = guild.UnknownCommandMessage,
                            CheckingBadWords = guild.CheckingBadWords,
                            CreateTextChannelsForVoiceChannels = guild.CreateTextChannelsForVoiceChannels,
                            AdvertisingAccepted = guild.AdvertisingAccepted,
                            AdvertisingModerationSended = guild.AdvertisingModerationSended,
                            HelloMessage = guild.HelloMessage,
                            EmojiOfRoom = guild.EmojiOfRoom,
                            Prefix = guild.Prefix,
                            EmbedColor = ColorProvider.GetColorFromName(guild.EmbedColor),
                            CommandsChannels = guild.CommandsChannels,
                            IgnoreRoles = guild.IgnoreRoles,
                            BlaskListedRolesToSale = guild.BlaskListedRolesToSale,
                            BadWords = guild.BadWords,
                            ExceptWords = guild.ExceptWords,
                            BadUsers = guild.BadUsers,
                            SystemCategories = guild.SystemCategories,
                            SystemChannels = guild.SystemChannels,
                            Advert = guild.Advert,
                            NextCheck = guild.NextCheck,
                            NextSend = guild.NextSend
                        };                                                

                        fs.Close();

                        File.WriteAllText(path, string.Empty);

                        using (FileStream fileStr = new FileStream(path, FileMode.Open))                        
                        newSerializer.Serialize(fileStr, instanceOfNewGuild);                                                
                    }
                }
            }
        }

        private bool IsDeserializable(Stream stream, out ObsoleteSerializableGuild guild)
        {
            var ser = new XmlSerializer(typeof(ObsoleteSerializableGuild), new XmlRootAttribute("SerializableGuild"));

            try
            {
                guild = (ObsoleteSerializableGuild)ser.Deserialize(stream);
                return true;
            }
            catch (Exception)
            {
                guild = null;
                return false;
            }
        }

        private void AddGuild(SocketGuild guild)
        {
            string basePath = FilesProvider.GetConfig().Path;
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableGuild));
            SerializableGuild serializableGuild = new SerializableGuild
            {
                GuildId = guild.Id
            };
            using (FileStream stream = new FileStream($@"{basePath}/BotGuilds/{guild.Id}.xml", FileMode.Create))
                serializer.Serialize(stream, serializableGuild);
            Console.WriteLine($"Guild({guild.Id}) serialized.", Color.Green);
        }

        private void DeleteGuild(ulong id)
        {
            File.Delete($@"{FilesProvider.GetBotDirectoryPath()}/BotGuilds/{id}.xml");
        }
    }
}
