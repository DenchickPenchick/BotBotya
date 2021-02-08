using Discord.WebSocket;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DiscordBot.DiscordBotListsServices
{
    public class DiscordBotListsService
    {
        private DiscordSocketClient Client { get; set; }

        public DiscordBotListsService(DiscordSocketClient client)
        {
            Client = client;
        }

        public async Task RunDiscordBotsListService(string token)
        {
            string url = $"https://api.server-discord.com/v2/bots/{Client.CurrentUser.Id}/stats?shards=0&servers={Client.Guilds.Count}";
            WebRequest request = WebRequest.Create(url);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add(HttpRequestHeader.Authorization, token);            

            Console.WriteLine($"HEADERS: {request.Headers}");

            WebResponse response = await request.GetResponseAsync();

            string answer = null;

            using (Stream s = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(s))
                {
                    answer = await reader.ReadToEndAsync();
                }
            }

            response.Close();

            Console.WriteLine(answer);
        }
    }
}
