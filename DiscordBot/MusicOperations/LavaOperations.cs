using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace DiscordBot.MusicOperations
{
    public class LavaOperations
    {
        private readonly LavaNode LavaNode;

        public LavaOperations(LavaNode lavaNode)
        {
            LavaNode = lavaNode;
        }

        public async Task<Embed> JoinAsync(SocketGuildUser user)
        {
            var voiceState = user.VoiceState;            
            var channel = voiceState.Value.VoiceChannel;

            if (channel == null)//Если пользователь не подключен
                return CreateErrorReplyEmbed(ErrorType.NotConnected);

            try
            {
                await LavaNode.JoinAsync(channel);
                return new EmbedBuilder
                {
                    Title = $"Подключен к каналу {channel.Name}",
                    Color = Color.Blue
                }.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return CreateErrorReplyEmbed(ErrorType.Exception);
            }
        }

        public async Task<Embed> LeaveAsync(SocketGuildUser user)
        {
            var voiceState = user.VoiceState;
            var channel = voiceState.Value.VoiceChannel;
            var player = LavaNode.GetPlayer(user.Guild);

            if (player.VoiceChannel == null)
                return CreateErrorReplyEmbed(ErrorType.BotNotConnected);

            if (player.VoiceChannel != channel)
                return new EmbedBuilder
                {
                    Title = "Ты не можешь меня отключить из другого канала",
                    Color = Color.Red
                }.Build();

            try
            {                
                await LavaNode.LeaveAsync(player.VoiceChannel);
                return new EmbedBuilder
                {
                    Title = $"Покинул канал {channel.Name}",
                    Color = Color.Blue
                }.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return CreateErrorReplyEmbed(ErrorType.Exception);
            }
        }

        public async Task<Embed> PlayTrackAsync(SocketGuildUser user, string[] query)
        {
            if (query == null)
                return CreateErrorReplyEmbed(ErrorType.NoName);
            string Query = null;

            for (int i = 0; i < query.Length; i++)            
                Query = i == 0 ? $"{query[i]}" : $" {query[i]}";            

            try
            {
                bool hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);
                if (user.VoiceChannel == null)
                    return CreateErrorReplyEmbed(ErrorType.NotConnected);

                if (!hasPlayer)                
                    player = await LavaNode.JoinAsync(user.VoiceChannel);
                
                var search = Uri.IsWellFormedUriString(Query, UriKind.Absolute) ? await LavaNode.SearchAsync(Query) : await LavaNode.SearchYouTubeAsync(Query);
                var track = search.Tracks.FirstOrDefault();
                await player.PlayAsync(track);
                return new EmbedBuilder
                {
                    Title = $"Играет {track.Title}",
                    Color = Color.Blue
                }.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return CreateErrorReplyEmbed(ErrorType.Exception);
            }
        }

        public async Task<Embed> StopTrackAsync(SocketGuildUser user)
        {
            try
            {
                bool hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);
                if (user.VoiceChannel == null)
                    return CreateErrorReplyEmbed(ErrorType.NotConnected);
                if (!hasPlayer)
                    return CreateErrorReplyEmbed(ErrorType.NoTrack);
                if(player.Track == null)
                    return CreateErrorReplyEmbed(ErrorType.NoTrack);

                await player.StopAsync();
                return new EmbedBuilder
                {
                    Title = $"Трек {player.Track.Title} остановлен",
                    Color = Color.Blue
                }.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return CreateErrorReplyEmbed(ErrorType.Exception);
            }
        }

        public async Task<Embed> PauseTrackAsync(SocketGuildUser user)
        {
            try
            {
                var hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);

                if (user.VoiceChannel == null)
                    return CreateErrorReplyEmbed(ErrorType.NotConnected);
                if (!hasPlayer)
                    return CreateErrorReplyEmbed(ErrorType.NoTrack);
                if (player.Track == null)
                    return CreateErrorReplyEmbed(ErrorType.NoTrack);

                await player.PauseAsync();
                return new EmbedBuilder
                {
                    Title = $"Трек {player.Track.Title} приостановлен",
                    Color = Color.Blue
                }.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return CreateErrorReplyEmbed(ErrorType.Exception);
            }
        }

        public async Task<Embed> PlayTrackAsync(SocketGuildUser user)
        {
            try
            {
                var hasPlayer = LavaNode.TryGetPlayer(user.Guild, out LavaPlayer player);

                if (user.VoiceChannel == null)
                    return CreateErrorReplyEmbed(ErrorType.NotConnected);
                if (!hasPlayer)
                    return CreateErrorReplyEmbed(ErrorType.NoTrack);
                if (player.Track == null)
                    return CreateErrorReplyEmbed(ErrorType.NoTrack);

                await player.ResumeAsync();
                return new EmbedBuilder
                {
                    Title = $"Трек {player.Track.Title} воспроизведен",
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

        private enum ErrorType { Exception, NotConnected, BotNotConnected, NoTrack, NoName }
    }
}
