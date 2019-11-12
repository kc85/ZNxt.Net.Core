using System;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Services
{
    [Obsolete]
    public abstract class BaseService
    {
        protected IDBService DBProxy { get; private set; }
        protected ILogger Logger { get; private set; }
        protected IActionExecuter ActionExecuter { get; private set; }
        protected IPingService PingService { get; private set; }
        protected IResponseBuilder ResponseBuilder { get; private set; }
        protected IAppSettingService AppSettingService { get; private set; }
        protected IViewEngine ViewEngine { get; private set; }
        protected IOTPService OTPService { get; private set; }
        protected ISMSService SMSService { get; private set; }
        protected IEmailService EmailService { get; private set; }
        protected IEncryption EncryptionService { get; private set; }
        protected IKeyValueStorage KeyValueStorage { get; private set; }

        public BaseService(ParamContainer paramContainer)
        {
            DBProxy = (IDBService) paramContainer.GetKey(CommonConst.CommonValue.PARAM_DBPROXY);
            Logger = (ILogger)paramContainer.GetKey(CommonConst.CommonValue.PARAM_LOGGER);
            ActionExecuter = (IActionExecuter)paramContainer.GetKey(CommonConst.CommonValue.PARAM_ACTIONEXECUTER);
            PingService = (IPingService)paramContainer.GetKey(CommonConst.CommonValue.PARAM_PING_SERVICE);
            ResponseBuilder  = (IResponseBuilder)paramContainer.GetKey(CommonConst.CommonValue.PARAM_RESPONBUILDER);
            ViewEngine = (IViewEngine)paramContainer.GetKey(CommonConst.CommonValue.PARAM_VIEW_ENGINE);
            AppSettingService =(IAppSettingService)  paramContainer.GetKey(CommonConst.CommonValue.PARAM_APP_SETTING);
            OTPService = (IOTPService) paramContainer.GetKey(CommonConst.CommonValue.PARAM_OTP_SERVICE);
            SMSService =  (ISMSService)paramContainer.GetKey(CommonConst.CommonValue.PARAM_SMS_SERVICE);
            EmailService =(IEmailService) paramContainer.GetKey(CommonConst.CommonValue.PARAM_EMAIL_SERVICE);
            EncryptionService = (IEncryption) paramContainer.GetKey(CommonConst.CommonValue.PARAM_ENCRYPTION_SERVICE);
            KeyValueStorage =(IKeyValueStorage) paramContainer.GetKey(CommonConst.CommonValue.PARAM_KEY_VALUE_STORAGE);
        }
    }
}