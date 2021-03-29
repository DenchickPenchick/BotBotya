using Discord;

namespace DiscordBot.Providers.Entities
{
    public class PiarToSWarnEmbed
    {
        public const string Title = "Нарушение правил взаимопиара";
        public const string FirstDescription = "Вы добавлены в черный список за нарушение ToS функции \"Взаимопиар\".";
        public string ToSPoint { get; set; }
        public string WarnDescription { get; set; }

        public Embed Build() => new EmbedBuilder
        {
            Title = Title,
            Description = $"{FirstDescription}\n**Пункт ToS:** {ToSPoint}\n**Описание:** {WarnDescription}\nЕсли Вам кажется, что Вы не нарушали ToS, тогда напишите на [сервер поддержки](https://discord.gg/p6R4yk7uqK)." +
            $"\nПрежде чем писать, советую ознакомиться с [ToS](https://botbotya.ru/Pages/PiarPage/index.html) функции \"Взаимопиар\"",
            Author = new EmbedAuthorBuilder
            { 
                Name = "Администрация",
                Url = "https://discord.gg/p6R4yk7uqK"
            },
            Color = new Color(18, 124, 164)
        }.Build();                
    }
}
