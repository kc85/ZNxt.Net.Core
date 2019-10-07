namespace ZNxt.Net.Core.Module.Notifier.Services
{
    interface IOTPNotifyService
    {
        bool SendEmail(string email, string emailTemplateText, string subject, string otpType, string securityToken);
        bool SendSMS(string phoneNumber, string smsTemplateText, string otpType, string securityToken);
        bool ValidateEmail(string email, string otp, string otpType, string securityToken);
        bool ValidateSMS(string phoneNumber, string otp, string otpType, string securityToken = null);
    }
}