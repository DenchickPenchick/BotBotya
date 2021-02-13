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
using DiscordBot.TextReaders;
using Discord.Commands;

namespace DiscordBot.Modules.ContentManaging
{
    public class ContentModule : IModule
    {
        private DiscordSocketClient Client { get; set; }
        private CommandService CommandService { get; set; }

        public ContentModule(DiscordSocketClient client, CommandService commandService)
        {
            Client = client;
            CommandService = commandService;
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
            var mess = arg as SocketUserMessage;
            var guild = ((arg as SocketUserMessage).Author as SocketGuildUser).Guild;
            var serGuild = FilesProvider.GetGuild(guild);

            int argPos = 0;

            if (!((arg as SocketUserMessage).Author as SocketGuildUser).IsBot)
            {
                if (serGuild.CheckingBadWords)
                {
                    bool prefix = mess.HasStringPrefix(serGuild.Prefix, ref argPos);
                    string str = mess.Content.Remove(0, serGuild.Prefix.Length).Split(' ')[0];
                    List<string> commandsNames = new List<string>();
                    foreach (var command in CommandService.Commands)
                    {
                        commandsNames.Add(command.Name.ToLower());
                        for (int i = 1; i < command.Aliases.Count; i++)
                            commandsNames.Add(command.Aliases[i].ToLower());
                    }                    

                    if ((prefix && !commandsNames.Contains(str.ToLower())) 
                        || !prefix)
                    {
                        Filter filter = new Filter(arg.Content, serGuild);

                        var res = filter.Filt();
                        if (arg.Author is SocketGuildUser user)
                        {
                            List<ulong> badUsersIds = new List<ulong>();

                            foreach (var badUser in serGuild.BadUsers)
                                badUsersIds.Add(badUser.Item1);

                            if (res == Filter.Result.Words)
                            {
                                if (serGuild.WarnsForBadWords)
                                { 
                                    int warns;
                                    if (!badUsersIds.Contains(user.Id))
                                    {
                                        serGuild.BadUsers.Add((user.Id, 1));
                                        warns = 1;
                                    }
                                    else
                                    {
                                        serGuild.BadUsers[badUsersIds.IndexOf(user.Id)] = (user.Id, serGuild.BadUsers[badUsersIds.IndexOf(user.Id)].Item2 + 1);
                                        warns = serGuild.BadUsers[badUsersIds.IndexOf(user.Id)].Item2;
                                    }
                                    await arg.Channel.SendMessageAsync($"{user.Mention}, на этом сервере запрещен мат." +
                                        $"{(serGuild.WarnsForBadWords == true ? $"\nКоличество предупреждений: ${warns}" : null)}");
                                    if (warns > serGuild.MaxWarns)
                                    {
                                        if (serGuild.KickForWarns || serGuild.BanForWarns) 
                                        {
                                            var channel = await user.GetOrCreateDMChannelAsync();                                        
                                            await channel.SendMessageAsync(embed: new EmbedBuilder
                                            {
                                                Title = serGuild.KickForWarns == true ? $"Ты кикнут с сервера {user.Guild.Name}" : $"Ты забанен на сервере {user.Guild.Name}",
                                                Description = $"Ты  {(serGuild.KickForWarns == true ? "кикнут" : "забанен")} из-за нарушений правил сервера, а именно за употребление запрещенных на сервере слов. " +
                                                $"Ты превысил лимит предупреждений ({serGuild.MaxWarns}).\n" +
                                                $"Сообщение из-за которого тебя выгнали:\n" +
                                                $"`{arg.Content}`",
                                                Color = Color.Blue,
                                                ThumbnailUrl = user.Guild.IconUrl
                                            }.Build());
                                        }

                                        await user.KickAsync();
                                    }
                                }                                
                                await arg.DeleteAsync();


                                FilesProvider.RefreshGuild(serGuild);
                            }

                        }
                    }
                }
                if (serGuild.CheckingContent
                    && guild.GetTextChannel(serGuild.SystemChannels.LinksChannelId) != null
                    && guild.GetTextChannel(serGuild.SystemChannels.VideosChannelId) != null)
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

        private enum ProtocolType { HTTP, HTTPS };
    }
}
