using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.CustomCommands;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

        public async Task<Result> ExecuteCustomCommandAsync(SocketCommandContext context, string name)
        {
            var command = CustomCommandsSerial.GetCustomCommand(name);
            if (command == null)                            
                return Result.Error;
            
            
            foreach (var action in command.Actions)
            {
                switch (action)
                {
                    case CustomCommand.Action.Message:
                        await context.Channel.SendMessageAsync(command.Message);
                        break;
                    case CustomCommand.Action.Kick:
                        if (command.Actions.Contains(CustomCommand.Action.Ban))
                            return Result.Error;
                        var usersToKick = context.Message.MentionedUsers;
                        if (usersToKick.Count == 0)                        
                            return Result.Error;                                                   
                        foreach (var user in usersToKick)                        
                            await (user as SocketGuildUser).KickAsync();                        
                        break;
                    case CustomCommand.Action.Ban:
                        if (command.Actions.Contains(CustomCommand.Action.Kick))
                            return Result.Error;
                        var usersToBan = context.Message.MentionedUsers;
                        if (usersToBan.Count == 0)
                            return Result.Error;
                        foreach (var user in usersToBan)
                            await (user as SocketGuildUser).BanAsync();
                        break;                    
                }
            }
            return Result.Success;
        }

        public Task<Result> AddCommand(SocketCommandContext context)
        {
            var message = context.Message;
            var files = message.Attachments;

            foreach (var file in files)
            {
                if (Path.GetExtension(file.Filename) == ".xml")
                {
                    string url = file.Url;
                    WebClient web = new WebClient();                    
                    var serial = new CustomCommandsSerial(context.Guild);

                    try
                    {                        
                        var command = (CustomCommand)new XmlSerializer(typeof(CustomCommand)).Deserialize(web.OpenRead(url));
                        command.GuildId = context.Guild.Id;
                        serial.SerializeCommand(command);                       
                    }
                    catch
                    {
                        return Task.FromResult(Result.Error);
                    }
                }
            }
            return Task.FromResult(Result.Success);
        }

        public enum Result { Success, Error }
    }
}