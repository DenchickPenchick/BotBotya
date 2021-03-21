//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System;
using System.Collections.Generic;

namespace DiscordBot.Serializable
{
    public class SerializableGuild
    {
        /// <summary>
        /// Id Discord сервера
        /// </summary>
        public ulong GuildId { get; set; } = 0;
        /// <summary>
        /// Id роли по-умолчанию
        /// </summary>
        public ulong DefaultRoleId { get; set; } = 0;
        /// <summary>
        /// Максимальное количество предупреждений
        /// </summary>
        public int MaxWarns { get; set; } = 5;
        /// <summary>
        /// Максимальное количество лотов в лотерее.
        /// </summary>
        public int LotsCount { get; set; } = 3;
        /// <summary>
        /// Кикать за превышение предупреждений
        /// </summary>
        public bool KickForWarns { get; set; } = false;
        /// <summary>
        /// Банить за превышение предупреждений
        /// </summary>
        public bool BanForWarns { get; set; } = false;
        /// <summary>
        /// Мутить за превышение предупреждений
        /// </summary>
        public bool MuteForWarns { get; set; } = false;
        /// <summary>
        /// Получать предупреждения за плохие слова
        /// </summary>
        public bool WarnsForBadWords { get; set; } = false;
        /// <summary>
        /// Получать предупреждения за ссылки-приглашения
        /// </summary>
        public bool WarnsForInviteLink { get; set; } = false;
        /// <summary>
        /// Отправка приветственных сообщений
        /// </summary>
        public bool HelloMessageEnable { get; set; } = false;
        /// <summary>
        /// Проверка контента
        /// </summary>
        public bool CheckingContent { get; set; } = false;
        /// <summary>
        /// Проверка правописания команд. Если true, тогда бот будет уведомлять о неправильной команде.
        /// </summary>
        public bool UnknownCommandMessage { get; set; } = false;
        /// <summary>
        /// Проверка плохих слов
        /// </summary>
        public bool CheckingBadWords { get; set; } = false;
        /// <summary>
        /// Создание текстового канала для голосового канала, который будет виден только тем кто гвоорит в данном голосовом канале.
        /// </summary>
        public bool CreateTextChannelsForVoiceChannels { get; set; } = false;
        /// <summary>
        /// Показывает, допущен ли сервер к рекламе
        /// </summary>
        public bool AdvertisingAccepted { get; set; } = false;
        /// <summary>
        /// Показывает, рассматривается ли объявление в данный момент
        /// </summary>
        public bool AdvertisingModerationSended { get; set; } = false;
        /// <summary>
        /// Содержание приветственного сообщения
        /// </summary>
        public string HelloMessage { get; set; } = null;
        /// <summary>
        /// Эмодзи у комнаты
        /// </summary>
        public string EmojiOfRoom { get; set; } = "🎤";
        /// <summary>
        /// Префикс бота
        /// </summary>
        public string Prefix { get; set; } = "!";
        /// <summary>
        /// Цвет эмбедов
        /// </summary>
        public string EmbedColor { get; set; } = "Синий";
        /// <summary>
        /// Коллекция с id каналов, где можно отвечать на команды
        /// </summary>
        public List<ulong> CommandsChannels { get; set; } = new List<ulong>();
        /// <summary>
        /// Игнор роли
        /// </summary>
        public List<ulong> IgnoreRoles { get; set; } = new List<ulong>();
        /// <summary>
        /// Черный список ролей на продажу
        /// </summary>
        public List<ulong> BlaskListedRolesToSale { get; set; } = new List<ulong>();
        /// <summary>
        /// Список плохих слов
        /// </summary>
        public List<string> BadWords { get; set; } = new List<string>();
        /// <summary>
        /// Список слов-исключений
        /// </summary>
        public List<string> ExceptWords { get; set; } = new List<string>();
        /// <summary>
        /// Список людей с предупреждениями
        /// </summary>
        public List<(ulong, int)> BadUsers { get; set; } = new List<(ulong, int)>();
        /// <summary>
        /// Системные категории
        /// </summary>
        public SerializableCategories SystemCategories { get; set; } = new SerializableCategories();
        /// <summary>
        /// Системные каналы.
        /// </summary>
        public SerializableChannels SystemChannels { get; set; } = new SerializableChannels();
        /// <summary>
        /// Объявление
        /// </summary>
        public SerializableAdvertising Advert { get; set; } = new SerializableAdvertising();
        /// <summary>
        /// Показывает, когда можно совершить следующий запрос на проверку
        /// </summary>
        public DateTime NextCheck { get; set; } = new DateTime();
        /// <summary>
        /// Показывает следующее время рассылки
        /// </summary>
        public DateTime NextSend { get; set; } = new DateTime();
    }
}
