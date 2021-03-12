//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using DiscordBot.Collections;

namespace DiscordBot.Modules.ProcessManage
{
    public class ProcessingModule : IModule
    {
        private ModulesCollection ModulesCollection { get; }

        public ProcessingModule(ModulesCollection modulesCollection) => ModulesCollection = modulesCollection;        

        public void RunModule() => ModulesCollection.RunAllModules();
        
    }
}
