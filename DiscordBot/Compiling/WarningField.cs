namespace DiscordBot.Compiling
{
    public class WarningField : IField
    {
        public WarningField(string mess)
        {
            message = mess;
        }
        public string Message { get => $"⚠️ {message}"; }

        private readonly string message = null;
    }
}
