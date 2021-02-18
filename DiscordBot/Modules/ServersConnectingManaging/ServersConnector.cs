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
using Discord.WebSocket;
using DiscordBot.Providers;
using DiscordBot.Serializable;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DiscordBot.Modules.ServersConnectingManaging
{
    public class ServersConnector : IModule
    {
        private readonly DiscordSocketClient Client;

        public ServersConnector(DiscordSocketClient client)
        {
            Client = client;
            client.MessageReceived += Client_MessageReceived;
            client.ChannelDestroyed += Client_ChannelDestroyed;
        }

        public void RunModule()
        {
            
        }

        private Task Client_ChannelDestroyed(SocketChannel arg)
        {
            if (arg is SocketTextChannel channel)
            { 
                var guild = channel.Guild;

                if (guild != null)
                {
                    var connectors = FilesProvider.GetConnectors(guild);
                    SerializableConnector connector = null;
                    if (connectors != null)
                        foreach (var conn in connectors.SerializableConnectorsChannels)
                            if (channel.Id == conn.HostId)
                                connector = conn;

                    if (connector != null)
                    {
                        connectors.SerializableConnectorsChannels.Remove(connector);
                        FilesProvider.RefreshConnectors(connectors);
                    }
                }            
            }
            
            return Task.CompletedTask;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            try
            {
                var connector = FilesProvider.GetConnector(arg.Channel.Id);

                if (connector == null)
                    return;

                var messageEmbed = GenerateUserMessage(arg);
                if(!arg.Author.IsBot && arg.Author.Id != Client.CurrentUser.Id)
                    foreach (ulong id in connector.EndPointsId)
                    {
                        SocketTextChannel channel = (SocketTextChannel)Client.GetChannel(id);
                        if (channel != null)
                        {
                            WebClient webClient = new WebClient();
                            await channel.SendMessageAsync(embed: messageEmbed);
                            if (arg.Attachments.Count > 0)
                                foreach (var attach in arg.Attachments)
                                    await channel.SendFileAsync(webClient.OpenRead(attach.Url), attach.Filename, "Входящий файл");
                        }
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private Embed GenerateUserMessage(SocketMessage message)
        {
            string content = message.Content;
            var author = message.Author;
            var guild = (author as SocketGuildUser).Guild;
            var serGuild = FilesProvider.GetGuild(guild);

            return new EmbedBuilder
            {
                Title = $"Сообщение с сервера {guild.Name}",
                Description = content,
                Author = new EmbedAuthorBuilder
                { 
                    IconUrl = author.GetAvatarUrl(),
                    Name = author.Username
                },
                Footer = new EmbedFooterBuilder
                { 
                Text = $"{DateTime.Now.ToShortTimeString()} | {DateTime.Now.ToShortDateString()}"
                },
                Color = Color.Blue
            }.Build();
        }
    }
}
