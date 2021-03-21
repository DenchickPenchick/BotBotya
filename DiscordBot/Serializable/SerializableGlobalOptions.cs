//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using DiscordBot.Models;
using System.Collections.Generic;

namespace DiscordBot.Serializable
{
    public class SerializableGlobalOptions
    {
        ///// <summary>
        ///// Глобальные нежелательные слова
        ///// </summary>
        //public List<WordModel> GlobalBadWords { get; set; } = new List<WordModel>();
        ///// <summary>
        ///// Претенденты слов на добавление в глобальный список опечаток. Первый параметр - это слово, которое правильно написано. Второй - это кортеж из опечатки и голосов (первый за, а второй против)
        ///// </summary>
        //public Dictionary<string, (string, int, int)> ApplicantWords { get; set; } = new Dictionary<string, (string, int, int)>();
        /// <summary>
        /// Коллекция ключ-значение, где хранится в виде ключа - id сообщения, а в виде значения - id Discord севрера. Относится к взаимопиару.
        /// </summary>
        public List<ulong> MessagesToCheck { get; set; } = new List<ulong>();
    }
}
