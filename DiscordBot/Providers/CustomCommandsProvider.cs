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

using Discord;
using DiscordBot.Compiling;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.CustomCommands;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DiscordBot.Serializable;
using DiscordBot.Serializable.SerializableActions;
using System.Collections.Generic;

namespace DiscordBot.Providers
{
    public class CustomCommandsProvider
    {        
        private SocketGuild Guild { get; set; }
        private CustomCommandsSerial CustomCommandsSerial { get => new CustomCommandsSerial(Guild); }

        public CustomCommandsProvider(SocketGuild guild)
        {
            Guild = guild;            
        }

        public async Task AddCommand(SocketCommandContext context)
        {
            var message = context.Message;
            var files = message.Attachments;
            var compiler = new Compiler(Compiler.CompilerTypeEnum.Command);

            foreach (var file in files)
            {
                if (Path.GetExtension(file.Filename) == ".xml")
                {
                    string url = file.Url;
                    WebClient web = new WebClient();                    
                    var serial = new CustomCommandsSerial(context.Guild);                    
                    var command = (SerializableCommand)new XmlSerializer(typeof(SerializableCommand), new[] { typeof(SerializableBan), typeof(SerializableKick), typeof(SerializableMessage) }).Deserialize(web.OpenRead(url));
                    command.GuildId = context.Guild.Id;
                    var res = compiler.Result(context.Guild, context.Message);
                    var ser = serial.GetCustomCommands();

                    List<string> CommandsNames = new List<string>();
                    if(ser != null)
                        foreach (var _command in ser.Commands)
                        {
                            CommandsNames.Add(_command.Name);
                        }

                    if (CommandsNames.Contains(command.Name))
                        await context.Channel.SendMessageAsync($"Команда с именем {command.Name} уже существует.");

                    await context.Channel.SendMessageAsync(embed: res);                    

                    if(res.Color != Color.Red)
                        serial.SerializeCommand(command);                                           
                }
            }            
        }

        public enum Result { Success, Error }
    }
}