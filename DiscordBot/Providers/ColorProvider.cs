using Discord;
using DiscordBot.Serializable;

namespace DiscordBot.Providers
{
    public static class ColorProvider
    {
        public static Color GetColorFromName(string name)
        {
            Color color;

            switch (name.ToLower())
            {
                case "синий":
                    color = new Color(18, 124, 164);
                    break;
                case "красный":
                    color = Color.Red;
                    break;
                case "зеленый":
                    color = Color.Green;
                    break;
                case "фиолетовый":
                    color = Color.Purple;
                    break;
                case "оранжевый":
                    color = Color.Orange;
                    break;
                case "серый":
                    color = Color.LightGrey;
                    break;
                default:
                    return new Color(18, 124, 164);
            }

            return color;
        }

        public static Color GetColorForCurrentGuild(IGuild guild)
        {
            var serGuild = FilesProvider.GetGuild(guild);
            return GetColorFromName(serGuild.EmbedColor);
        }

        public static Color GetColorForCurrentGuild(SerializableGuild guild) => GetColorFromName(guild.EmbedColor);

        public static void SerializeColor(string name, IGuild guild)
        {
            var serGuild = FilesProvider.GetGuild(guild);
            serGuild.EmbedColor = name;
            FilesProvider.RefreshGuild(serGuild);
        }
    }
}
