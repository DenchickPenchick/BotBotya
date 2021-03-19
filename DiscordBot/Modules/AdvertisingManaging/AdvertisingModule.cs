using Discord;
using Discord.WebSocket;
using DiscordBot.Providers;
using DiscordBot.Serializable;
using System.Threading.Tasks;

namespace DiscordBot.Modules.AdvertisingManaging
{
    public class AdvertisingModule : IModule
    {
        private DiscordSocketClient Client { get; }
        private SerializableConfig Configuration { get; }

        public AdvertisingModule(DiscordSocketClient client, SerializableConfig configuration)
        {
            Client = client;
            Configuration = configuration;
        }

        public void RunModule()
        {
            Client.ReactionAdded += Client_ReactionAdded;
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var message = arg1.Value;
            var reactAuthor = arg3.User.Value;
            var globOptions = FilesProvider.GetGlobalOptions();

            if (reactAuthor.Id == Configuration.AdminId && globOptions.CheckingMessagesForAdvertising.ContainsKey(message.Id))
            {
                ulong guildId = globOptions.CheckingMessagesForAdvertising[message.Id];
                var serGuild = FilesProvider.GetGuild(guildId);
                var guild = Client.GetGuild(guildId);

                if (guild != null && serGuild != null)
                { 
                    switch (arg3.Emote.Name)
                    {
                        case "✅":
                            serGuild.AdvertisingAccepted = true;
                            serGuild.AdvertisingModerationSended = false;
                            await guild.DefaultChannel.SendMessageAsync("Проверка пройдена успешно! Теперь Вы можете рассылать Ваши объявления!");
                            break;
                        case "❌":
                            serGuild.AdvertisingAccepted = false;
                            serGuild.AdvertisingModerationSended = false;
                            await guild.DefaultChannel.SendMessageAsync("Вам пришел отказ. Проверьте, соблюдаете ли Вы все правила, которые написаны в условии пользования данной функцией.");
                            break;
                    }
                    FilesProvider.RefreshGuild(serGuild);
                }
            }
        }
    }
}
