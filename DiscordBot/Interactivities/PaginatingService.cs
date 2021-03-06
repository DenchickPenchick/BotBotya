﻿//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Providers;

namespace DiscordBot.Interactivities
{
    public class PaginatingService
    {                
        public PaginatingService(DiscordSocketClient client, int maxMessages)
        {
            client.ReactionAdded += Client_ReactionAdded;
            MaxMessages = maxMessages;
        }
        
        private int MaxMessages { get; } = default;

        private readonly List<(RestUserMessage, PaginatorEntity, int)> Messages = new List<(RestUserMessage, PaginatorEntity, int)>();

        public async Task SendPaginatedMessageAsync(PaginatorEntity paginatorEntity, SocketCommandContext context, int num = 0)
        {            
            var mess = await context.Channel.SendMessageAsync(embed: new EmbedBuilder
            { 
                Title = paginatorEntity.Title,
                ThumbnailUrl = paginatorEntity.ThumbnailUrl,
                Description = paginatorEntity.Pages.ToList()[num],
                Color = paginatorEntity.Color == default ? ColorProvider.GetColorForCurrentGuild(context.Guild) : paginatorEntity.Color
            }.Build());

            await mess.AddReactionsAsync(new List<IEmote>
            {
                new Emoji("⏪"),
                new Emoji("◀️"),
                new Emoji("⏹"),
                new Emoji("▶️"),
                new Emoji("⏩")
            }.ToArray());

            Messages.Add((mess, paginatorEntity, num));

            if (Messages.Count > MaxMessages && MaxMessages != default)
                Messages.RemoveAt(0);
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg1.HasValue && !arg3.User.Value.IsBot)
            {
                var message = arg1.Value;
                
                if (Messages.Select(x => x.Item1.Id).Contains(message.Id))
                {
                    var ourTuple = Messages.Where(x => x.Item1.Id == message.Id).FirstOrDefault();
                    await message.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                    switch (arg3.Emote.Name)
                    {
                        case "⏩":
                            await ChangePageAsync(message, ourTuple.Item2.Pages.Count());
                            break;
                        case "⏪":
                            await ChangePageAsync(message, 0);
                            break;
                        case "▶️":
                            await ChangePageAsync(message, 1);
                            break;
                        case "◀️":
                            await ChangePageAsync(message, -1);
                            break;
                        case "⏹":
                            await message.DeleteAsync();
                            Messages.RemoveAt(Messages.Select(x => x.Item1.Id).ToList().IndexOf(message.Id));
                            break;
                    }
                }
            }             
        }

        //Вперед: index > 0
        //Назад: index < 0        
        private async Task ChangePageAsync(IUserMessage message, int index)
        {
            var ourTuple = Messages.Where(x => x.Item1.Id == message.Id).FirstOrDefault();

            var mess = ourTuple.Item1;
            var paginatorEntity = ourTuple.Item2;

            if (index > 0)
                Messages[Messages.IndexOf(ourTuple)] = new(mess, paginatorEntity, Messages[Messages.IndexOf(ourTuple)].Item3 + 1);
            else if (index == ourTuple.Item2.Pages.Count())
                Messages[Messages.IndexOf(ourTuple)] = new(mess, paginatorEntity, ourTuple.Item2.Pages.Count() - 1);
            else if (index == 0 || index == -1)
                Messages[Messages.IndexOf(ourTuple)] = new(mess, paginatorEntity, 0);            
            else
                Messages[Messages.IndexOf(ourTuple)] = new(mess, paginatorEntity, Messages[Messages.IndexOf(ourTuple)].Item3 - 1);

            try
            {
                await mess.ModifyAsync(x => x.Embed = new Optional<Embed>(new EmbedBuilder
                {
                    Title = paginatorEntity.Title,
                    ThumbnailUrl = paginatorEntity.ThumbnailUrl,
                    Description = paginatorEntity.Pages.ToList()[Messages[Messages.Select(z => z.Item1.Id).ToList().IndexOf(ourTuple.Item1.Id)].Item3],
                    Color = paginatorEntity.Color
                }.Build()));
            }
            catch (Exception)
            {
                await mess.ModifyAsync(x => x.Embed = new Optional<Embed>(new EmbedBuilder
                {
                    Title = paginatorEntity.Title,
                    ThumbnailUrl = paginatorEntity.ThumbnailUrl,
                    Description = paginatorEntity.Pages.ToList()[0],
                    Color = paginatorEntity.Color
                }.Build()));
            }
        }        
    }
}
