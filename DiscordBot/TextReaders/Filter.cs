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

namespace DiscordBot.TextReaders
{
    public class Filter 
    {
        public IEnumerable<string> Words { get; set; }
        public string StringToFilter { get; set; }

        public enum Result { Nothing, Error, Words }

        public Filter(string str, SerializableGuild guild)
        {
            StringToFilter = str;
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
                        if (Distance(word, toCheck) < totalMarks)
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

        public int Distance(string firstWord, string secondWord)
        {
            firstWord = firstWord.ToLower();
            secondWord = secondWord.ToLower();
            const int deleteCost = 2, insertCost = 1;
            int n = firstWord.Length + 1;
            int m = secondWord.Length + 1;
            int[,] matrix = new int[n, m];

            for (int i = 0; i < n; i++)
                matrix[i, 0] = i;

            for (int i = 0; i < m; i++)
                matrix[0, i] = i;


            for (int i = 1; i < n; i++)
            {
                for (int j = 1; j < m; j++)
                {
                    int substitutionCost = firstWord[i - 1] == secondWord[j - 1] ? 0 : 1;
                    matrix[i, j] = Minimum(
                        matrix[i - 1, j] + deleteCost,
                        matrix[i, j - 1] + insertCost,
                        matrix[i - 1, j - 1] + substitutionCost);
                }
            }

            return matrix[n - 1, m - 1];
        }

        private int Minimum(int a, int b, int c) => (a = a < b ? a : b) < c ? a : c;
    }
}
