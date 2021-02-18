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
using System.Threading.Tasks;
using System;
using DiscordBot.Serializable;
using System.Collections.Generic;

namespace DiscordBot.Providers
{
    public class GuildProvider
    {
        public SocketGuild Guild { get; }
        public SerializableGuild SerializableGuild { get => FilesProvider.GetGuild(Guild);}

        public GuildProvider(SocketGuild guild)
        {            
            Guild = guild;
        }

        public SocketVoiceChannel CreateRoomChannel() => Guild.GetVoiceChannel(SerializableGuild.SystemChannels.CreateRoomChannelId);
        public SocketTextChannel LinksTextChannel() => Guild.GetTextChannel(SerializableGuild.SystemChannels.LinksChannelId);
        public SocketTextChannel VideosTextChannel() => Guild.GetTextChannel(SerializableGuild.SystemChannels.VideosChannelId);        
        public SocketCategoryChannel RoomsCategoryChannel() => Guild.GetCategoryChannel(SerializableGuild.SystemCategories.VoiceRoomsCategoryId);
        public SocketCategoryChannel ContentCategoryChannel() => Guild.GetCategoryChannel(SerializableGuild.SystemCategories.ContentCategoryId);                

        public enum GetCategoryIdEnum { MainVoiceChannelsCategory, RoomVoiceChannelsCategory, MainTextChannelsCategory, ContentChannelsCategory, BotChannelsCategory }
                
        public async Task SendHelloMessageToGuild(DiscordSocketClient client)
        {
            try
            {
                await Guild.DefaultChannel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = $"👋 Спасибо, что пригласили меня на сервер {Guild.Name} 👋",
                    Description = $"Меня зовут {client.CurrentUser.Username}. Я много что умею! Пропиши `!Хелп`, чтобы узнать мой функционал.\n" +
                    $"🤖 *Мой сайт:* https://botbotya.ru 🤖\n" +
                    $"🤖 *GitHub:* https://github.com/DenchickPenchick/BotBotya 🤖",
                    Color = Color.Blue
                }.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
            }
        }

        public void SetWarns(IUser user, int warns)
        {
            var serGuild = SerializableGuild;
            List<ulong> ids = new List<ulong>();            
            foreach (var bad in serGuild.BadUsers)
                ids.Add(bad.Item1);
            int index = ids.IndexOf(user.Id);

            if (index >= 0 && warns > 0)
                serGuild.BadUsers[index] = (user.Id, warns);            
            else
                serGuild.BadUsers.Add((user.Id, warns));

            FilesProvider.RefreshGuild(serGuild);
        }

        public void PlusWarns(IUser user, int count)
        {
            var serGuild = SerializableGuild;
            List<ulong> ids = new List<ulong>();
            foreach (var bad in serGuild.BadUsers)
                ids.Add(bad.Item1);
            int index = ids.IndexOf(user.Id);

            if (index >= 0)
                serGuild.BadUsers[index] = (user.Id, serGuild.BadUsers[index].Item2 + count);
            else
                serGuild.BadUsers.Add((user.Id, count));

            FilesProvider.RefreshGuild(serGuild);
        }

        public void MinusWarns(IUser user, int count)
        {
            var serGuild = SerializableGuild;
            List<ulong> ids = new List<ulong>();
            foreach (var bad in serGuild.BadUsers)
                ids.Add(bad.Item1);
            int index = ids.IndexOf(user.Id);

            if (index >= 0)
                if(serGuild.BadUsers[index].Item2 - count >= 0)
                    serGuild.BadUsers[index] = (user.Id, serGuild.BadUsers[index].Item2 - count);
            else
                serGuild.BadUsers.Add((user.Id, count));

            FilesProvider.RefreshGuild(serGuild);
        }

        public (ulong, int) GetBadUser(IUser user)
        {
            var serGuild = SerializableGuild;
            List<ulong> ids = new List<ulong>();
            foreach (var bad in serGuild.BadUsers)
                ids.Add(bad.Item1);
            int index = ids.IndexOf(user.Id);
            if (index >= 0)
                return (serGuild.BadUsers[index].Item1, serGuild.BadUsers[index].Item2);
            else
                return (0, 0);
        }

        public bool ExistChannelByName(SocketGuildChannel channel)
        {
            string name = channel.Name;
            var guild = channel.Guild;
            bool val = false;

            foreach (var ch in guild.Channels)
            {
                if (ch.Name == name)
                { 
                    val = true;
                    continue;
                }                 
            }

            return val;
        }
    }
}