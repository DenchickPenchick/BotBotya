using System.Collections.Generic;
using System.Linq;
using Discord;
using DiscordBot.Providers.Entities;
using DiscordBot.Serializable;

namespace DiscordBot.Providers
{
    /// <summary>
    /// Провайдер для упрощенного создания предупреждений о нарушении ToS функции "Взаимопиар"
    /// </summary>
    public class PiarToSProvider
    {
        private readonly PiarToSWarnEmbed EmbedWarn = new();

        public Embed Embed { get => EmbedWarn.Build(); }

        private readonly Dictionary<double, string> defaultPoints = new()
        {
            { 2.1, "В текстовом канале у бота выключено право \"Отправлять сообщение\"." },
            { 2.2, "В текстовом канале у роли `everyone`выключено право \"Просмотр канала\"." },
            { 4.0, "Объявление содержит нецензурную лексику" },
            { 5.0, "Объявление содержит [NSFW-материалы](https://ru.wikipedia.org/wiki/NSFW)." },
            { 6.0, "Объявление содержит материалы, которые запрещены к публикации на территории РФ законодательством." },
            { 7.0, "Объявление содержит материалы, которые нарушают [ToS Discord](https://discord.com/terms)." },
            { 8.0, "Объявление содержит материалы, которые оскверняют бота." },
            { 9.0, "Объявление содержит материалы, которые содержат подтекст осквернения бота." },
            { 10.0, "Объявление содержит защищенные авторским правом материалы." }
        };

        /// <summary>
        /// Устанавливает пункт нарушения ToS функции "Взаимпоиар"
        /// </summary>
        /// <param name="point">Пункт</param>
        /// <param name="withDefaultDescription">Если <c>true</c>, тогда подбирается соответствующее описание</param>
        public void SetPoint(int point, bool withDefaultDescription = false) => SetPoint(pointDouble: point, withDefaultDescription);

        /// <summary>
        /// Устанавливает пункт нарушения ToS функции "Взаимпоиар". 
        /// </summary>
        /// <param name="pointDouble">Пункт</param>
        /// <param name="withDefaultDescription">Если <c>true</c>, тогда подбирается соответствующее описание</param>
        public void SetPoint(double pointDouble, bool withDefaultDescription = false)
        {
            EmbedWarn.ToSPoint = pointDouble.ToString();

            if (withDefaultDescription && defaultPoints.ContainsKey(pointDouble))
                EmbedWarn.WarnDescription = defaultPoints[pointDouble];
        }

        /// <summary>
        /// Устанавливает описание нарушения
        /// </summary>
        /// <param name="description">Описание</param>
        /// <param name="withDefaultPoint">Соответствующий пункт по-умолчанию</param>
        public void SetDescription(string description, bool withDefaultPoint = false)
        {
            EmbedWarn.WarnDescription = description;

            if (withDefaultPoint && defaultPoints.ContainsValue(description))
                EmbedWarn.ToSPoint = defaultPoints
                    .Where(x => x.Value == description)
                    .First()
                    .Key
                    .ToString();
        }

        /// <summary>
        /// Обновляет конфигурацию сервера
        /// </summary>
        /// <param name="guild">Текущая конфигурация</param>
        /// <returns>Новая конфигурация сервера (<see cref="SerializableGuild"/>)</returns>
        public SerializableGuild SerializeEmbed(SerializableGuild guild)
        {
            guild.IsBanned = true;
            guild.WarnEmbed = EmbedWarn;

            return guild;
        }

        /// <summary>
        /// Обновляет конфигурацию сервера с последующей записью в файл конфигурации
        /// </summary>
        /// <param name="guild">Текущая конфигурация</param>
        /// <returns>Новая конфигурация сервера (<see cref="SerializableGuild"/>)</returns>
        public SerializableGuild SerializeEmbedWithRefresh(SerializableGuild guild)
        {
            var serGuild = SerializeEmbed(guild);

            FilesProvider.RefreshGuild(serGuild);

            return serGuild;
        }
    }
}
