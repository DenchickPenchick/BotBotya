using Discord;
using Discord.WebSocket;
using DiscordBot.Providers;
using DiscordBot.Serializable;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Modules.AdvertisingManaging
{
    public class AdvertisingModule : IModule
    {
        private DiscordSocketClient Client { get; }
        private SerializableConfig Configuration { get; set; }

        public AdvertisingModule(DiscordSocketClient client)
        {
            Client = client;            
            Client.Ready += Client_Ready;
        }


        public void RunModule()
        {
            Client.ReactionAdded += Client_ReactionAdded;
        }

        private Task Client_Ready()
        {
            Configuration = FilesProvider.GetConfig();
            return Task.CompletedTask;
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var message = arg1.Value;
            var reactAuthor = arg3.User.Value;
            var globOptions = FilesProvider.GetGlobalOptions();
            if (Configuration != null && message != null)
            {
                if (reactAuthor.Id == Configuration.AdminId && globOptions.MessagesToCheck.Contains(message.Id))
                {
                    ulong guildId = ulong.Parse(arg1.Value.Embeds.First().Footer.Value.Text);
                    var serGuild = FilesProvider.GetGuild(guildId);
                    var guild = Client.GetGuild(guildId);

                    int indexOf = globOptions.MessagesToCheck.IndexOf(message.Id);

                    if (guild != null && serGuild != null)
                    {
                        var adminDM = await guild.Owner.GetOrCreateDMChannelAsync();

                        switch (arg3.Emote.Name)
                        {
                            case "✅":
                                serGuild.AdvertisingAccepted = true;
                                serGuild.AdvertisingModerationSended = false;
                                globOptions.MessagesToCheck.RemoveAt(indexOf);                                
                                                                
                                await adminDM.SendMessageAsync($"Проверка для сервера {guild.Name} пройдена успешно! Теперь Вы можете рассылать Ваши объявления!");                                                                

                                break;
                            case "❌":
                                serGuild.AdvertisingAccepted = false;
                                serGuild.AdvertisingModerationSended = false;

                                serGuild.NextCheck = DateTime.Now.ToUniversalTime().AddHours(3);

                                globOptions.MessagesToCheck.RemoveAt(indexOf);
                                await adminDM.SendMessageAsync($"Вам пришел отказ в использовании функции \"Взаимопиар\" на сервере {guild.Name}. Проверьте, соблюдаете ли Вы все правила, которые написаны в условии пользования данной функцией.\nВы можете прислать объявление на проверку повторно через 3 часа.");
                                break;
                        }

                        FilesProvider.RefreshGuild(serGuild);
                        FilesProvider.RefreshGlobalOptions(globOptions);
                    }
                }
            }            
        }
    }
}
