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
                    var provider = new GuildProvider(guild);
                    var linksChannel = provider.LinksTextChannel();
                    var videosChannel = provider.VideosTextChannel();                    

                    var uris = GetUrisFromMessage(arg);

                    List<string> VideoHostnames = new List<string>
                    { 
                        "www.youtube.com",
                        "youtube.com",
                        "www.youtu.be",
                        "youtu.be"
                    };

                    List<string> ContentLinks = new List<string>
                    { 
                        "discord.com",
                        "tenor.com"
                    };

                    List<string> links = new List<string>();

                    foreach (var uri in uris)
                    {
                        bool sorted = false;

                        if (uri.Host == "discord.gg" && uri.AbsolutePath.Length == 11) //Если приглашение, тогда прекращаем операцию
                        {
                            await arg.DeleteAsync();
                            return;
                        }

                        if (VideoHostnames.Contains(uri.Host))
                        {
                            if (videosChannel != null && !links.Contains(uri.ToString()))
                            {
                                links.Add(uri.ToString());
                                if (arg.Channel.Id != videosChannel.Id)
                                {
                                    await videosChannel.SendMessageAsync(uri.ToString());
                                    await arg.DeleteAsync();
                                }
                                
                                sorted = true;
                            }
                        }

                        if (ContentLinks.Contains(uri.Host))
                        {
                            if (!links.Contains(uri.ToString()))
                            {
                                links.Add(uri.ToString());                                                                
                                sorted = true;
                            }
                        }

                        if (sorted)
                        {
                            continue;
                        }

                        if (linksChannel == null || links.Contains(uri.ToString()))
                        {
                            continue;
                        }
                        if (arg.Channel.Id != linksChannel.Id)
                        {
                            await arg.DeleteAsync();
                            await linksChannel.SendMessageAsync(uri.ToString());
                        }
                        
                            links.Add(uri.ToString());
                    }                                            
                }
        }
        #region --АЛГОРИТМ ПО ИЗВЛЕЧЕНИБ ССЫЛОК ИЗ СООБЩЕНИЯ
        private List<Uri> GetUrisFromMessage(SocketMessage message)
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
                        if (httpChars && chars.Length - (i + 6) >= 0)
                        {
                            url += "http";
                            string doubleSlash = chars[i + 4].ToString() + chars[i + 5].ToString() + chars[i + 6].ToString();
                            if (doubleSlash == "://")
                            {
                                url += "://";
                                url += Path(chars, i + 7, ProtocolType.HTTP);
                                if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                                    uris.Add(new Uri(url));
                            }
                        }
                        else if(httpsChars && chars.Length - (i + 7) >= 0)
                        {
                            url += "https";
                            string doubleSlash = chars[i + 5].ToString() + chars[i + 6].ToString() + chars[i + 7].ToString();
                            if (doubleSlash == "://")
                            {
                                url += "://";
                                url += Path(chars, i + 8, ProtocolType.HTTPS);
                                if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                                    uris.Add(new Uri(url));
                            }
                        }
                    }
                }
            }
            return uris;
        }

        private string Path(char[] chars, int index, ProtocolType protocolType)
        {
            string pathRes = null;

            for (int i = index; i < chars.Length; i++)
            {
                bool nextIsNewLink = false;

                switch (protocolType)
                {
                    case ProtocolType.HTTP:
                        if (chars.Length - (i + 7) >= 0)
                        {
                            nextIsNewLink = chars[i].ToString() + chars[i + 1].ToString() + chars[i + 2].ToString() + chars[i + 3].ToString() + chars[i + 4].ToString() + chars[i + 5].ToString() + chars[i + 6].ToString() == "http://";
                        }
                        break;
                    case ProtocolType.HTTPS:
                        if (chars.Length - (i + 8) >= 0)
                        {
                            nextIsNewLink = chars[i].ToString() + chars[i + 1].ToString() + chars[i + 2].ToString() + chars[i + 3].ToString() + chars[i + 4].ToString() + chars[i + 5].ToString() + chars[i + 6].ToString() + chars[i + 7].ToString() == "https://";
                        }
                        break;
                }

                if (chars[i] != ' ' && !nextIsNewLink)
                {
                    pathRes += chars[i].ToString();
                }
                else
                    break;
            }

            return pathRes;
        }
        #endregion

        #region--РАССТОЯНИЕ ЛЕВЕНШТЕЙНА--
        private int Minimum(int a, int b, int c) => (a = a < b ? a : b) < c ? a : c;

        private int Distance(string firstWord, string secondWord)
        {
            const int deleteCost = 1, insertCost = 1;
            int n = firstWord.Length + 1;
            int m = secondWord.Length + 1;
            int[,] matrix = new int[n, m];

            for (int i = 0; i < n; i++)
                matrix[i, 0] = i;

            for (int i = 0; i < m; i++)
                matrix[0, i] = i;


            for (int i = 1; i < n; i++)
            {
                for (int j = 1; j < m; j++)
                {
                    int substitutionCost = firstWord[i - 1] == secondWord[j - 1] ? 0 : 1;
                    matrix[i, j] = Minimum(
                        matrix[i - 1, j] + deleteCost,
                        matrix[i, j - 1] + insertCost,
                        matrix[i - 1, j - 1] + substitutionCost);
                }
            }

            return matrix[n - 1, m - 1];
        }
        #endregion

        private enum ProtocolType { HTTP, HTTPS };
    }
}
