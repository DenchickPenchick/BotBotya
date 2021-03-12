//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System.Collections.Generic;

namespace DiscordBot.Serializable
{
    public class SerializableReactRoleMessages
    {
        public List<SerializableReactRoleMessage> ReactRoleMessages { get; set; } = new List<SerializableReactRoleMessage>();
    }
}
