﻿using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using System.Linq;

namespace ZNxt.Net.Core.Module.Notifier.Services.Api
{
    public class NotifierController
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IDBService _dbService;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        private readonly IEmailNotifyService _emailService;
        private readonly ISMSNotifyService _smsService;
        private readonly IOTPNotifyService _oTPNotifyService;

        private readonly IAppSettingService _appSettingService;

        public NotifierController(IResponseBuilder responseBuilder, IDBService dbService, IHttpContextProxy httpContextProxy, IAppSettingService appSettingService,ILogger logger)
        {
            
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _appSettingService = appSettingService;
            _emailService = new EmailNotifyService(dbService, appSettingService, logger) ;
            _smsService = new SMSNotifyService(dbService, appSettingService, logger);
            _oTPNotifyService = new OTPNotifyService(dbService, appSettingService, _smsService, _emailService, logger);
        }
        [Route("/notifier/send", CommonConst.ActionMethods.POST, CommonConst.CommonField.API_AUTH_TOKEN)]
        public JObject Send()
        {

            var model = _httpContextProxy.GetRequestBody<NotifyModel>();
            if (model == null)
            {
                return _responseBuilder.BadRequest();
            }
            var to = model.To.Split(';').ToList();
            var message = model.Message;
            if (model.Type == "SMS")
            {
                JObject dataresponse  =  new JObject();
                foreach (var res in _smsService.Send(to, message))
                {
                    dataresponse[res.Key] = res.Value;
                }
                return _responseBuilder.Success(dataresponse);
            }
            else
            {
                if (_emailService.Send(to, _appSettingService.GetAppSettingData(CommonConst.CommonField.FROM_EMAIL_ID), model.CC.Split(';').ToList(), message, model.Subject))
                {
                    return _responseBuilder.Success();
                }
                else
                {
                    return _responseBuilder.ServerError();
                }
            }
        }
        [Route("/notifier/otp/send", CommonConst.ActionMethods.POST, CommonConst.CommonField.API_AUTH_TOKEN)]
        public JObject SendOTP()
        {

            var model = _httpContextProxy.GetRequestBody<OTPModel>();
            if (model == null)
            {
                return _responseBuilder.BadRequest();
            }
            if (model.Duration == null)
            {
                var otpduration = _appSettingService.GetAppSettingData(CommonConst.CommonField.OTP_DURATION);
                long duration = 15;
                long.TryParse(otpduration, out duration);
                model.Duration = duration;
            }
            var message = model.Message;
            if (model.Type == "SMS")
            {
                if (_oTPNotifyService.SendSMS(model.To, model.Message, model.OTPType, model.SecurityToken, model.Duration.Value))
                {
                    return _responseBuilder.Success();
                }
                else
                {
                    return _responseBuilder.ServerError();
                }
            }
            else
            {
                if (_oTPNotifyService.SendEmail( model.To, model.Message, model.Subject, model.OTPType, model.SecurityToken, model.Duration.Value))
                {
                    return _responseBuilder.Success();
                }
                else
                {
                    return _responseBuilder.ServerError();
                }
            }
        }

        [Route("/notifier/otp/validate", CommonConst.ActionMethods.POST, CommonConst.CommonField.API_AUTH_TOKEN)]
        public JObject ValidateOTP()
        {
            try
            {
                var model = _httpContextProxy.GetRequestBody<OTPModel>();
                if (model == null)
                {
                    return _responseBuilder.BadRequest();
                }
                var message = model.Message;
                if (model.Type == "SMS")
                {
                    if (_oTPNotifyService.ValidateSMS(model.To, model.OTP, model.OTPType, model.SecurityToken))
                    {
                        return _responseBuilder.Success();
                    }
                    else
                    {
                        return _responseBuilder.ServerError();
                    }
                }
                else
                {
                    if (_oTPNotifyService.ValidateEmail(model.To, model.OTP, model.OTPType, model.SecurityToken))
                    {
                        return _responseBuilder.Success();
                    }
                    else
                    {
                        return _responseBuilder.ServerError();
                    }
                }

            }
            catch (System.Exception ex)
            {
                _logger.Error(ex.Message, ex);
              return  _responseBuilder.ServerError();
            }
        }


        [Route("/notifier/config", CommonConst.ActionMethods.GET, CommonConst.CommonValue.SYS_ADMIN)]
        public JObject GetConfig()
        {
            JObject configs = new JObject();
            configs["smtp_address"] = _appSettingService.GetAppSettingData(CommonConst.CommonField.SMTP_SERVER);
            configs["smtp_port"] = _appSettingService.GetAppSettingData(CommonConst.CommonField.SMTP_SERVER_PORT);
            configs["smtp_user"] = _appSettingService.GetAppSettingData(CommonConst.CommonField.SMTP_SERVER_USER);
            configs["smtp_pass"] = _appSettingService.GetAppSettingData(CommonConst.CommonField.SMTP_SERVER_PASSWORD);
            configs["from_email"] = _appSettingService.GetAppSettingData(CommonConst.CommonField.FROM_EMAIL_ID);
            configs["sms_provider"] = _appSettingService.GetAppSettingData("sms_provider");

            configs["sms_gateway_key"] = _appSettingService.GetAppSettingData("sms_gateway_key");
            configs["sms_gateway_endpoint"] = _appSettingService.GetAppSettingData("gateway_endpoint");
            configs["sms_from"] = _appSettingService.GetAppSettingData("sms_from");
            return _responseBuilder.Success(configs);
           
        }
        [Route("/notifier/config/save", CommonConst.ActionMethods.POST, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject SaveConfig()
        {
            var model = _httpContextProxy.GetRequestBody<JObject>();
            if (model == null)
            {
                return _responseBuilder.BadRequest();
            }

            _appSettingService.SetAppSetting(CommonConst.CommonField.SMTP_SERVER, model["smtp_address"].ToString());
            _appSettingService.SetAppSetting(CommonConst.CommonField.SMTP_SERVER_PORT, model["smtp_port"].ToString());
            _appSettingService.SetAppSetting(CommonConst.CommonField.SMTP_SERVER_USER, model["smtp_user"].ToString());
            _appSettingService.SetAppSetting(CommonConst.CommonField.SMTP_SERVER_PASSWORD, model["smtp_pass"].ToString());
            _appSettingService.SetAppSetting(CommonConst.CommonField.FROM_EMAIL_ID, model["from_email"].ToString());
            _appSettingService.SetAppSetting("sms_gateway_key", model["sms_gateway_key"].ToString());
            _appSettingService.SetAppSetting("gateway_endpoint", model["sms_gateway_endpoint"].ToString());
            _appSettingService.SetAppSetting("sms_from", model["sms_from"].ToString());
            _appSettingService.SetAppSetting("sms_provider", model["sms_provider"].ToString());
            return _responseBuilder.Success();

        }
    }
}
