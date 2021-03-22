//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
using DiscordBot.Serializable;

namespace DiscordBot.Providers
{
    public static class ColorProvider
    {
        public static Color GetColorFromName(string name)
        {
            Color color;

            if (name == null)
                name = default;

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
                case "желтый":
                    color = new Color(255, 255, 0);
                    break;                
                case "жёлтый":
                    color = new Color(255, 255, 0);
                    break;
                case "бирюзовый":
                    color = new Color(0, 0, 255);
                    break;
                case null:
                    return new Color(18, 124, 164);
                default:
                    return new Color(18, 124, 164);
            }

            return color;
        }

        public static Color GetColorForCurrentGuild(IGuild guild)
        {
            var serGuild = FilesProvider.GetGuild(guild);
            return serGuild.EmbedColor;
        }

        public static Color GetColorForCurrentGuild(SerializableGuild guild) => guild.EmbedColor;

        public static void SerializeColor(string name, IGuild guild)
        {
            var serGuild = FilesProvider.GetGuild(guild);
            serGuild.EmbedColor = GetColorFromName(name);
            FilesProvider.RefreshGuild(serGuild);
        }
    }
}
