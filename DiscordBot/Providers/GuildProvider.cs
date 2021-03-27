﻿//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
using Discord.WebSocket;
using DiscordBot.Serializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Providers
{
    public class GuildProvider
    {
        public SocketGuild Guild { get; }
        public SerializableGuild SerializableGuild { get => FilesProvider.GetGuild(Guild); }

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
                var botyaEmoji = Emote.Parse("<:botya:806129905849860137>");
                await Guild.DefaultChannel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = $"👋 Спасибо, что пригласили меня на сервер {Guild.Name} 👋",
                    Description = $"Меня зовут {client.CurrentUser.Username}. Я много чего умею! Пропиши `!Хелп`, чтобы узнать мой функционал.\nЕсли возникнут проблемы, тогда можешь задать вопрос на [сервере поддержки](https://discord.gg/p6R4yk7uqK).\n",
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = $"{botyaEmoji} Мой сайт:",
                            Value = "https://botbotya.ru",
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = $"{botyaEmoji} Мой GitHub:",
                            Value = "https://github.com/denvot/BotBotya",
                            IsInline = true
                        }
                    },
                    Color = Color.Blue
                }.Build());
            }
            catch (Exception ex)
            {
                LogsProvider.ExceptionLog(ex);
            }
        }

        public void SetWarns(IUser user, int warns)
        {
            var serGuild = SerializableGuild;
            List<ulong> ids = serGuild.BadUsers.Select(x => x.Item1).ToList();
            int index = ids.IndexOf(user.Id);

            if (index >= 0 && warns >= 0)
                serGuild.BadUsers[index] = (user.Id, warns);
            else if (warns >= 0)
                serGuild.BadUsers.Add((user.Id, warns));
            else if (index >= 0 && warns < 0)
                serGuild.BadUsers[index] = (user.Id, 0);

            FilesProvider.RefreshGuild(serGuild);
        }

        public void PlusWarns(IUser user, int count)
        {
            var serGuild = SerializableGuild;
            List<ulong> ids = serGuild.BadUsers.Select(x => x.Item1).ToList();
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
            List<ulong> ids = serGuild.BadUsers.Select(x => x.Item1).ToList();
            int index = ids.IndexOf(user.Id);

            if (index >= 0)
                if (serGuild.BadUsers[index].Item2 - count >= 0)
                    serGuild.BadUsers[index] = (user.Id, serGuild.BadUsers[index].Item2 - count);
                else
                    serGuild.BadUsers.Add((user.Id, count));

            FilesProvider.RefreshGuild(serGuild);
        }

        public (ulong, int) GetBadUser(IUser user)
        {
            var serGuild = SerializableGuild;
            List<ulong> ids = serGuild.BadUsers.Select(x => x.Item1).ToList();

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