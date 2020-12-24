using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Victoria;

namespace DiscordBot.MusicOperations
{
    public class LavaOperations
    {
        private readonly LavaNode LavaNode;

        public Hashtable GuildsPlayers { get; set; } = new Hashtable();

        public LavaOperations(LavaNode lavaNode, DiscordSocketClient client)
        {
            LavaNode = lavaNode;
            foreach (var guild in client.Guilds)
                GuildsPlayers.Add(guild, null);
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

        public async Task AddTrackAsync(SocketGuildUser user, string[] query, SocketTextChannel contextChannel)
        {
            if (query == null)
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

                var search = Uri.IsWellFormedUriString(Query, UriKind.Absolute) ? await LavaNode.SearchAsync(Query) : await LavaNode.SearchYouTubeAsync(Query);
                var track = search.Tracks.FirstOrDefault();
                player.Queue.Enqueue(track);
                if (player.Queue.Count == 1)
                    await player.PlayAsync(track);
                await SendPlayer(player);                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);                
            }
        }

        public async Task<Embed> SetVolumeAsync(SocketGuildUser user, ushort vol, SocketTextChannel contextChannel)
        {
            try
            {
                var hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);
                if (!hasPlayer)
                    return CreateErrorReplyEmbed(ErrorType.NoTrack);
                await player.UpdateVolumeAsync(vol);
                await SendPlayer(player);
                return new EmbedBuilder
                {
                    Title = $"Громкость установлена до {vol}",
                    Color = Color.Blue
                }.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return CreateErrorReplyEmbed(ErrorType.Exception);
            }
        }

        private Embed CreateErrorReplyEmbed(ErrorType error)
        {
            return error switch
            {
                ErrorType.Exception => new EmbedBuilder
                {
                    Title = "Было вызвано исключение во время выполнения команды",
                    Color = Color.Red
                }.Build(),
                ErrorType.NotConnected => new EmbedBuilder
                {
                    Title = "Ты не подключен к каналу",
                    Color = Color.Red
                }.Build(),
                ErrorType.BotNotConnected => new EmbedBuilder
                {
                    Title = "Я не подключен к каналу",
                    Color = Color.Red
                }.Build(),
                ErrorType.NoTrack => new EmbedBuilder
                {
                    Title = "Нет трека в воспроизведении",
                    Color = Color.Red
                }.Build(),
                ErrorType.NoName => new EmbedBuilder 
                {
                    Title = "Ты не указал ссылку или название на видео или трек",
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
                Color = Color.Blue,
                Author = new EmbedAuthorBuilder { Name = player.Track.Author, Url = player.Track.Url }
            }.Build());
            

            await mess.AddReactionsAsync(new Emoji[]
            {
                new Emoji("⏯️"), 
                new Emoji("⏹"), 
                new Emoji("⏮"),
                new Emoji("⏭")
            });

            GuildsPlayers[guild] = mess;            
        }

        private enum ErrorType { Exception, NotConnected, BotNotConnected, NoTrack, NoName }
    }
}
