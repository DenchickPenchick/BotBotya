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

using Discord.WebSocket;
using DiscordBot.FileWorking;
using DiscordBot.Serializable;
using DiscordBot.Serializable.SerializableActions;
using System.IO;
using System.Xml.Serialization;

namespace DiscordBot.CustomCommands
{
    public class CustomCommandsSerial
    {
        private SocketGuild Guild { get; set; }

        public CustomCommandsSerial(SocketGuild guild)
        {
            Guild = guild;
        }

        public void SerializeCommand(SerializableCommand command)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableCommands), new[] { typeof(SerializableBan), typeof(SerializableKick), typeof(SerializableMessage) });
            SerializableCommands commands;
            string pathToFile = $"{FilesProvider.GetBotDirectoryPath()}/CustomCommandsDirectory/{Guild.Id}.xml";

            if (!File.Exists(pathToFile))
                using (FileStream fs = new FileStream(pathToFile, FileMode.Create))
                    new XmlSerializer(typeof(SerializableCommands), new[] { typeof(SerializableBan), typeof(SerializableKick), typeof(SerializableMessage) }).Serialize(fs, new SerializableCommands());

            using (StreamReader reader = new StreamReader(pathToFile))
            {
                commands = (SerializableCommands)new XmlSerializer(typeof(SerializableCommands), new[] { typeof(SerializableBan), typeof(SerializableKick), typeof(SerializableMessage) }).Deserialize(reader);
            }
            command.Name = command.Name.Replace(" ", string.Empty);
            commands.Commands.Add(command);

            using StreamWriter writer = new StreamWriter(pathToFile);            
            serializer.Serialize(writer, commands);
            writer.Close();
        }

        public void DeleteCommand(string name)
        {
            var commands = GetCustomCommands();
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableCommands), new[] { typeof(SerializableBan), typeof(SerializableKick), typeof(SerializableMessage) });
            var command = GetCustomCommand(name);
            string pathToFile = $"{FilesProvider.GetBotDirectoryPath()}/CustomCommandsDirectory/{Guild.Id}.xml";

            if (File.Exists(pathToFile))
                using (StreamWriter writer = new StreamWriter(pathToFile))                
                    if (command != null)
                    {
                        commands.Commands.Remove(command);
                        writer.Write(string.Empty);
                        serializer.Serialize(writer, commands);
                    }                                    
        }

        public SerializableCommands GetCustomCommands()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableCommands), new[] { typeof(SerializableBan), typeof(SerializableKick), typeof(SerializableMessage) });
            string pathToFile = $"{FilesProvider.GetBotDirectoryPath()}/CustomCommandsDirectory/{Guild.Id}.xml";
            if (File.Exists(pathToFile))
                using (StreamReader reader = new StreamReader(pathToFile))
                    return (SerializableCommands)serializer.Deserialize(reader);            
            else
                return null;
        }

        public SerializableCommand GetCustomCommand(string name)
        {
            var commands = GetCustomCommands().Commands;

            foreach (var command in commands)            
                if (command.Name.ToLower() == name.ToLower())
                    return command;
            return null;
        }
    }
}
