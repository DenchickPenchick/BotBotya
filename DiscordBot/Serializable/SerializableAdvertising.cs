using Discord;
using DiscordBot.Providers;
using System;
using System.Collections.Generic;

namespace DiscordBot.Serializable
{
    public class SerializableAdvertising
    {
        public string Title { get; set; } = null;
        public string Description { get; set; } = null;
        public Color AdvColor { get; set; } = new Color(18, 124, 164);
        public string ThumbnailUrl { get; set; } = "g";
        public string ImageUrl { get; set; } = "g";
        public string InviteUrl { get; set; } = "NONE";

        public Embed BuildAdvertise(IGuild guild) => new EmbedBuilder
        {
            Title = Title,
            Description = Description,
            Color = AdvColor,
            Fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    Name = "Зайти:",
                    Value = InviteUrl
                }
            },
            ThumbnailUrl = ThumbnailUrl.ToLower() == "g" ? guild.IconUrl : ThumbnailUrl,
            ImageUrl = ImageUrl.ToLower() == "g" ? guild.IconUrl : ImageUrl,
            Footer = new EmbedFooterBuilder
            {
                Text = DateTime.Now.ToShortDateString()
            }
        }.Build();

        public EmbedBuilder GetBuilder(IGuild guild) => new()
        {
            Title = Title,
            Description = Description,
            Color = AdvColor,
            Fields = new List<EmbedFieldBuilder>
            { 
                new EmbedFieldBuilder
                { 
                    Name = "Зайти:",
                    Value = InviteUrl
                }
            },
            ThumbnailUrl = ThumbnailUrl.ToLower() == "g" ? guild.IconUrl : ThumbnailUrl,
            ImageUrl = ImageUrl.ToLower() == "g" ? guild.IconUrl : ImageUrl,
            Footer = new EmbedFooterBuilder
            {
                Text = DateTime.Now.ToShortDateString()
            }
        };
    }
}
