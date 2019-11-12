using System.Collections.Generic;

namespace ZNxt.Net.Core.Module.Notifier.Services
{
    interface IEmailNotifyService
    {
        bool Send(List<string> toEmail, string fromEmail, List<string> CC, string emailBody, string subject);
    }
}