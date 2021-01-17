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
|© Denis Voitenko                                                       |
_________________________________________________________________________
 */

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
    public static class FilesProvider
    {
        public static SerializableNewsAndPlans GetNewsAndPlans()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableNewsAndPlans));
            FileStream stream = new FileStream($@"{GetBotDirectoryPath()}/UpdateNewsAndPlans.xml", FileMode.Open);

            var newsAndPlans = (SerializableNewsAndPlans)serializer.Deserialize(stream);
            stream.Dispose();
            return newsAndPlans;
        }

        public static void ChangeNewsAndPlansToFalse()
        {
            var newsAndPlans = GetNewsAndPlans();
            newsAndPlans.ShouldSend = false;
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableNewsAndPlans));                        
            File.WriteAllText($@"{GetBotDirectoryPath()}/UpdateNewsAndPlans.xml", string.Empty);

            FileStream stream = new FileStream($@"{GetBotDirectoryPath()}/UpdateNewsAndPlans.xml", FileMode.Open);
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
            using FileStream stream = new FileStream($@"{GetBotDirectoryPath()}/BotGuilds/{guild.Id}.xml", FileMode.Open);
            return (SerializableGuild)serializer.Deserialize(stream);
        }        

        public static void DeleteGuild(ulong id)
        {
            File.Delete($@"{GetBotDirectoryPath()}/BotGuilds/{id}.xml");
        }

        public static void AddGuild(SocketGuild guild)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableGuild));
            SerializableGuild serializableGuild = new SerializableGuild
            {
                GuildId = guild.Id
            };            
            using (FileStream stream = new FileStream($@"{GetBotDirectoryPath()}/BotGuilds/{guild.Id}.xml", FileMode.Create))
                serializer.Serialize(stream, serializableGuild);
            Console.WriteLine($"Guild({guild.Id}) serialized.", Color.Green);
        }

        public static void RefreshGuild(SerializableGuild guild)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableGuild));

            File.WriteAllText($@"{GetBotDirectoryPath()}/BotGuilds/{guild.GuildId}.xml", string.Empty);
            using FileStream stream = new FileStream($@"{GetBotDirectoryPath()}/BotGuilds/{guild.GuildId}.xml", FileMode.Open, FileAccess.ReadWrite);
            serializer.Serialize(stream, guild);
        }
    }
}
