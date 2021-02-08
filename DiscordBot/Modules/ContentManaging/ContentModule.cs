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
using System;
using System.Threading.Tasks;
using DiscordBot.Providers;
using System.Collections.Generic;

namespace DiscordBot.Modules.ContentManaging
{
    public class ContentModule : IModule
    {
        private DiscordSocketClient Client { get; set; }

        public ContentModule(DiscordSocketClient client)
        {
            Client = client;
        }

        public void RunModule()
        {
            Client.MessageReceived += CheckContent;
            Client.MessageUpdated += MessageUpdated;
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            await CheckContent(arg2);
        }

        private async Task CheckContent(SocketMessage arg)
        {
            var guild = ((arg as SocketUserMessage).Author as SocketGuildUser).Guild;
            var serGuild = FilesProvider.GetGuild(guild);

            if (!((arg as SocketUserMessage).Author as SocketGuildUser).IsBot)
                if (serGuild.CheckingContent && guild.GetTextChannel(serGuild.SystemChannels.LinksChannelId) != null && guild.GetTextChannel(serGuild.SystemChannels.VideosChannelId) != null)
                {
                    List<Uri> uris = GetUrisFromMessage(arg);
                    var messUser = arg.Author as SocketGuildUser;
                    GuildProvider provider = new GuildProvider(messUser.Guild);
                    var VideosAndPicturesChannel = provider.VideosTextChannel();
                    var LinksChannel = provider.LinksTextChannel();
                    var message = arg;
                    bool Video = false;                    
                    bool DiscordMedia = false;
                    bool Invite = false;
                    bool InVideoChannel = false;
                    bool InLinksChannel = false;

                    if (uris.Count == 1)
                        foreach (Uri uri in uris)
                        {
                            string host = uri.Host;

                            if (host == "www.youtube.com" || host == "www.youtu.be")
                                Video = true;
                            else if (host.ToLower().Contains("discord") || host.ToLower().Contains("tenor"))
                                DiscordMedia = true;
                        }
                    else
                        await arg.DeleteAsync();

                    if (message.Channel == VideosAndPicturesChannel)
                        InVideoChannel = true;
                    else if (message.Channel == LinksChannel)
                        InLinksChannel = true;

                    if (Video && !DiscordMedia)
                        await VideosAndPicturesChannel.SendMessageAsync(arg.Content);
                    else if (Video && !DiscordMedia)
                        await LinksChannel.SendMessageAsync(arg.Content);
                    if (Invite)
                        await arg.DeleteAsync();
                    //var messUser = arg.Author as SocketGuildUser;
                    //GuildProvider provider = new GuildProvider(messUser.Guild);
                    //var VideosAndPicturesChannel = provider.VideosTextChannel();
                    //var LinksChannel = provider.LinksTextChannel();
                    //var message = arg;

                    //if (Uri.IsWellFormedUriString(message.Content, UriKind.RelativeOrAbsolute))
                    //{
                    //    Uri uri = new Uri(message.Content);
                    //    string host = uri.Host;
                    //    bool Video = false;
                    //    bool DiscordMedia = false;
                    //    bool InVideoChannel = false;
                    //    bool InLinksChannel = false;

                    //    if (message.Channel == VideosAndPicturesChannel)
                    //        InVideoChannel = true;
                    //    else if (message.Channel == LinksChannel)
                    //        InLinksChannel = true;

                    //    if (host == "www.youtube.com" || host == "www.youtu.be")
                    //        Video = true;
                    //    else if (host.ToLower().Contains("discord") || host.ToLower().Contains("tenor"))
                    //        DiscordMedia = true;

                    //    if (!DiscordMedia)
                    //    {
                    //        if (!((InVideoChannel && Video) || (InLinksChannel && !Video)))
                    //        {
                    //            if ((!Video && guild.GetTextChannel(serGuild.SystemChannels.LinksChannelId) != null) || (Video && guild.GetTextChannel(serGuild.SystemChannels.VideosChannelId) != null))
                    //                await message.DeleteAsync();
                    //            string link = message.Content;
                    //            switch (Video)
                    //            {
                    //                case true:
                    //                    if (guild.GetTextChannel(serGuild.SystemChannels.VideosChannelId) != null)
                    //                        await VideosAndPicturesChannel.SendMessageAsync(link);
                    //                    break;
                    //                case false:
                    //                    if (guild.GetTextChannel(serGuild.SystemChannels.LinksChannelId) != null)
                    //                        await LinksChannel.SendMessageAsync(link);
                    //                    break;
                    //            }
                    //        }
                    //    }
                    //}
                }
        }

        private static List<Uri> GetUrisFromMessage(SocketMessage message)
        {
            List<Uri> uris = new List<Uri>();
            string content = message.Content;
            char[] chars = content.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == 'h')
                {
                    bool httpChars = chars[i + 1] == 't' && chars[i + 2] == 't' && chars[i + 3] == 'p';
                    bool httpsChars = httpChars && chars[i + 4] == 's';
                    if (httpsChars)
                        httpChars = false;
                    if (httpChars || httpsChars)
                    {
                        string url = null;
                        if (httpChars)
                        {
                            url += "http";
                            string doubleSlash = chars[i + 4].ToString() + chars[i + 5].ToString() + chars[i + 6].ToString();
                            if (doubleSlash == "://")
                            {
                                url += "://";
                                url += Path(chars, i + 7);
                                if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                                    uris.Add(new Uri(url));
                            }
                        }
                        else
                        {
                            url += "https";
                            string doubleSlash = chars[i + 5].ToString() + chars[i + 6].ToString() + chars[i + 7].ToString();
                            if (doubleSlash == "://")
                            {
                                url += "://";
                                url += Path(chars, i + 8);
                                if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                                    uris.Add(new Uri(url));
                            }
                        }
                    }
                }
            }
            return uris;
        }

        private static string Path(char[] chars, int index)
        {
            string pathRes = null;
            for (int i = index; i < chars.Length; i++)
            {
                if (chars[i] != ' ')
                {
                    pathRes += chars[i].ToString();
                }
                else
                    break;
            }

            return pathRes;
        }
    }
}
