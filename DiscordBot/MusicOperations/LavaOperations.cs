﻿//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
using Discord.Net;
using Discord.WebSocket;
using DiscordBot.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Victoria;

namespace DiscordBot.MusicOperations
{
    public class LavaOperations
    {
        public readonly LavaNode LavaNode;

        private delegate Task UpdatePlayerHandler(IGuild guild);
        private event UpdatePlayerHandler UpdatePlayer;

        public Dictionary<IGuild, IUserMessage> PlayersMessagesCollection = new Dictionary<IGuild, IUserMessage>();

        public LavaOperations(LavaNode lavaNode, DiscordSocketClient client)
        {
            LavaNode = lavaNode;
            foreach (var guild in client.Guilds)
                PlayersMessagesCollection.Add(guild, null);
            UpdatePlayer += LavaOperations_UpdatePlayer;
        }

        public async Task JoinAsync(SocketGuildUser user, SocketTextChannel contextChannel)
        {
            var voiceState = user.VoiceState;
            var channel = voiceState.Value.VoiceChannel;
            var serGuild = FilesProvider.GetGuild(user.Guild);

            if (channel == null)
                await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NotConnected));

            try
            {
                if(LavaNode.GetPlayer(contextChannel.Guild).VoiceChannel.Id != channel.Id)
                    await LavaNode.LeaveAsync(LavaNode.GetPlayer(contextChannel.Guild).VoiceChannel);
                await LavaNode.JoinAsync(channel);
                await contextChannel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = $"Подключен к каналу {channel.Name}",
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                }.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task LeaveAsync(SocketGuildUser user, SocketTextChannel contextChannel)
        {
            try
            {
                if (user.Guild.CurrentUser.VoiceChannel != null)
                {
                    var serGuild = FilesProvider.GetGuild(user.Guild);
                    await LavaNode.LeaveAsync(user.Guild.CurrentUser.VoiceChannel);
                    await contextChannel.SendMessageAsync(embed: new EmbedBuilder
                    {
                        Title = $"Покинул канал {user.Guild.CurrentUser.VoiceChannel.Name}",
                        Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                    }.Build());
                }
                else
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.BotNotConnected));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task PlayTrackAsync(SocketGuildUser user, string[] query, SocketTextChannel contextChannel)
        {
            if (user.VoiceChannel == null)
            {
                await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NoName));
                return;
            }
            string Query = null;

            for (int i = 0; i < query.Length; i++)
                Query += i == 0 ? $"{query[i]}" : $" {query[i]}";

            try
            {
                bool hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);
                if (user.VoiceChannel == null)
                {
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NotConnected));
                    return;
                }

                if (!hasPlayer)
                    player = await LavaNode.JoinAsync(user.VoiceChannel);
                else
                    if (player.VoiceState.VoiceChannel == null)
                    await LavaNode.JoinAsync(user.VoiceChannel);

                var search = Uri.IsWellFormedUriString(Query, UriKind.Absolute) ? await LavaNode.SearchAsync(Query) : await LavaNode.SearchYouTubeAsync(Query);
                var track = search.Tracks.FirstOrDefault();
                await player.PlayAsync(track);

                await SendPlayer(player, contextChannel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task StopTrackAsync(SocketGuildUser user, SocketTextChannel contextChannel)
        {
            try
            {
                bool hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);
                if (user.VoiceChannel == null)
                {
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NotConnected));
                    return;
                }

                if (!hasPlayer)
                {
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NoTrack));
                    return;
                }

                if (player.Track == null)
                {
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NoTrack));
                    return;
                }

                await player.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task PauseTrackAsync(SocketGuildUser user, SocketTextChannel contextChannel)
        {
            try
            {
                var hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);

                if (user.VoiceChannel == null)
                {
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NotConnected));
                    return;
                }

                if (!hasPlayer)
                {
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NoTrack));
                    return;
                }

                if (player.Track == null)
                {
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NoTrack));
                    return;
                }

                await player.PauseAsync();
                await SendPlayer(player, contextChannel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task ResumeTrackAsync(SocketGuildUser user, SocketTextChannel contextChannel)
        {
            try
            {
                var hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);

                if (user.VoiceChannel == null)
                {
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NotConnected));
                    return;
                }

                if (!hasPlayer)
                {
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NoTrack));
                    return;
                }

                if (player.Track == null)
                {
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NoTrack));
                    return;
                }

                await player.ResumeAsync();
                await SendPlayer(player, contextChannel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task SetVolumeAsync(SocketGuildUser user, ushort vol, SocketTextChannel contextChannel)
        {
            try
            {
                var serGuild = FilesProvider.GetGuild(user.Guild);
                ushort conv = (ushort)(vol * ushort.MaxValue / 100);

                var hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);
                if (!hasPlayer)
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NoTrack));
                await player.UpdateVolumeAsync(conv);
                await contextChannel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Description = $"Текущая громкость: {vol}%",
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild)
                }.Build());
                await UpdatePlayer.Invoke(user.Guild);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private Embed CreateErrorReplyEmbed(ErrorType error)
        {
            return error switch
            {
                ErrorType.Exception => new EmbedBuilder
                {
                    Description = "Было вызвано исключение во время выполнения команды",
                    Color = Color.Red
                }.Build(),
                ErrorType.NotConnected => new EmbedBuilder
                {
                    Description = "Ты не подключен к каналу",
                    Color = Color.Red
                }.Build(),
                ErrorType.BotNotConnected => new EmbedBuilder
                {
                    Description = "Я не подключен к каналу",
                    Color = Color.Red
                }.Build(),
                ErrorType.NoTrack => new EmbedBuilder
                {
                    Description = "Нет трека в воспроизведении",
                    Color = Color.Red
                }.Build(),
                ErrorType.NoName => new EmbedBuilder
                {
                    Description = "Ты не указал ссылку или название на видео или трек",
                    Color = Color.Red
                }.Build(),
                _ => null,
            };
        }

        private async Task SendPlayer(LavaPlayer player, SocketTextChannel textChannel)
        {
            try
            {
                var guild = player.VoiceChannel.Guild;
                var serGuild = FilesProvider.GetGuild(guild);
                int h = player.Track.Position.Hours;
                int m = player.Track.Position.Minutes;
                int s = player.Track.Position.Seconds;
                int part = s / player.Track.Duration.Seconds * 100;
                var mess = await textChannel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = $"Плеер сервера {guild.Name}",
                    Description = player.Track.Title,
                    Color = ColorProvider.GetColorForCurrentGuild(serGuild),
                    Author = new EmbedAuthorBuilder { Name = player.Track.Author, Url = player.Track.Url },
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"{(h < 10 ? $"0{h}" : h.ToString())}:{(m < 10 ? $"0{m}" : m.ToString())}:{(s < 10 ? $"0{s}" : s.ToString())} {(part >= 0 && part < 0.2 ? "◯" : "─")}{(part >= 0.2 && part < 0.3 ? "◯" : "─")}{(part >= 0.3 && part < 0.4 ? "◯" : "─")}{(part >= 0.4 && part < 0.5 ? "◯" : "─")}{(part >= 0.5 && part < 0.6 ? "◯" : "─")}{(part >= 0.6 && part < 0.7 ? "◯" : "─")}{(part >= 0.7 && part < 0.8 ? "◯" : "─")}{(part >= 0.8 && part < 0.9 ? "◯" : "─")}{(part >= 0.9 && part < 0.95 ? "◯" : "─")}{(part >= 0.95 ? "◯" : "─")}"
                    }
                }.Build());

                await mess.AddReactionsAsync(new Emoji[]
                {
                    new Emoji("⏯️"),
                    new Emoji("⏹"),
                    new Emoji("➖"),
                    new Emoji("➕"),
                    new Emoji("❌")
                });

                PlayersMessagesCollection.Remove(guild);
                PlayersMessagesCollection.Add(guild, mess);
                new Thread(UpdateTimeThreadTask).Start(guild);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
            }
        }

        private async Task LavaOperations_UpdatePlayer(IGuild guild)
        {
            try
            {
                var serGuild = FilesProvider.GetGuild(guild);
                var message = PlayersMessagesCollection[guild];
                var player = LavaNode.GetPlayer(guild);
                int h = player.Track.Position.Hours;
                int m = player.Track.Position.Minutes;
                int s = player.Track.Position.Seconds;
                double st = player.Track.Position.TotalSeconds;
                double part = st / player.Track.Duration.TotalSeconds;
                if (message != null)
                    await message.ModifyAsync(x => x.Embed = new Optional<Embed>(new EmbedBuilder
                    {
                        Title = $"Плеер сервера {guild.Name}",
                        Description = player.Track.Title,
                        Color = ColorProvider.GetColorForCurrentGuild(serGuild),
                        Author = new EmbedAuthorBuilder { Name = player.Track.Author, Url = player.Track.Url },
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"{(h < 10 ? $"0{h}" : h.ToString())}:{(m < 10 ? $"0{m}" : m.ToString())}:{(s < 10 ? $"0{s}" : s.ToString())} {(part >= 0 && part < 0.2 ? "◯" : "─")}{(part >= 0.2 && part < 0.3 ? "◯" : "─")}{(part >= 0.3 && part < 0.4 ? "◯" : "─")}{(part >= 0.4 && part < 0.5 ? "◯" : "─")}{(part >= 0.5 && part < 0.6 ? "◯" : "─")}{(part >= 0.6 && part < 0.7 ? "◯" : "─")}{(part >= 0.7 && part < 0.8 ? "◯" : "─")}{(part >= 0.8 && part < 0.9 ? "◯" : "─")}{(part >= 0.9 && part < 0.95 ? "◯" : "─")}{(part >= 0.95 ? "◯" : "─")}"
                        }
                    }.Build()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
            }
        }

        private async void UpdateTimeThreadTask(object guild)
        {
            try
            {
                SocketGuild Guild = (SocketGuild)guild;
                var hasPlayer = LavaNode.TryGetPlayer(Guild, out LavaPlayer player);
                while (player != null)
                {
                    if (player.Track == null)
                        break;
                    try
                    {
                        var serGuild = FilesProvider.GetGuild(Guild);
                        var message = PlayersMessagesCollection[Guild];
                        int h = player.Track.Position.Hours;
                        int m = player.Track.Position.Minutes;
                        int s = player.Track.Position.Seconds;
                        double st = player.Track.Position.TotalSeconds;
                        double part = st / player.Track.Duration.TotalSeconds;
                        if (message != null)
                            await message.ModifyAsync(x => x.Embed = new Optional<Embed>(new EmbedBuilder
                            {
                                Title = $"Плеер сервера {Guild.Name}",
                                Description = player.Track.Title,
                                Color = ColorProvider.GetColorForCurrentGuild(serGuild),
                                Author = new EmbedAuthorBuilder { Name = player.Track.Author, Url = player.Track.Url },
                                Footer = new EmbedFooterBuilder
                                {
                                    Text = $"{(h < 10 ? $"0{h}" : h.ToString())}:{(m < 10 ? $"0{m}" : m.ToString())}:{(s < 10 ? $"0{s}" : s.ToString())} {(part >= 0 && part < 0.2 ? "◯" : "─")}{(part >= 0.2 && part < 0.3 ? "◯" : "─")}{(part >= 0.3 && part < 0.4 ? "◯" : "─")}{(part >= 0.4 && part < 0.5 ? "◯" : "─")}{(part >= 0.5 && part < 0.6 ? "◯" : "─")}{(part >= 0.6 && part < 0.7 ? "◯" : "─")}{(part >= 0.7 && part < 0.8 ? "◯" : "─")}{(part >= 0.8 && part < 0.9 ? "◯" : "─")}{(part >= 0.9 && part < 0.95 ? "◯" : "─")}{(part >= 0.95 ? "◯" : "─")}"
                                }
                            }.Build()));
                    }
                    catch (HttpException ex)
                    {
                        if (ex.DiscordCode == 50001)
                        {
                            Thread.Sleep(-1);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ex: {ex}");
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex}");
            }
        }

        private enum ErrorType { Exception, NotConnected, BotNotConnected, NoTrack, NoName }
    }
}
