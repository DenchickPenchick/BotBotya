//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using DiscordBot.Modules;
using DiscordBot.Providers;
using System.Collections;
using System.Collections.Generic;

namespace DiscordBot.Collections
{
    public class ModulesCollection : IEnumerable
    {
        public ModulesCollection()
        {

        }

        private ModulesCollection(List<IModule> modules)
        {
            Modules = modules;
        }

        public ModulesCollection AddModule(IModule module)
        {
            Modules.Add(module);
            return new ModulesCollection(Modules);
        }

        public void RunAllModules()
        {
            Modules.ForEach(x => x.RunModule());
            LogsProvider.Log("All modules started");
        } 

        public IEnumerator GetEnumerator() => Modules.GetEnumerator();

        private List<IModule> Modules { get; set; } = new List<IModule>();
    }
}
