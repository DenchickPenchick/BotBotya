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
using System;
using System.Threading.Tasks;
using DiscordBot.FileWorking;
using DiscordBot.Providers;

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
                    var messUser = arg.Author as SocketGuildUser;
                    GuildProvider provider = new GuildProvider(messUser.Guild);
                    var VideosAndPicturesChannel = provider.VideosTextChannel();
                    var LinksChannel = provider.LinksTextChannel();                    
                    var message = arg;
                    
                    if (Uri.IsWellFormedUriString(message.Content, UriKind.RelativeOrAbsolute))
                    {
                        Uri uri = new Uri(message.Content);
                        string host = uri.Host;
                        bool Video = false;
                        bool DiscordMedia = false;
                        bool InVideoChannel = false;
                        bool InLinksChannel = false;

                        if (message.Channel == VideosAndPicturesChannel)
                            InVideoChannel = true;
                        else if (message.Channel == LinksChannel)
                            InLinksChannel = true;

                        if (host == "www.youtube.com" || host == "www.youtu.be")
                            Video = true;
                        else if (host.ToLower().Contains("discord") || host.ToLower().Contains("tenor"))
                            DiscordMedia = true;

                        if (!DiscordMedia)
                        {
                            if (!((InVideoChannel && Video) || (InLinksChannel && !Video)))
                            {
                                if((!Video && guild.GetTextChannel(serGuild.SystemChannels.LinksChannelId) != null) || (Video && guild.GetTextChannel(serGuild.SystemChannels.VideosChannelId) != null))
                                    await message.DeleteAsync();
                                string link = message.Content;
                                switch (Video)
                                {
                                    case true:
                                        if (guild.GetTextChannel(serGuild.SystemChannels.VideosChannelId) != null)
                                            await VideosAndPicturesChannel.SendMessageAsync(link);  
                                        break;
                                    case false:
                                        if (guild.GetTextChannel(serGuild.SystemChannels.LinksChannelId) != null)
                                            await LinksChannel.SendMessageAsync(link);
                                        break;
                                }
                            }                            
                        }                        
                    }
                }
        }
    }
}
