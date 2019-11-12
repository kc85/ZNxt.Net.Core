using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Module.Notifier.Services.Api
{
    public class NotifierViewController : Core.Services.ApiBaseService
    {
        public NotifierViewController(IHttpContextProxy httpContextProxy, IDBService dBService, ILogger logger, IResponseBuilder responseBuilder) : base(httpContextProxy, dBService, logger, responseBuilder)
        {
        }
        [Route("/notifier/email/queue", CommonConst.ActionMethods.GET, "sys_admin")]
        public JObject GetEmails()
        {
            return GetPaggedData(CommonConst.Collection.EMAIL_QUEUE);
        }
        [Route("/notifier/sms/queue", CommonConst.ActionMethods.GET, "sys_admin")]
        public JObject GetSms()
        {
            return GetPaggedData(CommonConst.Collection.SMS_QUEUE);
        }
    }
}
