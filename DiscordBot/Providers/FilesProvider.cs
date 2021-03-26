//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
using Discord.WebSocket;
using DiscordBot.Serializable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;

namespace DiscordBot.Providers
{
    public static class FilesProvider
    {
        public static string GetHelloText(IGuild guild) => GetGuild(guild).HelloMessage;

        public static string GetBotDirectoryPath()
        {
            using StreamReader reader = new("config.json");
            return JsonSerializer.Deserialize<SerializableConfig>(reader.ReadToEnd()).Path;
        }

        public static SerializableGuild GetGuild(IGuild guild)
        {
            try
            { 
                return GetGuild(guild.Id);            
            }
            catch (IOException)
            {
                AddGuild(guild);
                return GetGuild(guild);
            }
        } 

        public static SerializableGuild GetGuild(ulong guildId)
        {            
            XmlSerializer serializer = new(typeof(SerializableGuild));

            using FileStream stream = new($@"{GetBotDirectoryPath()}/BotGuilds/{guildId}.xml", FileMode.Open);
            return (SerializableGuild)serializer.Deserialize(stream);
        }

        public static SerializableConfig GetConfig()
        {
            using StreamReader reader = new("config.json");
            return JsonSerializer.Deserialize<SerializableConfig>(reader.ReadToEnd());
        }

        public static IEnumerable<SerializableGuild> GetAllGuilds()
        {
            XmlSerializer serializer = new(typeof(SerializableGuild));
            string directoryWithGuilds = $"{GetBotDirectoryPath()}/BotGuilds";
            string[] files = Directory.GetFiles(directoryWithGuilds);

            foreach (string file in files)
            {
                using FileStream stream = new(file, FileMode.Open);
                yield return (SerializableGuild)serializer.Deserialize(stream);
            }
        }

        public static SerializableGlobalOptions GetGlobalOptions()
        {
            XmlSerializer serializer = new(typeof(SerializableGlobalOptions));
            string pathToXML = $"{GetBotDirectoryPath()}/GlobalOptions.xml";

            using FileStream stream = new(pathToXML, FileMode.Open);
            return (SerializableGlobalOptions)serializer.Deserialize(stream);
        }

        public static void DeleteGuild(ulong id)
        {
            File.Delete($@"{GetBotDirectoryPath()}/BotGuilds/{id}.xml");
        }

