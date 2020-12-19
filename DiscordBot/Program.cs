using DiscordBot;
using System;
using System.Drawing;
using Console = Colorful.Console;

namespace TestBot
{
    class Program
    {
        static void Main(string[] args) 
        {
            try
            {
                new Bot().RunBotAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}", Color.Red);
            }
        }
    }
}