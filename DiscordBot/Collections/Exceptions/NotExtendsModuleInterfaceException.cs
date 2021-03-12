//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System;

namespace DiscordBot.Collections.Exceptions
{
    public class NotExtendsModuleInterfaceException : Exception
    {
        public Type ExceptionType { get; }

        public NotExtendsModuleInterfaceException(Type exceptionType) : base($"Type {exceptionType.Name} not extends IModule interface.")
        {
            ExceptionType = exceptionType;
        }
    }
}
