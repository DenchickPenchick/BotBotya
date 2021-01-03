using Discord.WebSocket;
using DiscordBot.FileWorking;
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

        public void SerializeCommand(CustomCommand command)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CustomCommands));
            CustomCommands commands;
            string pathToFile = $"{FilesProvider.GetBotDirectoryPath()}/CustomCommandsDirectory/{Guild.Id}.xml";

            if (!File.Exists(pathToFile))
                using (FileStream fs = new FileStream(pathToFile, FileMode.Create))
                    new XmlSerializer(typeof(CustomCommands)).Serialize(fs, new CustomCommands());

            using (StreamReader reader = new StreamReader(pathToFile))
            {
                commands = (CustomCommands)new XmlSerializer(typeof(CustomCommands)).Deserialize(reader);
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
            XmlSerializer serializer = new XmlSerializer(typeof(CustomCommands));
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

        public CustomCommands GetCustomCommands()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CustomCommands));
            string pathToFile = $"{FilesProvider.GetBotDirectoryPath()}/CustomCommandsDirectory/{Guild.Id}.xml";
            if (File.Exists(pathToFile))
                using (StreamReader reader = new StreamReader(pathToFile))
                    return (CustomCommands)serializer.Deserialize(reader);            
            else
                return null;
        }

        public CustomCommand GetCustomCommand(string name)
        {
            var commands = GetCustomCommands().Commands;

            foreach (var command in commands)            
                if (command.Name.ToLower() == name.ToLower())
                    return command;
            return null;
        }
    }
}
