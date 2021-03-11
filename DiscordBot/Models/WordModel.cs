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
|GitHub: https://github.com/DenVot/BotBotya                             |
|______________________________________________________________________ |
|© Copyright 2021 Denis Voitenko                                        |
|© Copyright 2021 All rights reserved                                   |
|License: http://opensource.org/licenses/MIT                            |
_________________________________________________________________________
*/


using DiscordBot.TextReaders;
using System.Collections.Generic;

namespace DiscordBot.Models
{
    /// <summary>
    /// В данный момент на стадии разработки модель слова. Не использовать!
    /// </summary>
    public class WordModel
    {                
        public string Word { get; set; } = null;

        /// <summary>
        /// Слова с опечатками
        /// </summary>
        public List<string> Exceptions { get; private set; } = new List<string>();
        /// <summary>
        /// Считает редакционное расстояние Левенштейна.
        /// </summary>
        /// <param name="wordToCalc">Слово, для которого нужно посчитать редакционное расстояние</param>
        /// <returns>Расстояние Левенштейна. Тип: <see cref="int"/>.</returns>
        public int CalculateLevensteinDistanse(string wordToCalc) => Filter.Distance(Word, wordToCalc);        
    }
}
