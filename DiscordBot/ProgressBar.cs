using System;

namespace DiscordBot
{
    public class ProgressBar
    {
        private readonly int count;
        private int pos = 0;

        public ProgressBar(int count)
        {
            this.count = count;
        }

        public void PlusVal(int posY)
        {
            Console.SetCursorPosition(0, posY);
            pos++;
            Console.WriteLine($"Done {pos / count * 100}%");
        }
    }
}
