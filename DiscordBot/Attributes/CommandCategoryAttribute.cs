//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System;

namespace DiscordBot.Attributes
{
    /// <summary>
    /// Абстрактный класс, который наследуют все категории команд. Наследует <see cref="Attribute"/>.
    /// </summary>
    public abstract class CommandCategoryAttribute : Attribute
    {
        /// <summary>
        /// Название категории команд
        /// </summary>
        public abstract string CategoryName { get; }
    }
}
