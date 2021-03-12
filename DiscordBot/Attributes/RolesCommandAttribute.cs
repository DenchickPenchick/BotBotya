//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

namespace DiscordBot.Attributes
{
    /// <summary>
    /// Атрибут, который присваивается всем командам, которые предназначены для музыки. Наследует абстрактный класс <see cref="CommandCategoryAttribute"/>
    /// </summary>
    public class RolesCommandAttribute : CommandCategoryAttribute
    {
        /// <summary>
        /// Название категории
        /// </summary>
        public override string CategoryName { get => "Магазин ролей"; }
    }
}
