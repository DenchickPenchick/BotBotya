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

using Discord;
using Discord.WebSocket;
using DiscordBot.CustomCommands;
using DiscordBot.CustomCommands.Actions;
using DiscordBot.Modules.FileManaging;
using DiscordBot.Serializable;
using DiscordBot.Serializable.SerializableActions;
using System;
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
            SerializableCommand serComm = null;

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
                        serComm = (SerializableCommand)new XmlSerializer(typeof(SerializableCommand), new[] { typeof(SerializableBan), typeof(SerializableKick), typeof(SerializableMessage) }).Deserialize(stream);
                        break;
                }

                switch (CompilerType)
                {
                    case CompilerTypeEnum.Guild:
                        try
                        {
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
                        }
                        catch (Exception ex) { Console.WriteLine(ex); }
                        break;
                    case CompilerTypeEnum.Command:                        
                        int currentPos = 1;
                        bool kick = false;
                        bool ban = false;
                        bool dataEn = false;

                        try
                        {
                            foreach (var action in serComm.Actions)
                            {
                                switch (action.Item1)
                                {
                                    case SerializableCommand.CommandActionType.Ban:
                                        if (!dataEn && (action.Item2 as SerializableBan).DataFromBuffer)
                                            embed.Warnings.Add(new WarningField($"Действие Ban обращается к буферу, но буфер пустой. Возможные варианты решения проблемы: добавить действие Interactive перед действием Ban, отключить обращение к буферу. Номер действия: {currentPos}."));
                                        if ((kick || ban) && !(action.Item2 as SerializableBan).DataFromBuffer)
                                            embed.Errors.Add(new ErrorField($"Действие Ban не может обращаться к содержанию сообщения, т.к. какое-то действие уже к нему обращается. Возможный вариант решения проблемы: установить параметр DataFromBuffer на true. Номер действия: {currentPos}."));
                                        if (!(action.Item2 as SerializableBan).DataFromBuffer)
                                            ban = true;
                                        currentPos++;
                                        break;
                                    case SerializableCommand.CommandActionType.Interactive:
                                        if (currentPos == 1)
                                            embed.Errors.Add(new ErrorField($"Ожидание сообщения после запроса на выполнение команды. Номер действия: {currentPos}."));
                                        dataEn = true;
                                        currentPos++;
                                        break;
                                    case SerializableCommand.CommandActionType.Kick:
                                        if (!dataEn && (action.Item2 as SerializableKick).DataFromBuffer)
                                            embed.Warnings.Add(new WarningField($"Действие Kick обращается к буферу, но буфер пустой. Возможные варианты решения проблемы: добавить действие Interactive перед действием Kick, отключить обращение к буферу. Номер действия: {currentPos}."));
                                        if ((kick || ban) && !(action.Item2 as SerializableKick).DataFromBuffer)
                                            embed.Errors.Add(new ErrorField($"Действие Kick не может обращаться к содержанию сообщения, т.к. какое-то действие уже к нему обращается. Возможный вариант решения проблемы: установить параметр DataFromBuffer на true. Номер действия: {currentPos}."));
                                        if (!(action.Item2 as SerializableKick).DataFromBuffer)
                                            kick = true;
                                        currentPos++;
                                        break;
                                    case SerializableCommand.CommandActionType.Message:
                                        if (!dataEn && (action.Item2 as SerializableMessage).DataFromBuffer && currentPos == 1)
                                            embed.Errors.Add(new ErrorField($"Действие Message обращается к буферу, но буфер пустой, также оно стоит в начале. Возможные варианты решения проблемы: добавить действие Interactive перед действием Message, отключить обращение к буферу. Номер действия: {currentPos}."));
                                        else if (!dataEn && (action.Item2 as SerializableMessage).DataFromBuffer)
                                            embed.Warnings.Add(new WarningField($"Действие Message обращается к буферу, но буфер пустой. Возможные варианты решения проблемы: добавить действие Interactive перед действием Message, отключить обращение к буферу. Номер действия: {currentPos}."));
                                        currentPos++;
                                        break;
                                }
                            }
                        }
                        catch (NullReferenceException)
                        {
                            embed.Errors.Add(new ErrorField("Нарушена типовая структура программы. Возможный вариант решения проблемы: проверьте написание команд, тегов и т.д."));
                        }
                        break;
                }
            }
            return embed.Build();
        }

        public enum CompilerTypeEnum { Guild, Command }
    }
}
