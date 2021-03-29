//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using System;
using System.Drawing;
using Console = Colorful.Console;

namespace DiscordBot.Providers
{
    public static class LogsProvider
    {        
        public static LogStream Log(string log, bool withAutoEnd = true)
        {
            var stream = new LogStream("log", Color.White);

            Console.WriteLine($"LOG [Time: {DateTime.Now.ToShortTimeString()} Date: {DateTime.Now.ToShortDateString()}]", Color.White);

            stream.StartStream();                       
            stream.WriteLine($"Description: {log}");

            if (withAutoEnd)
                stream.EndStream();            

            return stream;
        }

        public static LogStream ErrorLog(Error error, bool withAutoEnd = true) 
        {
            var stream = new LogStream("error", Color.OrangeRed);

            Console.WriteLine($"ERROR OCCURED [Time: {DateTime.Now.ToShortTimeString()} Date: {DateTime.Now.ToShortDateString()}]", Color.OrangeRed);
            stream.StartStream();
            stream.WriteLine($"Description: {error.Description}");  

            if(error.OccuredIn != "NONE")
                stream.WriteLine($"Occured in: {error.OccuredIn}");

            if(withAutoEnd)
                stream.EndStream();            

            return stream;
        }

        public static LogStream ExceptionLog(Exception exception, bool withAutoEnd = true)
        {
            var stream = new LogStream("exception", Color.Red);

            Console.WriteLine($"EXCEPTION THROWED [Time: {DateTime.Now.ToShortTimeString()} Date: {DateTime.Now.ToShortDateString()}]", Color.Red);
            stream.StartStream();
            stream.WriteLine($"Message: {exception.Message}");
            stream.WriteLine($"Throwed in: {exception.Source}");

            if(withAutoEnd)
                stream.EndStream();

            return stream;
        }
    }

    public class Error
    {
        public string Description { get; set; } = "NONE";
        public string OccuredIn { get; set; } = "NONE";
    }

    public class LogStream : IDisposable
    {
        private readonly Color streamColor;
        private readonly string streamName;

        public string Content { get; private set; }
        public bool IsEnded { get; private set; }

        public LogStream(string streamName, Color streamColor)
        {
            this.streamColor = streamColor;
            this.streamName = streamName.ToUpper();
        }

        public void WriteLine(string line)
        {
            if (IsEnded)
                throw new InvalidOperationException("Log stream ended and you can't write");

            Content += $"\n│\n├─{line}";

            Console.WriteLine("│", streamColor);
            Console.WriteLine($"├─{line}", streamColor);
        }

        public void StartStream()
        {
            if (IsEnded)
                throw new InvalidOperationException("Log stream ended and you can't start it again");

            Content += $"┌─{streamName} STREAM";
            Console.WriteLine($"┌─{streamName} STREAM", streamColor);
        }

        public void EndStream()
        {
            if (IsEnded)
                throw new InvalidOperationException("Log stream ended and you can't end it again");

            Content += $"│\n└─{streamName} STREAM ENDED";
            Console.WriteLine("│", streamColor);
            Console.WriteLine($"└─{streamName} STREAM ENDED", streamColor);

            IsEnded = true;
        }

        public void Dispose()
        {
            IsEnded = true;
        }
    }
}
