using System.Collections.Generic;

namespace ZNxt.Net.Core.Module.Notifier.Services
{
    public interface ISMSNotifyService
    {
        Dictionary<string, bool> Send(List<string> to, string message);
        bool Send(string toSms, string message);
    }
}