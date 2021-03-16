using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.Providers.Entities
{
    public static class LoteryEntity
    {
        private static readonly string[] allPoints = { "🍒", "🍊", "🍎", "🧀", "💎", "🍆" };

        public static Tuple<string, double> CalculateWon(int lotsCount)
        {
            var random = new Random();
            string[] currentPoints = new string[lotsCount];

            for (int i = 0; i < currentPoints.Length; i++)
                currentPoints[i] = allPoints[random.Next(0, allPoints.Length - 1)];

            Dictionary<string, int> table = new Dictionary<string, int>();

            currentPoints.Distinct().ToList().ForEach(x => table.Add(x, 1));
            currentPoints.ToList().ForEach(x => table[x] = currentPoints.Where(y => y == x).Count());

            string val = null;

            currentPoints.ToList().ForEach(x =>
            {
                if (val == null)
                    val += x;
                else
                    val += $"┋{x}";
            });

            if (table.Any(x => x.Value > currentPoints.Length / 2))
                return new Tuple<string, double>(val, 1.5);
            else if (table.Any(x => x.Value == currentPoints.Length))
                return new Tuple<string, double>(val, 5);
            else
                return new Tuple<string, double>(val, 0);
        }
    }
}
