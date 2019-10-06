using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Helpers;
using System.Linq;

namespace ZNxt.Net.Core.Module.Notifier.Services.Api
{
    public class NotifierController
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IDBService _dbService;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        private readonly IEmailService _emailService;
        private readonly IAppSettingService _appSettingService;

        public NotifierController(IResponseBuilder responseBuilder, IDBService dbService, IHttpContextProxy httpContextProxy, IAppSettingService appSettingService,ILogger logger)
        {
            
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _appSettingService = appSettingService;
            _emailService = new EmailService(dbService, appSettingService, logger) ;
        }
        [Route("/notifier/send", CommonConst.ActionMethods.POST, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject Send()
        {

            var model = _httpContextProxy.GetRequestBody<NotifyModel>();
            if (model == null)
            {
                return _responseBuilder.BadRequest();
            }

            if(_emailService.Send(model.To.Split(';').ToList(), _appSettingService.GetAppSettingData(CommonConst.CommonField.FROM_EMAIL_ID), model.CC.Split(';').ToList(), model.Message, model.Subject))
            {
                return _responseBuilder.Success();
            }
            else
            {
                return _responseBuilder.ServerError();
            }
        }
    }
}
