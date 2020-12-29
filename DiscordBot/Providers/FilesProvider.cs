using System.IO;
using System.Text.Json;
using Discord.WebSocket;
using System.Xml.Serialization;
using DiscordBot.Modules.FileManaging;
using System;
using System.Drawing;
using DiscordBot.Serializable;

namespace DiscordBot.FileWorking
{
    /// <summary>
    /// Класс, который отвечает за опеации с файлами бота.
    /// </summary>
    public static class FilesProvider
    {
        public static SerializableNewsAndPlans GetNewsAndPlans()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableNewsAndPlans));
            FileStream stream = new FileStream($@"{GetBotDirectoryPath()}\UpdateNewsAndPlans.xml", FileMode.Open);

            var newsAndPlans = (SerializableNewsAndPlans)serializer.Deserialize(stream);
            stream.Dispose();
            return newsAndPlans;
        }

        public static void ChangeNewsAndPlansToFalse()
        {
            var newsAndPlans = GetNewsAndPlans();
            newsAndPlans.ShouldSend = false;
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableNewsAndPlans));                        
            File.WriteAllText($@"{GetBotDirectoryPath()}\UpdateNewsAndPlans.xml", string.Empty);

            FileStream stream = new FileStream($@"{GetBotDirectoryPath()}\UpdateNewsAndPlans.xml", FileMode.Open);
            serializer.Serialize(stream, newsAndPlans);
            stream.Dispose();
        }

        public static string GetHelloText(SocketGuild guild) => GetGuild(guild).HelloMessage;

        public static string GetBotDirectoryPath()
        {
            using StreamReader reader = new StreamReader("config.json");
            return JsonSerializer.Deserialize<SerializableConfig>(reader.ReadToEnd()).Path;
        }        
                                
        public static SerializableGuild GetGuild(SocketGuild guild)
        {            
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableGuild));
            using FileStream stream = new FileStream($@"{GetBotDirectoryPath()}\BotGuilds\{guild.Id}.xml", FileMode.Open);
            return (SerializableGuild)serializer.Deserialize(stream);
        }        

        public static void DeleteGuild(ulong id)
        {
            File.Delete($@"{GetBotDirectoryPath()}\BotGuilds\{id}.xml");
        }

        public static void AddGuild(SocketGuild guild)
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
                HelloMessage = "Добро пожаловать на сервер.\nЕсли ты в первый раз на сервере и не знаешь что я умею, тогда напиши !help.",
                EmojiOfRoom = "🎤",
                Prefix = "!",
                GuildNotifications = false,
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
            using (FileStream stream = new FileStream($@"{GetBotDirectoryPath()}\BotGuilds\{guild.Id}.xml", FileMode.Create))
                serializer.Serialize(stream, serializableGuild);
            Console.WriteLine($"Guild({guild.Id}) serialized.", Color.Green);
        }

        public static void RefreshGuild(SerializableGuild guild)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableGuild));                        
            
            File.WriteAllText($@"{GetBotDirectoryPath()}\BotGuilds\{guild.GuildId}.xml", string.Empty);
            using FileStream stream = new FileStream($@"{GetBotDirectoryPath()}\BotGuilds\{guild.GuildId}.xml", FileMode.Open, FileAccess.ReadWrite);
            serializer.Serialize(stream, guild);
        }

        /// <summary>
        /// Безопасно обновляет файлы. Данная функция обязательно должна вызваться после обновления алгоритмов связанных с файлами серверов.
        /// </summary>
        public static void UpdateGuildFiles()
        {
            string path = @$"{GetBotDirectoryPath()}\BotGuilds";
            string[] guildsFiles = Directory.GetFiles(path);

            XmlSerializer serializer = new XmlSerializer(typeof(SerializableGuild));
            foreach (string guildFile in guildsFiles)
            {
                FileStream streamToOld = new FileStream(guildFile, FileMode.Open);
                var oldSerGuild = (SerializableGuild)serializer.Deserialize(streamToOld);
                streamToOld.Dispose();

                File.WriteAllText(guildFile, string.Empty);
                FileStream streamToNew = new FileStream(guildFile, FileMode.Open);
                serializer.Serialize(streamToNew, oldSerGuild);
                streamToNew.Dispose();
            }
        }
    }
}