        public static void AddGuild(IGuild guild)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableGuild));
            SerializableGuild serializableGuild = new SerializableGuild
            {
                GuildId = guild.Id
            };
            using (FileStream stream = new($@"{GetBotDirectoryPath()}/BotGuilds/{guild.Id}.xml", FileMode.Create))
                serializer.Serialize(stream, serializableGuild);
            Console.WriteLine($"Guild({guild.Id}) serialized.", Color.Green);
        }

        public static void RefreshGuild(SerializableGuild guild)
        {
            try
            {
                XmlSerializer serializer = new(typeof(SerializableGuild));

                File.WriteAllText($@"{GetBotDirectoryPath()}/BotGuilds/{guild.GuildId}.xml", string.Empty);
                using FileStream stream = new($@"{GetBotDirectoryPath()}/BotGuilds/{guild.GuildId}.xml", FileMode.Open, FileAccess.ReadWrite);
                serializer.Serialize(stream, guild);
            }
            catch (IOException e)
            {
                Console.WriteLine($"Refreshing guild failed.\nId: {guild.GuildId}\nPath: {GetBotDirectoryPath()}/BotGuilds/{guild.GuildId}.xml\nStack trace: {e.StackTrace}");
            }
        }

        public static void RefreshGlobalOptions(SerializableGlobalOptions options)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableGlobalOptions));
            string pathToXML = $"{GetBotDirectoryPath()}/GlobalOptions.xml";

            File.WriteAllText(pathToXML, string.Empty);
            using FileStream stream = new FileStream(pathToXML, FileMode.Open);
            serializer.Serialize(stream, options);
        }

        public static SerializableConnector GetConnector(ulong id)
        {
            string pathToDir = $"{GetBotDirectoryPath()}/ServerConnectorsHandlers";
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableConnectors));

            foreach (string file in Directory.GetFiles(pathToDir))
            {
                using FileStream fs = new FileStream(file, FileMode.Open);
                var serConn = (SerializableConnectors)serializer.Deserialize(fs);
                List<ulong> hostsId = serConn.SerializableConnectorsChannels.Select(x => x.HostId).ToList();

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
                List<ulong> hostsId = conn.SerializableConnectorsChannels.Select(x => x.HostId).ToList();

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
            return new SerializableEconomicGuildUser
            {
                Id = user.Id
            };
        }

        public static void AddEconomicGuild(IGuild guild)
        {
            string path = $"{GetBotDirectoryPath()}/EconomicGuilds/{guild.Id}.xml";
            if (!File.Exists(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializableEconomicGuild));
                SerializableEconomicGuild serializableGuild = new SerializableEconomicGuild
                {
                    Id = guild.Id
                };
                using FileStream stream = new FileStream(path, FileMode.Create);
                serializer.Serialize(stream, serializableGuild);
            }
        }

        public static void RefreshEconomicGuild(SerializableEconomicGuild guild)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableEconomicGuild));

            File.WriteAllText($"{GetBotDirectoryPath()}/EconomicGuilds/{guild.Id}.xml", string.Empty);
            using FileStream stream = new FileStream($"{GetBotDirectoryPath()}/EconomicGuilds/{guild.Id}.xml", FileMode.Open, FileAccess.ReadWrite);
            serializer.Serialize(stream, guild);
        }

        public static SerializableReactRoleMessages GetReactRoleMessages()
        {
            string path = $"{GetBotDirectoryPath()}/ReactRoleMessages.xml";

            using FileStream stream = new FileStream(path, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableReactRoleMessages));
            return (SerializableReactRoleMessages)serializer.Deserialize(stream);
        }

        public static void RefreshReactRoleMessages(SerializableReactRoleMessages messages)
        {
            string path = $"{GetBotDirectoryPath()}/ReactRoleMessages.xml";

            File.WriteAllText(path, string.Empty);
            using FileStream stream = new FileStream(path, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableReactRoleMessages));
            serializer.Serialize(stream, messages);
        }

        public static void RefreshReactRoleMessage(SerializableReactRoleMessage message)
        {
            var messages = GetReactRoleMessages();
            int indexOf = 0;

            foreach (var mess in messages.ReactRoleMessages)
                if (mess.Id != message.Id)
                    indexOf++;
                else
                    break;

            messages.ReactRoleMessages[indexOf] = message;

            RefreshReactRoleMessages(messages);
        }

        public static void AddReactRoleMessage(SerializableReactRoleMessage message)
        {
            var messages = GetReactRoleMessages();
            if (!messages.ReactRoleMessages.Contains(message))
            {
                messages.ReactRoleMessages.Add(message);
                RefreshReactRoleMessages(messages);
            }
        }

        public static SerializableReactRoleMessage GetReactRoleMessage(ulong id)
        {
            var messages = GetReactRoleMessages();
            foreach (var mess in messages.ReactRoleMessages)
                if (mess.Id == id)
                    return mess;
            return null;
        }

        public static void AddReactRoleToReactRoleMessage(ulong messId, string reactName, ulong roleId)
        {
            var message = GetReactRoleMessage(messId);

            message.EmojiesRoleId.Add((reactName, roleId));

            RefreshReactRoleMessage(message);
        }

        public static void RemoveReactRoleFromReactRoleMessage(ulong messId, string reactName)
        {
            var message = GetReactRoleMessage(messId);
            int indexOf = 0;
            bool changed = false;

            foreach (var eR in message.EmojiesRoleId)
                if (eR.Item1 != reactName)
                {
                    indexOf++;
                    changed = true;
                }
                else
                    break;
            if (changed)
            {
                message.EmojiesRoleId.Remove(message.EmojiesRoleId[indexOf]);
                RefreshReactRoleMessage(message);
            }
        }
    }
}