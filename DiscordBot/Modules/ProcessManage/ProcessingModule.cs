using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Modules.ProcessManage
{
    public class ProcessingModule : IModule
    {
        private ProcessingConfiguration ProcessingConfiguration { get; set; }

        public ProcessingModule(ProcessingConfiguration configuration) 
        {
            ProcessingConfiguration = configuration;
        }

        public void RunModule()
        {
            foreach (var module in ProcessingConfiguration.GetAllModules())            
                module.RunModule();            
        }
    }
}
