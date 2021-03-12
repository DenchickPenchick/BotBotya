//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

namespace DiscordBot.Attributes
{
    /// <summary>
    /// Атрибут, который присваивается всем музыкальным командам бота. Наследует <see cref="CommandCategoryAttribute"/>
    /// </summary>
    public class MusicCommandAttribute : CommandCategoryAttribute
    {
        /// <summary>
        /// Название категории
        /// </summary>
        public override string CategoryName { get => "Музыкальные команды"; }
    }
}
