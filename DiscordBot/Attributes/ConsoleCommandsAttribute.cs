//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

namespace DiscordBot.Attributes
{
    /// <summary>
    /// Добавляется к командам, которые упрощают работу со свервером Discord (консоль по сути). Наследует абстрактный класс <see cref="CommandCategoryAttribute"/>.
    /// </summary>
    public class ConsoleCommandsAttribute : CommandCategoryAttribute
    {
        /// <summary>
        /// Название категории
        /// </summary>
        public override string CategoryName => "Консоль сервера";
    }
}
