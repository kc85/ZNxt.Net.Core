namespace ZNxt.Net.Core.Module.Notifier.Services.Api
{
    public class OTPModel
    {
        public string Message { get; set; } = "";

        public string To { get; set; } = "";

        public string Subject { get; set; } = "";

        public string Type { get; set; } = "SMS";

        public string OTPType { get; set; } = "";
        public string SecurityToken { get; set; } = "";

        public string OTP { get; set; } = "";

        public long? Duration  { get; set; } 

    }

}
