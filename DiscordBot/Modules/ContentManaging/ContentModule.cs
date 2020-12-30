using Discord;
using Discord.WebSocket;
using DiscordBot.GuildManaging;
using System;
using System.Threading.Tasks;
using DiscordBot.FileWorking;

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
            if (!((arg2 as SocketUserMessage).Author as SocketGuildUser).IsBot)
                if (FilesProvider.GetGuild(((arg2 as SocketUserMessage).Author as SocketGuildUser).Guild).CheckingContent && FilesProvider.GetGuild(((arg2 as SocketUserMessage).Author as SocketGuildUser).Guild).ContentEnable)
                {
                    var messUser = arg2.Author as SocketGuildUser;
                    GuildProvider provider = new GuildProvider(messUser.Guild);
                    var VideosAndPicturesChannel = provider.VideosTextChannel();
                    var LinksChannel = provider.LinksTextChannel();
                    var message = arg2;

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
                                await message.DeleteAsync();
                                string link = message.Content;
                                switch (Video)
                                {
                                    case true:
                                        await VideosAndPicturesChannel.SendMessageAsync(link);
                                        break;
                                    case false:
                                        await LinksChannel.SendMessageAsync(link);
                                        break;
                                }
                            }
                        }
                    }
                }            
        }

        private async Task CheckContent(SocketMessage arg)
        {
            if (!((arg as SocketUserMessage).Author as SocketGuildUser).IsBot)
                if (FilesProvider.GetGuild(((arg as SocketUserMessage).Author as SocketGuildUser).Guild).CheckingContent && FilesProvider.GetGuild(((arg as SocketUserMessage).Author as SocketGuildUser).Guild).ContentEnable)
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
                                await message.DeleteAsync();
                                string link = message.Content;
                                switch (Video)
                                {
                                    case true:
                                        await VideosAndPicturesChannel.SendMessageAsync(link);
                                        break;
                                    case false:
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
