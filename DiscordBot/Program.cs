//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System;
using System.Drawing;
using Console = Colorful.Console;

namespace DiscordBot
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