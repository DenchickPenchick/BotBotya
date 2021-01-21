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
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.CustomCommands.Actions;
using DiscordBot.Serializable;
using DiscordBot.Serializable.SerializableActions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.CustomCommands
{
    public class CustomCommandsCore
    {
        public SocketCommandContext CommandContext { get; private set; }

        public CustomCommandsCore(SocketCommandContext context)
        {
            CommandContext = context;
        }

        public async Task ExecuteCommand(string name)
        {
            try
            {
                string buffer = null;
                var command = new CustomCommandsSerial(CommandContext.Guild).GetCustomCommand(name);
                var guild = CommandContext.Guild;

                foreach (var action in command.Actions)
                {
                    switch (action.Item1)
                    {
                        case SerializableCommand.CommandActionType.Ban:
                            List<SocketGuildUser> usersB = new List<SocketGuildUser>();
                            var serCommB = action.Item2 as SerializableBan;

                            if (serCommB.DataFromBuffer)
                                foreach (var user in guild.Users)
                                {
                                    if (user.Username == buffer || user.Nickname == buffer)
                                        usersB.Add(user);
                                }
                            else
                            {
                                var u = CommandContext.Message.MentionedUsers;
                                foreach (var us in u)
                                    usersB.Add(us as SocketGuildUser);                                
                            }
                            if (usersB == null && !serCommB.DataFromBuffer)
                            {
                                await CommandContext.Channel.SendMessageAsync(embed: CreateErrorEmbed("Пользователи не найдены."));
                                return;
                            }
                            Ban ban = new Ban(usersB);
                            await ban.DoAction();
                            break;
                        case SerializableCommand.CommandActionType.Interactive:
                            Interactive interactive = new Interactive(CommandContext);
                            buffer = await interactive.DoAction();
                            break;
                        case SerializableCommand.CommandActionType.Kick:
                            List<SocketGuildUser> usersK = new List<SocketGuildUser>();
                            var serCommK = action.Item2 as SerializableKick;
                            if (serCommK.DataFromBuffer)
                                foreach (var userK in guild.Users)
                                {
                                    if (userK.Username == buffer || userK.Nickname == buffer)
                                        usersK.Add(userK);
                                }
                            else
                            {
                                var u = CommandContext.Message.MentionedUsers;
                                foreach (var us in u)
                                    usersK.Add(us as SocketGuildUser);
                            }
                            if (usersK == null && !serCommK.DataFromBuffer)
                            {
                                await CommandContext.Channel.SendMessageAsync(embed: CreateErrorEmbed("Пользователи не найдены."));
                                return;
                            }
                            Kick kick = new Kick(usersK);
                            await kick.DoAction();
                            break;
                        case SerializableCommand.CommandActionType.Message:
                            string messageContent = null;
                            var serCommM = action.Item2 as SerializableMessage;
                            if (serCommM.DataFromBuffer)
                                messageContent = buffer;
                            else
                                messageContent = serCommM.Message;
                            if (messageContent != null)
                            {
                                Message message = new Message(messageContent, CommandContext.Channel);
                                await message.DoAction();
                            }                            
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex by comm: {ex}");
            }            
        }

        private Embed CreateErrorEmbed(string mess) => new EmbedBuilder
        {
            Description = mess,
            Color= Color.Red
        }.Build();
    }
}
