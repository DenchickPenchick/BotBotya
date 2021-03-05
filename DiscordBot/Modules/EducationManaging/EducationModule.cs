/*
_________________________________________________________________________
|                                                                       |
|██████╗░░█████╗░████████╗  ██████╗░░█████╗░████████╗██╗░░░██╗░█████╗░  |
|██╔══██╗██╔══██╗╚══██╔══╝  ██╔══██╗██╔══██╗╚══██╔══╝╚██╗░██╔╝██╔══██╗  |
|██████╦╝██║░░██║░░░██║░░░  ██████╦╝██║░░██║░░░██║░░░░╚████╔╝░███████║  |
|██╔══██╗██║░░██║░░░██║░░░  ██╔══██╗██║░░██║░░░██║░░░░░╚██╔╝░░██╔══██║  |
|██████╦╝╚█████╔╝░░░██║░░░  ██████╦╝╚█████╔╝░░░██║░░░░░░██║░░░██║░░██║  |
|╚═════╝░░╚════╝░░░░╚═╝░░░  ╚═════╝░░╚════╝░░░░╚═╝░░░░░░╚═╝░░░╚═╝░░╚═╝  |
|______________________________________________________________________ |
|Author: Denis Voitenko.                                                |
|GitHub: https://github.com/DenchickPenchick                            |
|DEV: https://dev.to/denchickpenchick                                   |
|_____________________________Project__________________________________ |
|GitHub: https://github.com/DenchickPenchick/BotBotya                   |
|______________________________________________________________________ |
|© Copyright 2021 Denis Voitenko                                        |
|© Copyright 2021 All rights reserved                                   |
|License: http://opensource.org/licenses/MIT                            |
_________________________________________________________________________
*/

using DiscordBot.Models;
using DiscordBot.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DiscordBot.Modules.EducationManaging
{
    /// <summary>
    /// Модуль, который отвечает за правильный отбор нежелательных слов в глобальные настройки (<see cref="Serializable.SerializableGlobalOptions"/>.)
    /// </summary>
    public class EducationModule : IModule
    {           
        /// <summary>
        /// Инициализирует новый объект класса <see cref="EducationModule"/>.
        /// </summary>
        public void RunModule()
        {
            RunEducationThread();
        }

        private void RunEducationThread()
        {
            new Thread(x =>
            {
                while (true)
                {
                    var globalOptions = FilesProvider.GetGlobalOptions();
                    var allBadWords = new List<string>();
                    FilesProvider.GetAllGuilds().ToList().Select(y => y.BadWords).ToList().ForEach(z => allBadWords.AddRange(z));
                    List<(string, int)> badWordsStats = new List<(string, int)>();

                    foreach(var word in allBadWords)                    
                        if (badWordsStats.Any(y => y.Item1 != word))
                            badWordsStats.Add((word, 1));
                        else
                            badWordsStats[badWordsStats.Select(y => y.Item1).ToList().IndexOf(word)] = new (word, badWordsStats[badWordsStats.Select(y => y.Item1).ToList().IndexOf(word)].Item2 + 1);

                    foreach (var word in badWordsStats)                    
                        if (word.Item2 / Convert.ToSingle(allBadWords.Count) > 0.5)                        
                            if (!globalOptions.GlobalBadWords.Any(y => y.Word == word.Item1))                            
                                globalOptions.GlobalBadWords.Add(new WordModel
                                { 
                                    Word = word.Item1
                                });

                    FilesProvider.RefreshGlobalOptions(globalOptions);
                    Thread.Sleep(TimeSpan.FromHours(24));
                }
            });
        }
    }
}
