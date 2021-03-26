//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
using DiscordBot.Serializable;
using System;

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
            return ConvertFromHex(serGuild.ColorOfEmbed);
        }

        public static Color GetColorForCurrentGuild(SerializableGuild guild) => ConvertFromHex(guild.ColorOfEmbed);

        public static Color ConvertFromHex(string hex)
        {
            try
            {
                var color = System.Drawing.ColorTranslator.FromHtml(hex);

                if (color.IsEmpty)
                    return new Color(18, 124, 164);

                return new Color(color.R, color.G, color.B);
            }
            catch (Exception)
            {
                return new Color(18, 124, 164);
            }
        }

        public static void SerializeColor(string name, IGuild guild)
        {
            var serGuild = FilesProvider.GetGuild(guild);
            serGuild.ColorOfEmbed = GetColorFromName(name).ToString();
            FilesProvider.RefreshGuild(serGuild);
        }
    }
}
