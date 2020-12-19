using Discord;
using Discord.WebSocket;
using DiscordBot.GuildManaging;
using System.Linq;
using System;
using System.Threading.Tasks;
using DiscordBot.FileWorking;
using System.Threading;

namespace DiscordBot.Modules.ContentManaging
{
    /// <summary>
    /// Модуль, который отвечает за управление контентом 
    /// </summary>
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
                    var provider = new GuildProvider(messUser.Guild);
                    var VideosAndPicturesChannel = provider.VideosTextChannel();
                    var LinksChannel = provider.LinksTextChannel();
                    var textChannel = arg2.Channel;
                    var message = arg2;

                    if (textChannel == VideosAndPicturesChannel && !message.Content.Contains("https://youtu.be/"))
                    {
                        if (message.Content.Contains("https://"))
                        {
                            await message.DeleteAsync();
                            string link = message.Content;
                            await LinksChannel.SendMessageAsync(link);
                        }
                        else
                        {
                            await message.DeleteAsync();
                        }
                    }
                    else if (textChannel == LinksChannel)
                    {
                        if (message.Content.Contains("https://youtu.be/"))
                        {
                            await message.DeleteAsync();
                            string link = message.Content;
                            await VideosAndPicturesChannel.SendMessageAsync(link);
                        }
                        else
                        {
                            await message.DeleteAsync();
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
                    var MainChannel = provider.MainTextChannelsCategory();
                    var textChannel = arg.Channel as SocketTextChannel;
                    var message = arg;

                    if (textChannel == VideosAndPicturesChannel && !message.Content.Contains("https://youtu.be/") && !message.Content.Contains("https://www.youtube.com/"))
                    {
                        if (message.Content.Contains("https://"))
                        {
                            await message.DeleteAsync();
                            string link = message.Content;
                            await LinksChannel.SendMessageAsync(link);
                        }
                        else
                        {
                            await message.DeleteAsync();
                        }
                    }
                    else if (MainChannel.Channels.Contains(textChannel) && message.Content.Contains("https://"))
                    {
                        if (message.Content.Contains("https://youtu.be/") || message.Content.Contains("https://www.youtube.com/"))
                        {
                            await message.DeleteAsync();
                            string link = message.Content;
                            await VideosAndPicturesChannel.SendMessageAsync(link);
                        }
                        else if (!(message.Content.StartsWith("https://tenor.com/") || message.Content.StartsWith("https://cdn.discordapp.com/") || message.Content.StartsWith("https://media.discordapp.net/")))
                        {
                            await message.DeleteAsync();
                            string link = message.Content;
                            await LinksChannel.SendMessageAsync(link);
                        }
                    }
                    else if (textChannel == LinksChannel)
                    {
                        if (message.Content.Contains("https://youtu.be/") || message.Content.Contains("https://www.youtube.com/"))
                        {
                            await message.DeleteAsync();
                            string link = message.Content;
                            await VideosAndPicturesChannel.SendMessageAsync(link);
                        }
                        else if (!message.Content.Contains("https://"))
                        {
                            await message.DeleteAsync();
                        }
                    }
                }            
        }
    }
}
