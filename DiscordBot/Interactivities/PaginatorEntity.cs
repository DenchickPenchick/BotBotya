using System.Collections.Generic;
using System.Linq;
using Discord;

namespace DiscordBot.Interactivities
{
    public class PaginatorEntity
    {
        public string Title { get; set; }
        public Color Color { get; set; }
        public string ThumbnailUrl { get; set; }
        public IEnumerable<string> Pages { get; set; } = new List<string>();

        public void AddPage(string value) 
        {
            var oldValInList = Pages.ToList();

            oldValInList.Add(value);

            Pages = oldValInList;
        }

        public static PaginatorEntity operator +(PaginatorEntity val1, string val2)
        {
            var pagesList = val1.Pages.ToList();
            string prevValue = null;

            if(pagesList.Count > 0)
                prevValue = pagesList.Last();

            if ((prevValue + val2).Length <= 1024 && pagesList.Count > 0)
            {
                pagesList.Remove(pagesList.Last());
                pagesList.Add(prevValue + val2);
            }                        
            else
                pagesList.Add(val2);                            

           val1.Pages = pagesList;

            return val1;
        }
    }
}
