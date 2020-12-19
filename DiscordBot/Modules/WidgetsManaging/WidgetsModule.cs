using Discord.WebSocket;
using DiscordBot.FileWorking;
using System;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace DiscordBot.Modules.WidgetsManaging
{
    public class WidgetsModule : IModule
    {
        private DiscordSocketClient Client { get; set; }

        public enum WidgetType { Users, Channels } 

        public WidgetsModule(DiscordSocketClient client) 
        {
            Client = client;
        }

        public void RunModule()
        {            
            Client.UserJoined += UpdateUsersData;
            Client.UserLeft += UpdateUsersData;
            Client.ChannelCreated += UpdateChannelsData;
            Client.ChannelDestroyed += UpdateChannelsData;            
        }

        private async Task UpdateChannelsData(SocketChannel arg)
        {
            var guild = (arg as SocketGuildChannel).Guild;
            var widget = (SocketGuildChannel)GetWidget(guild, WidgetType.Channels);            
            
            if (widget != null)
                await widget.ModifyAsync(x => x.Name = $"Каналов: {guild.Channels.Count}");
        }

        private async Task UpdateUsersData(SocketGuildUser arg)
        {
            var guild = arg.Guild;
            var widget = (SocketGuildChannel)GetWidget(guild, WidgetType.Users);
            if (widget != null)
                await widget.ModifyAsync(x => x.Name = $"Участников: {guild.Users.Count}");
        }

        private SocketVoiceChannel GetWidget(SocketGuild guild, WidgetType type)
        {
            var serGuild = FilesProvider.GetGuild(guild);

            switch (type)
            {
                case WidgetType.Users:
                    if (serGuild.UsersWidgetId != default)
                    {
                        if (guild.GetVoiceChannel(serGuild.UsersWidgetId) != null)
                            return guild.GetVoiceChannel(serGuild.UsersWidgetId);
                        else
                        {
                            serGuild.UsersWidgetId = default;
                            FilesProvider.RefreshGuild(serGuild);
                            return null;
                        }
                    }
                    break;
                case WidgetType.Channels:
                    if (serGuild.ChannelsWidgetId != default)
                    {
                        if (guild.GetVoiceChannel(serGuild.ChannelsWidgetId) != null)
                            return guild.GetVoiceChannel(serGuild.ChannelsWidgetId);
                        else
                        {
                            serGuild.UsersWidgetId = default;
                            FilesProvider.RefreshGuild(serGuild);
                            return null;
                        }
                    }
                    break;
            }
            return null;
        }     

        private async Task Client_Ready()
        {
            //await Task.Delay(1000);
            //Console.WriteLine("Checking widgets...");
            //var guilds = Client.Guilds;
            //foreach (var guild in guilds)
            //{
            //    Console.WriteLine($"Checking {guild.Id}");
            //    var channelsWidget = GetWidget(guild, WidgetType.Channels);
            //    var usersWidget = GetWidget(guild, WidgetType.Users);
            //    if (channelsWidget != null)
            //        await channelsWidget.ModifyAsync(x => x.Name = $"Каналов: {guild.Channels.Count}");
            //    if (usersWidget != null)
            //        await usersWidget.ModifyAsync(x => x.Name = $"Участников: {guild.Users.Count}");
            //    Console.WriteLine($"{guild.Id} checked", System.Drawing.Color.Green);
            //}
            //Console.WriteLine("Widgets checked", System.Drawing.Color.Green);
        }
    }
}
