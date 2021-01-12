using Discord;
using DiscordBot.Compiling;
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
                    var command = (CustomCommand)new XmlSerializer(typeof(CustomCommand)).Deserialize(web.OpenRead(url));
                    command.GuildId = context.Guild.Id;
                    var res = compiler.Result(context.Guild, context.Message);

                    await context.Channel.SendMessageAsync(embed: res);
                    if(res.Color != Color.Red)
                        serial.SerializeCommand(command);                                           
                }
            }            
        }

        public enum Result { Success, Error }
    }
}