using System;
using System.Collections.Generic;
using Discord;

namespace DiscordBot.Interactivities
{
    public class PaginatorEntity
    {
        public string Title { get; set; }
        public Color Color { get; set; }
        public string ThumbnailUrl { get; set; }
        public IEnumerable<string> Pages { get; set; }

        internal object ToList()
        {
            throw new NotImplementedException();
        }
    }
}
