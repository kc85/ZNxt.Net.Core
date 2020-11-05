using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Enums;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Module.Notifier.Services
{
    public class SMSNotifyService : ISMSNotifyService
    {
        private readonly IDBService _dbService;
        private readonly ILogger _logger;
        private readonly IAppSettingService _appSettingService;
        public SMSNotifyService(IDBService dBService, IAppSettingService appSettingService, ILogger logger)
        {
            _logger = logger;
            _dbService = dBService;
            _appSettingService = appSettingService;
        }
        public Dictionary<string, bool> Send(List<string> to, string message)
        {
            Dictionary<string, bool> status = new Dictionary<string, bool>();
            foreach (var toSms in to)
            {
                status[toSms] = Send(toSms,message);
            }
            return status;
        }

        public bool Send(string toSms, string message)
        {
            JObject smsData = new JObject();
            smsData[CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
            smsData[CommonConst.CommonField.TO] = toSms;
            smsData[CommonConst.CommonField.BODY] = message;
            smsData[CommonConst.CommonField.STATUS] = SMSStatus.Queue.ToString();

            if (_dbService.Write(CommonConst.Collection.SMS_QUEUE, smsData))
            {
                Dictionary<string, string> filter = new Dictionary<string, string>();
                filter[CommonConst.CommonField.DISPLAY_ID] = smsData[CommonConst.CommonField.DISPLAY_ID].ToString();
                try
                {
                    if (_appSettingService.GetAppSettingData("sms_provider") == "PSBULKSMS")
                    {

                        if (PsbulkSMSHelper.SendSMS(
                              message,
                              toSms,
                              _appSettingService.GetAppSettingData("sms_gateway_key"),
                              _appSettingService.GetAppSettingData("gateway_endpoint"),
                              _appSettingService.GetAppSettingData("sms_from"),
                              _logger))
                        {
                            smsData[CommonConst.CommonField.STATUS] = EmailStatus.Sent.ToString();
                            _dbService.Write(CommonConst.Collection.SMS_QUEUE, smsData, filter);
                            return true;

                        }
                        else
                        {
                            smsData[CommonConst.CommonField.STATUS] = SMSStatus.SendError.ToString();
                            _dbService.Write(CommonConst.Collection.SMS_QUEUE, smsData, filter);
                            return false;
                        }
                    }
                    else
                    {
                        if (TextLocalSMSHelper.SendSMS(
                              message,
                              toSms,
                              _appSettingService.GetAppSettingData("sms_gateway_key"),
                              _appSettingService.GetAppSettingData("gateway_endpoint"),
                              _appSettingService.GetAppSettingData("sms_from"),
                              _logger))
                        {
                            smsData[CommonConst.CommonField.STATUS] = EmailStatus.Sent.ToString();
                            _dbService.Write(CommonConst.Collection.SMS_QUEUE, smsData, filter);
                            return true;

                        }
                        else
                        {
                            smsData[CommonConst.CommonField.STATUS] = SMSStatus.SendError.ToString();
                            _dbService.Write(CommonConst.Collection.SMS_QUEUE, smsData, filter);
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    smsData[CommonConst.CommonField.STATUS] = SMSStatus.SendError.ToString();
                    _dbService.Write(CommonConst.Collection.SMS_QUEUE, smsData, filter);
                    _logger.Error("Error SMS send ", ex);
                    return false;
                }
            }
            else
            {
                _logger.Error("Error in add SMS data in queue");
                return false;
            }
        }
    }
}
