using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Victoria;

namespace DiscordBot.MusicOperations
{
    public class LavaOperations
    {
        private readonly LavaNode LavaNode;        
        
        public List<ValueTuple<SocketGuild, SocketUserMessage>> GuildsPlayers = new List<(SocketGuild, SocketUserMessage)>();

        private readonly Thread ProgressBarsUpdaterThread;

        public LavaOperations(LavaNode lavaNode, DiscordSocketClient client)
        {
            LavaNode = lavaNode;
            foreach (var guild in client.Guilds)
                GuildsPlayers.Add(new ValueTuple<SocketGuild, SocketUserMessage>(guild, null));

            ProgressBarsUpdaterThread = new Thread(UpdateProgressBars);
            ProgressBarsUpdaterThread.Start();
        }
        
        public async Task JoinAsync(SocketGuildUser user, SocketTextChannel contextChannel)
        {
            var voiceState = user.VoiceState;            
            var channel = voiceState.Value.VoiceChannel;

            if (channel == null)
                await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NotConnected));

            try
            {
                await LavaNode.JoinAsync(channel);
                await contextChannel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = $"Подключен к каналу {channel.Name}",
                    Color = Color.Blue
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
                    await LavaNode.LeaveAsync(user.Guild.CurrentUser.VoiceChannel);
                    await contextChannel.SendMessageAsync(embed: new EmbedBuilder
                    {
                        Title = $"Покинул канал {user.Guild.CurrentUser.VoiceChannel.Name}",
                        Color = Color.Blue
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
                    if(player.VoiceState.VoiceChannel == null)
                        await LavaNode.JoinAsync(user.VoiceChannel);

                var search = Uri.IsWellFormedUriString(Query, UriKind.Absolute) ? await LavaNode.SearchAsync(Query) : await LavaNode.SearchYouTubeAsync(Query);
                var track = search.Tracks.FirstOrDefault();
                await player.PlayAsync(track);

                await SendPlayer(player);                
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
                await SendPlayer(player);                
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
                await SendPlayer(player);                
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
                await SendPlayer(player);                
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
                var hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);
                if (!hasPlayer)
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NoTrack));
                await player.UpdateVolumeAsync(vol);                
                await contextChannel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Description = $"Громкость установлена до {vol}",
                    Color = Color.Blue
                }.Build());
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

        private async Task SendPlayer(LavaPlayer player)
        {
            var guild = player.VoiceChannel.Guild;            

            var mess = await (player.TextChannel ?? (guild as SocketGuild).DefaultChannel).SendMessageAsync(embed: new EmbedBuilder
            { 
                Title = $"Плеер сервера {guild.Name}",                
                Description = player.Track.Title,
                Fields = new List<EmbedFieldBuilder>
                { 
                    new EmbedFieldBuilder
                    { 
                        Name = "Ползунок",
                        Value = "----------"
                    }
                },
                Color = Color.Blue,
                Author = new EmbedAuthorBuilder { Name = player.Track.Author, Url = player.Track.Url }                
            }.Build());
           
            await mess.AddReactionsAsync(new Emoji[]
            {
                new Emoji("⏯️"),
                new Emoji("⏹"),                                
                new Emoji("➖"),
                new Emoji("➕"),
                new Emoji("❌")
            });

            ValueTuple<SocketGuild, SocketUserMessage> playerMess = default;

            foreach (var pl in GuildsPlayers)
            {
                if (pl.Item1 == guild)
                    playerMess = pl;
            }

            GuildsPlayers.Remove(playerMess);
            playerMess.Item2 = mess as SocketUserMessage;
            GuildsPlayers.Add(playerMess);
        }

        private async void UpdateProgressBars()
        {
            while (true)
            {
                if (GuildsPlayers.Count > 0)
                {
                    foreach (var message in GuildsPlayers)
                    {
                        var mess = message.Item2;
                        if (mess != null)
                        {
                            var player = LavaNode.GetPlayer((mess.Author as SocketGuildUser).Guild);
                            var guild = (mess.Author as SocketGuildUser).Guild;
                            double per = (double)player.Track.Position.Seconds / 10 / player.Track.Duration.Seconds;
                            if (mess.Embeds != null)
                                await mess.ModifyAsync(x => x.Embed = new Optional<Embed>(new EmbedBuilder
                                {
                                    Title = $"Плеер сервера {guild.Name}",
                                    Description = player.Track.Title,
                                    Fields = new List<EmbedFieldBuilder>
                                {
                                    new EmbedFieldBuilder
                                    {
                                        Value = $"{ (per >= 0.1 ? "#" : "-") }{ (per >= 0.2 ? "#" : "-") }{ (per >= 0.3 ? "#" : "-") }{ (per >= 0.4 ? "#" : "-") }{ (per >= 0.5 ? "#" : "-") }{ (per >= 0.6 ? "#" : "-") }{ (per >= 0.7 ? "#" : "-") }{ (per >= 0.8 ? "#" : "-") }{ (per >= 0.9 ? "#" : "-") }{ (per >= 1 ? "#" : "-") }"
                                    }
                                },
                                    Color = Color.Blue,
                                    Author = new EmbedAuthorBuilder { Name = player.Track.Author, Url = player.Track.Url }
                                }.Build()));
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }                        
                }
            }            
        }

        private enum ErrorType { Exception, NotConnected, BotNotConnected, NoTrack, NoName }
    }
}
