using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace DiscordBot.Interactivities
{
    public class PaginatingService
    {                
        public PaginatingService(DiscordSocketClient client)
        {
            client.ReactionAdded += Client_ReactionAdded;
        }

        private List<(RestUserMessage, PaginatorEntity, int)> Messages = new List<(RestUserMessage, PaginatorEntity, int)>();

        public async Task SendPaginatedMessageAsync(PaginatorEntity paginatorEntity, SocketCommandContext context, int num = 0)
        {            
            var mess = await context.Channel.SendMessageAsync(embed: new EmbedBuilder
            { 
                Title = paginatorEntity.Title,
                ThumbnailUrl = paginatorEntity.ThumbnailUrl,
                Description = paginatorEntity.Pages.ToList()[num],
                Color = paginatorEntity.Color
            }.Build());
            await mess.AddReactionsAsync(new List<IEmote>
            {
                new Emoji("⏪"),
                new Emoji("◀️"),
                new Emoji("▶️"),
                new Emoji("⏩")
            }.ToArray());
            Messages.Add((mess, paginatorEntity, num));
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg1.HasValue)
            {
                var message = arg1.Value;
                
                if (Messages.Select(x => x.Item1.Id).Contains(message.Id))
                {
                    var ourTuple = Messages.Where(x => x.Item1.Id == message.Id).FirstOrDefault();
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
                    }                    
                }
            }             
        }

        //Вперед: index > 0
        //Назад: index < 0
        //Индекс равен по модулю кол-ву элементов коллекции -> 
        private async Task ChangePageAsync(IUserMessage message, int index)
        {
            var ourTuple = Messages.Where(x => x.Item1.Id == message.Id).FirstOrDefault();

            var mess = ourTuple.Item1;
            var paginatorEntity = ourTuple.Item2;                        

            if (index > 0)
                Messages[Messages.IndexOf(ourTuple)] = new(mess, paginatorEntity, Messages[Messages.IndexOf(ourTuple)].Item3 + 1);
            else if(index == ourTuple.Item2.Pages.Count())
                Messages[Messages.IndexOf(ourTuple)] = new(mess, paginatorEntity, ourTuple.Item2.Pages.Count() - 1);
            else if(index == 0)
                Messages[Messages.IndexOf(ourTuple)] = new(mess, paginatorEntity, 0);
            else
                Messages[Messages.IndexOf(ourTuple)] = new(mess, paginatorEntity, Messages[Messages.IndexOf(ourTuple)].Item3 - 1);

            await mess.ModifyAsync(x => x.Embed = new Optional<Embed>(new EmbedBuilder
            {
                Title = paginatorEntity.Title,
                ThumbnailUrl = paginatorEntity.ThumbnailUrl,
                Description = paginatorEntity.Pages.ToList()[Messages[Messages.IndexOf(ourTuple)].Item3],
                Color = paginatorEntity.Color
            }.Build()));
        }        
    }
}
