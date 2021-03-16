//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System.Collections.Generic;

namespace DiscordBot.Serializable
{
    public class SerializableEconomicGuild
    {
        public ulong Id { get; set; } = 0;
        public int RewardForMessage { get; set; } = 100;
        public List<SerializableEconomicGuildUser> SerializableEconomicUsers { get; set; } = new List<SerializableEconomicGuildUser>();
        public List<(ulong, int)> RolesAndCostList { get; set; } = new List<(ulong, int)>();
    }
}
