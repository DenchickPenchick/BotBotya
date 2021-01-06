using System;

namespace DiscordBot
{
    public class ProgressBar
    {
        private readonly double count;
        private double pos = 0;

        public ProgressBar(int count)
        {
            this.count = count;
        }

        public void PlusVal(int posY)
        {
            Console.SetCursorPosition(0, posY);
            pos++;
            Console.WriteLine($"Done {Convert.ToInt32(pos / count * 100)}%");
        }
    }
}
