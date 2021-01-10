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
        private readonly DiscordSocketClient Client;

        private delegate Task UpdatePlayerHandler(IGuild guild);
        private event UpdatePlayerHandler UpdatePlayer;
        
        public Dictionary<IGuild, IUserMessage> PlayersMessagesCollection = new Dictionary<IGuild, IUserMessage>();

        public LavaOperations(LavaNode lavaNode, DiscordSocketClient client)
        {
            Client = client;
            LavaNode = lavaNode;
            foreach (var guild in client.Guilds)
                PlayersMessagesCollection.Add(guild, null);
            UpdatePlayer += LavaOperations_UpdatePlayer;
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
                ushort conv = (ushort)(vol * ushort.MaxValue / 100);

                var hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);
                if (!hasPlayer)
                    await contextChannel.SendMessageAsync(embed: CreateErrorReplyEmbed(ErrorType.NoTrack));
                await player.UpdateVolumeAsync(conv);                
                await contextChannel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Description = $"Текущая громкость: {vol}%",
                    Color = Color.Blue
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

        private async Task SendPlayer(LavaPlayer player)
        {
            var guild = player.VoiceChannel.Guild;
            int h = player.Track.Position.Hours;
            int m = player.Track.Position.Minutes;
            int s = player.Track.Position.Seconds;
            int part = s / player.Track.Duration.Seconds * 100;
            var mess = await (player.TextChannel ?? (guild as SocketGuild).DefaultChannel).SendMessageAsync(embed: new EmbedBuilder
            {
                Title = $"Плеер сервера {guild.Name}",
                Description = player.Track.Title,
                Color = Color.Blue,
                Author = new EmbedAuthorBuilder { Name = player.Track.Author, Url = player.Track.Url },
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{(h < 10 ? $"0{h}" : h)}:{(m < 10 ? $"0{m}" : m)}:{(s < 10 ? $"0{s}" : s)} {(part >= 0 && part < 0.2 ? "◯" : "-")}{(part >= 0.2 && part < 0.3 ? "◯" : "-")}{(part >= 0.3 && part < 0.4 ? "◯" : "-")}{(part >= 0.4 && part < 0.5 ? "◯" : "-")}{(part >= 0.5 && part < 0.6 ? "◯" : "-")}{(part >= 0.6 && part < 0.7 ? "◯" : "-")}{(part >= 0.7 && part < 0.8 ? "◯" : "-")}{(part >= 0.8 && part < 0.9 ? "◯" : "-")}{(part >= 0.9 && part < 1 ? "◯" : "-")}{(part >= 1 ? "◯" : "-")}"
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

        private async Task LavaOperations_UpdatePlayer(IGuild guild)
        {
            var message = PlayersMessagesCollection[guild];
            var player = LavaNode.GetPlayer(guild);
            int h = player.Track.Position.Hours;
            int m = player.Track.Position.Minutes;
            int s = player.Track.Position.Seconds;
            double st = player.Track.Position.TotalSeconds;
            double part = st / player.Track.Duration.TotalSeconds;
            await message.ModifyAsync(x => x.Embed = new Optional<Embed>(new EmbedBuilder
            {
                Title = $"Плеер сервера {guild.Name}",
                Description = player.Track.Title,                
                Color = Color.Blue,
                Author = new EmbedAuthorBuilder { Name = player.Track.Author, Url = player.Track.Url },
                Footer = new EmbedFooterBuilder
                {
                    Text = $"{(h < 10 ? $"0{h}" : h)}:{(m < 10 ? $"0{m}" : m)}:{(s < 10 ? $"0{s}" : s)} {(part >= 0 && part < 0.2 ? "◯" : "-")}{(part >= 0.2 && part < 0.3 ? "◯" : "-")}{(part >= 0.3 && part < 0.4 ? "◯" : "-")}{(part >= 0.4 && part < 0.5 ? "◯" : "-")}{(part >= 0.5 && part < 0.6 ? "◯" : "-")}{(part >= 0.6 && part < 0.7 ? "◯" : "-")}{(part >= 0.7 && part < 0.8 ? "◯" : "-")}{(part >= 0.8 && part < 0.9 ? "◯" : "-")}{(part >= 0.9 && part < 1 ? "◯" : "-")}{(part >= 1 ? "◯" : "-")}"
                }
            }.Build()));
        }

        private async void UpdateTimeThreadTask(object guild)
        {
            SocketGuild Guild = (SocketGuild)guild;
            var player = LavaNode.GetPlayer(Guild);                        
            while (player.Track != null)
            {
                await UpdatePlayer.Invoke(Guild);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }

        private enum ErrorType { Exception, NotConnected, BotNotConnected, NoTrack, NoName }
    }
}
