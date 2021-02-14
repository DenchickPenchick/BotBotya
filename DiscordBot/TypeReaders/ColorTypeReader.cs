using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.TypeReaders
{
    public class ColorTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            Color color;

            switch (input.ToLower())
            {
                case "синий":
                    color = Color.Blue;
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
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Can't parse color {input}"));
            }
            return Task.FromResult(TypeReaderResult.FromSuccess(color));
        }
    }
}
