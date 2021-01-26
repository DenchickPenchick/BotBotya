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

using System.IO;
using System.Text.Json;
using Discord.WebSocket;
using System.Xml.Serialization;
using DiscordBot.Modules.FileManaging;
using System;
using DiscordBot.Serializable;
using System.Collections.Generic;
using Discord;

namespace DiscordBot.Providers
{   
    public static class FilesProvider
    {        
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

        public static SerializableConnector GetConnector(ulong id)
        {
            string pathToDir = $"{GetBotDirectoryPath()}/ServerConnectorsHandlers";
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableConnectors));

            foreach (string file in Directory.GetFiles(pathToDir))
            {
                using FileStream fs = new FileStream(file, FileMode.Open);
                var serConn = (SerializableConnectors)serializer.Deserialize(fs);
                List<ulong> hostsId = new List<ulong>();

                foreach (var serConnector in serConn.SerializableConnectorsChannels)
                    hostsId.Add(serConnector.HostId);

                if (hostsId.Contains(id))
                    return serConn.SerializableConnectorsChannels[hostsId.IndexOf(id)];
            }
            return null;
        }

        public static SerializableConnectors GetConnectors(SocketGuild guild)
        {            
            string path = $"{GetBotDirectoryPath()}/ServerConnectorsHandlers/{guild.Id}.xml";
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableConnectors));

            if (!File.Exists(path))
                return null;

            using FileStream fs = new FileStream(path, FileMode.Open);
            var serConn = (SerializableConnectors)serializer.Deserialize(fs);
            return serConn;
        }

        public static void AddConnector(SocketGuild guild, SerializableConnector connector)
        {            
            var conn = GetConnectors(guild);

            if (conn == null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializableConnectors));
                string pathToNewFile = $"{GetBotDirectoryPath()}/ServerConnectorsHandlers/{guild.Id}.xml";

                conn = new SerializableConnectors
                {
                    GuildId = guild.Id,
                    SerializableConnectorsChannels = new List<SerializableConnector>()
                };

                using FileStream fs = new FileStream(pathToNewFile, FileMode.Create);
                serializer.Serialize(fs, conn);
            }

            if (!conn.SerializableConnectorsChannels.Contains(connector))
            {
                List<ulong> hostsId = new List<ulong>();

                foreach (var connect in conn.SerializableConnectorsChannels)                
                    hostsId.Add(connect.HostId);

                if (hostsId.Contains(connector.HostId))
                {
                    var toDelete = conn.SerializableConnectorsChannels[hostsId.IndexOf(connector.HostId)];
                    conn.SerializableConnectorsChannels.Remove(toDelete);
                }

                conn.SerializableConnectorsChannels.Add(connector);
                RefreshConnectors(conn);
            }            
        }        

        public static void RefreshConnectors(SerializableConnectors connectors)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableConnectors));

            File.WriteAllText($"{GetBotDirectoryPath()}/ServerConnectorsHandlers/{connectors.GuildId}.xml", string.Empty);
            using FileStream stream = new FileStream($"{GetBotDirectoryPath()}/ServerConnectorsHandlers/{connectors.GuildId}.xml", FileMode.Open, FileAccess.ReadWrite);
            serializer.Serialize(stream, connectors);
        }

        public static SerializableEconomicGuild GetEconomicGuild(IGuild guild)
        {
            string path = $"{GetBotDirectoryPath()}/EconomicGuilds/{guild.Id}.xml";
            if (File.Exists(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializableEconomicGuild));
                using FileStream fs = new FileStream(path, FileMode.Open);
                return (SerializableEconomicGuild)serializer.Deserialize(fs);
            }
            else
                return null;
        }

        public static SerializableEconomicGuildUser GetEconomicGuildUser(IGuildUser user)
        {
            var guild = GetEconomicGuild(user.Guild);

            foreach (var economUser in guild.SerializableEconomicUsers)            
                if (economUser.Id == user.Id)
                    return economUser;
            return null;
        }

        public static void RefreshEconomicGuild(SerializableEconomicGuild guild)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableConnectors));

            File.WriteAllText($"{GetBotDirectoryPath()}/EconomicGuilds/{guild.Id}.xml", string.Empty);
            using FileStream stream = new FileStream($"{GetBotDirectoryPath()}/EconomicGuilds/{guild.Id}.xml", FileMode.Open, FileAccess.ReadWrite);
            serializer.Serialize(stream, guild);
        }
    }
}