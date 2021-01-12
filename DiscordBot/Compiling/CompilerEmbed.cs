using System.Collections.Generic;
using Discord;

namespace DiscordBot.Compiling
{
    public class CompilerEmbed
    {
        public List<ErrorField> Errors { get; set; } = new List<ErrorField>();
        public List<WarningField> Warnings { get; set; } = new List<WarningField>();

        public Embed Build()
        {
            Color colorOfEmbed;
            string errors = null;
            string warnings = null;
            bool err = false;
            bool warn = false;
            if (Errors.Count > 0)
                err = true;
            if (Warnings.Count > 0)
                warn = true;

            if (err && warn || err)
                colorOfEmbed = Color.Red;
            else if (warn)
                colorOfEmbed = Color.Orange;
            else
                colorOfEmbed = Color.Green;

            foreach (var error in Errors)
                errors += $"\n{error.Message}";
            foreach (var warning in Warnings)
                warnings += $"\n{warning.Message}";

            return new EmbedBuilder
            {
                Title = "Результат компиляции",               
                Description = errors == null && warnings == null ? "Компиляция прошла без ошибок." : $"{errors}\n{warnings}",
                Color = colorOfEmbed
            }.Build();
        }
    }
}
