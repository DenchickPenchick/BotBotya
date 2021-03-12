//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System.Collections.Generic;
using DiscordBot.Models;

namespace DiscordBot.Serializable
{
    public class SerializableGlobalOptions
    {
        /// <summary>
        /// Глобальные нежелательные слова
        /// </summary>
        public List<WordModel> GlobalBadWords { get; set; } = new List<WordModel>();
        /// <summary>
        /// Претенденты слов на добавление в глобальный список опечаток. Первый параметр - это слово, которое правильно написано. Второй - это кортеж из опечатки и голосов (первый за, а второй против)
        /// </summary>
        public Dictionary<string, (string, int, int)> ApplicantWords { get; set; } = new Dictionary<string, (string, int, int)>();
    }
}
