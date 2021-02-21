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

using DiscordBot.Serializable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.TextReaders
{
    public class Filter 
    {
        public IEnumerable<string> Words { get; set; }
        public IEnumerable<string> ExceptWords { get; set; }
        public string StringToFilter { get; set; }

        public enum Result { Nothing, Error, Words }

        public Filter(string str, SerializableGuild guild)
        {
            StringToFilter = str;
            ExceptWords = guild.ExceptWords;
            Words = guild.BadWords;
        }

        public Filter(string str, IEnumerable<string> words)
        {
            StringToFilter = str;
            Words = words;
        }

        public Result Filt()
        {
            try
            {
                bool words = false;

                foreach (string word in StringToFilter.Split(' '))
                {
                    int totalMarks = (Convert.ToSingle(word.Length) % 2F > 0 ? word.Length + 1 : word.Length) / 2;
                    foreach (var toCheck in Words)
                        if (Distance(word, toCheck) < totalMarks && !ExceptWords.Contains(word.ToLower()))
                            words = true;
                }

                if (words)
                    return Result.Words;
                else
                    return Result.Nothing;
            }
            catch (Exception)
            {
                return Result.Error;
            }
        }

        public static int Distance(string firstWord, string secondWord)
        {
            firstWord = firstWord.ToLower();
            secondWord = secondWord.ToLower();
            var n = firstWord.Length + 1;
            var m = secondWord.Length + 1;
            var matrixD = new int[n, m];

            const int deletionCost = 1;
            const int insertionCost = 1;

            for (var i = 0; i < n; i++)
            {
                matrixD[i, 0] = i;
            }

            for (var j = 0; j < m; j++)
            {
                matrixD[0, j] = j;
            }

            for (var i = 1; i < n; i++)
            {
                for (var j = 1; j < m; j++)
                {
                    var substitutionCost = firstWord[i - 1] == secondWord[j - 1] ? 0 : 1;

                    matrixD[i, j] = Minimum(matrixD[i - 1, j] + deletionCost,          
                                            matrixD[i, j - 1] + insertionCost,         
                                            matrixD[i - 1, j - 1] + substitutionCost); 
                }
            }

            return matrixD[n - 1, m - 1];
        }

        private static int Minimum(int a, int b, int c) => (a = a < b ? a : b) < c ? a : c;
    }
}
