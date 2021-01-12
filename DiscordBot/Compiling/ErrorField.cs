namespace DiscordBot.Compiling
{
    public class ErrorField : IField
    {
        public ErrorField(string mess)
        {
            message = mess;
        }
        public string Message { get => $"⛔ {message}"; }

        private readonly string message = null;
    }
}
