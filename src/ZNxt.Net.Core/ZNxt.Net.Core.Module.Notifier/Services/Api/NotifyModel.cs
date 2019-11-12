namespace ZNxt.Net.Core.Module.Notifier.Services.Api
{
    public class NotifyModel
    {
        public string Message { get; set; } = "";

        public string To { get; set; } = "";

        public string CC { get; set; } = "";

        public string BCC { get; set; } = "";

        public string Subject { get; set; } = "";

        public string Type { get; set; } = "";
    }

}
