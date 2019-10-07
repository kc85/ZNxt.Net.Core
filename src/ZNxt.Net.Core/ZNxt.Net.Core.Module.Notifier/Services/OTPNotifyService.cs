﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Enums;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Module.Notifier.Services
{
    class OTPNotifyService : IOTPNotifyService
    {
        private readonly ILogger _logger;
        private readonly IDBService _dbService;
        private readonly ISMSNotifyService _smsService;
        private readonly IEmailNotifyService _emailService;
        private readonly IAppSettingService _appSettingService;
        public OTPNotifyService(
                          IDBService dbService,
                          IAppSettingService appSettingService,
                          ISMSNotifyService smsService,
                          IEmailNotifyService emailService,
                          ILogger logger
                          )
        {
            _logger = logger;
            _smsService = smsService;
            _dbService = dbService;
            _emailService = emailService;
            _appSettingService = appSettingService;
        }
        public bool SendSMS(string phoneNumber, string smsTemplateText, string otpType, string securityToken)
        {
            var otpData = CreateOTPData(otpType, securityToken);
            otpData[CommonConst.CommonField.PHONE] = phoneNumber;

            if (_dbService.Write(CommonConst.Collection.OTPs, otpData))
            {
                smsTemplateText = smsTemplateText.Replace("{{OTP}}", otpData[CommonConst.CommonField.OTP].ToString());
               return  _smsService.Send(phoneNumber, smsTemplateText);
            }
            else
            {
                return false;
            }
        }

        public bool SendEmail(string email, string emailTemplateText, string subject, string otpType, string securityToken)
        {
            var otpData = CreateOTPData(otpType, securityToken);
            otpData[CommonConst.CommonField.EMAIL] = email;

            if (_dbService.Write(CommonConst.Collection.OTPs, otpData))
            {
                List<string> to = new List<string>() { email };
                var fromEmail = _appSettingService.GetAppSettingData(CommonConst.CommonField.FROM_EMAIL_ID);
                emailTemplateText = emailTemplateText.Replace("{{OTP}}", otpData[CommonConst.CommonField.OTP].ToString());
                return _emailService.Send(to, fromEmail, null, emailTemplateText, subject);
            }
            else
            {
                return false;
            }
        }
        public bool ValidateSMS(string phoneNumber, string otp, string otpType, string securityToken = null)
        {
            JObject filter = new JObject();
            filter[CommonConst.CommonField.OTP] = otp;
            filter[CommonConst.CommonField.PHONE] = phoneNumber;
            filter[CommonConst.CommonField.STATUS] = OTPStatus.New.ToString();
            filter[CommonConst.CommonField.OTP_TYPE] = otpType.ToString();

            var otpDataArr = _dbService.Get(CommonConst.Collection.OTPs, new RawQuery(filter.ToString()) );
            if (otpDataArr.Count != 0)
            {
                var otpData = otpDataArr.First() as JObject;
                otpData[CommonConst.CommonField.STATUS] = OTPStatus.Used.ToString();
                if (_dbService.Update(CommonConst.Collection.OTPs, new RawQuery(filter.ToString()), otpData ) ==1)
                {
                    if (!string.IsNullOrEmpty(securityToken))
                    {
                        return otpData[CommonConst.CommonField.SECURITY_TOKEN].ToString() == securityToken;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    _logger.Error("Error updating OTP status on DB");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool ValidateEmail(string email, string otp, string otpType, string securityToken)
        {
            Dictionary<string, string> filter = new Dictionary<string, string>();
            filter[CommonConst.CommonField.OTP] = otp;
            filter[CommonConst.CommonField.EMAIL] = email;
            filter[CommonConst.CommonField.STATUS] = OTPStatus.New.ToString();
            filter[CommonConst.CommonField.OTP_TYPE] = otpType.ToString();

            var otpData = _dbService.FirstOrDefault(CommonConst.Collection.OTPs, filter);
            if (otpData != null)
            {
                otpData[CommonConst.CommonField.STATUS] = OTPStatus.Used.ToString();
                if (_dbService.Write(CommonConst.Collection.OTPs, otpData, filter))
                {
                    if (!string.IsNullOrEmpty(securityToken))
                    {
                        return otpData[CommonConst.CommonField.SECURITY_TOKEN].ToString() == securityToken;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    _logger.Error("Error updating OTP status on DB");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private JObject CreateOTPData(string otpType, string securityToken)
        {
            var otp = CommonUtility.RandomNumber(4);
            JObject otpData = new JObject();
            otpData[CommonConst.CommonField.ID] = CommonUtility.GetNewID();
            otpData[CommonConst.CommonField.OTP] = otp;
            otpData[CommonConst.CommonField.SECURITY_TOKEN] = securityToken;
            otpData[CommonConst.CommonField.OTP_TYPE] = otpType.ToString();
            otpData[CommonConst.CommonField.DURATION] = 15;
            otpData[CommonConst.CommonField.STATUS] = OTPStatus.New.ToString();
            return otpData;
        }
    }
}
