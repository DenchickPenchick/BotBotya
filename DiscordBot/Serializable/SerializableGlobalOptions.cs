using System.Collections.Generic;
using DiscordBot.Models;

namespace DiscordBot.Serializable
{
    public class SerializableGlobalOptions
    {
        public List<WordModel> GlobalBadWords { get; set; } = new List<WordModel>();
    }
}
