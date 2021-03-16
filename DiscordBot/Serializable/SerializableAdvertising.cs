using Discord;
using DiscordBot.Providers;
using System;

namespace DiscordBot.Serializable
{
    public class SerializableAdvertising
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string AdvColor { get; set; }

        public Embed BuildAdvertise() => new EmbedBuilder
        {
            Title = Title,
            Description = Description,
            Color = ColorProvider.GetColorFromName(AdvColor),
            Footer = new EmbedFooterBuilder
            {
                Text = DateTime.Now.ToShortDateString()
            }
        }.Build();
    }
}
