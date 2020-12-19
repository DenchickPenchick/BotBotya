using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using Console = Colorful.Console;

namespace DiscordBot.Serializable
{
    [Serializable]
    public class SerializableNewsAndPlans
    {
        public bool ShouldSend { get; set; }
        public List<string> News { get; set; } = new List<string>();
        public List<string> Plans { get; set; } = new List<string>();
        public Embed GetNewsAndPlansEmbed(DiscordSocketClient client)
        {
            if (News.Count > 0 && Plans.Count > 0)
            {
                string news = null;
                string plans = null;
                for (int i = 0; i < News.Count; i++)
                    news += $"{i + 1}. {News[i]}\n";
                for (int i = 0; i < Plans.Count; i++)
                    plans += $"{i + 1}. {Plans[i]}\n";
                return new EmbedBuilder
                {
                    Title = "Новости",                    
                    Fields = new List<EmbedFieldBuilder>
                    { 
                        new EmbedFieldBuilder
                        { 
                            Name = "Лог обновлений",
                            Value = news
                        },
                        new EmbedFieldBuilder
                        { 
                            Name = "Планы",
                            Value = plans
                        }
                    },
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = client.CurrentUser.GetAvatarUrl(),
                        Name = client.CurrentUser.Username,
                        Url = "https://botbotya.ru"
                    },
                    Color = Color.Blue                    
                }.Build();
            }
            else
            {
                Console.WriteLine("Can't get news or plans", Color.Red);
                return null;
            }
               
        }
    }
}
