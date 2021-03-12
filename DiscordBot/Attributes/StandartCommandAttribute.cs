//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

namespace DiscordBot.Attributes
{
    /// <summary>
    /// Атрибут, который есть у всех стандартных команд бота. Он добавляется к команде по-умолчанию, если у нее не указан специальный атрибут. Наследует абстрактный класс <see cref="CommandCategoryAttribute"/>.
    /// </summary>
    public class StandartCommandAttribute : CommandCategoryAttribute
    {
        /// <summary>
        /// Название категории
        /// </summary>
        public override string CategoryName { get => "Стандартные команды"; }
    }
}
