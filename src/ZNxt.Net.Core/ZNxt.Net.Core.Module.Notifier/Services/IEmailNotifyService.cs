using System.Collections.Generic;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Module.Notifier.Services
{
    interface IEmailNotifyService
    {
        bool Send(List<string> toEmail, string fromEmail, List<string> CC, string emailBody, string subject);
    }
}