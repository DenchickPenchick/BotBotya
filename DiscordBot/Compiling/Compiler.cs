using Discord;
using Discord.WebSocket;
using DiscordBot.CustomCommands;
using DiscordBot.Modules.FileManaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;

namespace DiscordBot.Compiling
{
    public class Compiler
    {
        public CompilerTypeEnum CompilerType { get; set; }

        public Compiler(CompilerTypeEnum compiler)
        {
            CompilerType = compiler;
        }

        public Embed Result(SocketGuild guild, IMessage message)
        {
            var embed = new CompilerEmbed();
            var attachment = message.Attachments.FirstOrDefault();
            WebClient webClient = new WebClient();
            var stream = webClient.OpenRead(attachment.Url);
            SerializableGuild serGuild = null;
            CustomCommand serComm = null;

            if (Path.GetExtension(attachment.Filename) != ".xml")
                embed.Errors.Add(new ErrorField("Некорректное расширение файла."));
            else
            {
                switch (CompilerType)
                {
                    case CompilerTypeEnum.Guild:
                        serGuild = (SerializableGuild)new XmlSerializer(typeof(SerializableGuild)).Deserialize(stream);
                        break;
                    case CompilerTypeEnum.Command:
                        serComm = (CustomCommand)new XmlSerializer(typeof(CustomCommand)).Deserialize(stream);
                        break;
                }

                switch (CompilerType)
                {
                    case CompilerTypeEnum.Guild:                                                
                        if (guild.GetRole(serGuild.DefaultRoleId) == null && serGuild.DefaultRoleId != 0)
                            embed.Warnings.Add(new WarningField($"Роли с Id {serGuild.DefaultRoleId} не существует."));
                        if (guild.GetTextChannel(serGuild.LoggerId) == null && serGuild.LoggerId != 0)
                            embed.Warnings.Add(new WarningField($"Канала логирования с Id {serGuild.LoggerId} не существует."));
                        if (guild.GetCategoryChannel(serGuild.SystemCategories.VoiceRoomsCategoryId) == null && serGuild.SystemCategories.VoiceRoomsCategoryId != 0)
                            embed.Warnings.Add(new WarningField($"Категории с Id {serGuild.SystemCategories.VoiceRoomsCategoryId} для создания каналов не существует."));
                        if (guild.GetCategoryChannel(serGuild.SystemCategories.ContentCategoryId) == null && serGuild.SystemCategories.ContentCategoryId != 0)
                            embed.Warnings.Add(new WarningField($"Категории контента с Id {serGuild.SystemCategories.ContentCategoryId} не существует."));
                        if (guild.GetVoiceChannel(serGuild.SystemChannels.CreateRoomChannelId) == null && serGuild.SystemChannels.CreateRoomChannelId != 0)
                            embed.Warnings.Add(new WarningField($"Голосового канала для создания каналов с Id {serGuild.SystemChannels.CreateRoomChannelId} не существует."));
                        if (guild.GetTextChannel(serGuild.SystemChannels.LinksChannelId) == null && serGuild.SystemChannels.LinksChannelId != 0)
                            embed.Warnings.Add(new WarningField($"Канала для ссылок с Id {serGuild.SystemChannels.LinksChannelId} не существует."));
                        if (guild.GetTextChannel(serGuild.SystemChannels.VideosChannelId) == null && serGuild.SystemChannels.VideosChannelId != 0)
                            embed.Warnings.Add(new WarningField($"Канала для видео с Id {serGuild.SystemChannels.VideosChannelId} не существует."));                        
                        break;
                    case CompilerTypeEnum.Command:
                        if (serComm.Actions.Contains(CustomCommand.Action.Ban) && serComm.Actions.Contains(CustomCommand.Action.Kick))
                            embed.Errors.Add(new ErrorField("Действия Ban и Kick не могут стоять вместе."));
                        int messCount = 0;
                        int kickCount = 0;
                        int banCount = 0;
                        foreach (var action in serComm.Actions)
                            switch (action)
                            {
                                case CustomCommand.Action.Message:
                                    messCount++;
                                    break;
                                case CustomCommand.Action.Kick:
                                    kickCount++;
                                    break;
                                case CustomCommand.Action.Ban:
                                    banCount++;
                                    break;
                            }
                        if (messCount > 1)
                            embed.Errors.Add(new ErrorField("Количество сообщений не может быть больше одного."));
                        if (kickCount > 1)
                            embed.Errors.Add(new ErrorField("Количество киков не может быть больше одного."));
                        if (banCount > 1)
                            embed.Errors.Add(new ErrorField("Количество банов не может быть больше одного."));
                        break;
                }
            }            
            return embed.Build();
        }

        public enum CompilerTypeEnum { Guild, Command }
    }
}
