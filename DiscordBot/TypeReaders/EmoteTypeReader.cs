//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace DiscordBot.TypeReaders
{
    public class EmojiTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            try
            {
                if (new Emoji(input) != null)
                    return Task.FromResult(TypeReaderResult.FromSuccess(new Emoji(input)));

                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Can't parse this emote"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(TypeReaderResult.FromError(ex));
            }
        }
    }
}
