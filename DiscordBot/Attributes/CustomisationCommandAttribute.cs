//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

namespace DiscordBot.Attributes
{
    /// <summary>
    /// Атрибут, который присваивается всем командам кастомизации бота. Наследует абстрактный класс <see cref="CommandCategoryAttribute"/>.
    /// </summary>
    public class CustomisationCommandAttribute : CommandCategoryAttribute
    {
        /// <summary>
        /// Название категории
        /// </summary>
        public override string CategoryName { get => "Кастомизация"; }
    }
}
