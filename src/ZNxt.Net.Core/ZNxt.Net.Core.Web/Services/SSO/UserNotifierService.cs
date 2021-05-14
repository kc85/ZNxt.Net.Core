using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace ZNxt.Identity.Services
{
    public class UserNotifierService : IUserNotifierService
    {
        private readonly IApiGatewayService _apiGatewayService;
        private readonly ILogger _logger;
        public UserNotifierService(IApiGatewayService apiGatewayService, ILogger logger)
        {
            _apiGatewayService = apiGatewayService;
            _logger = logger;
        }

        public async Task<bool> SendForgetpasswordEmailWithOTPAsync(UserModel user, string message = null)
        {
            try
            {
                _logger.Debug($"Sending forget password OTP email to {user.email}");
                var template_key = "forgetpassword_with_email_otp";
                var templateRequest = new JObject()
                {
                    ["key"] = template_key,
                    ["userdisplayname"] = user.GetDisplayName(),
                    ["userloginemail"] = user.email,
                    ["userlogin"] = user.user_name,
                    ["appname"] = ApplicationConfig.AppName,
                    ["appurl"] = ApplicationConfig.AppEndpoint,
                    ["message"] = message
                };

                var resultTemplateBase = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/template/process", "", templateRequest, null);
                _logger.Debug("template/process response", resultTemplateBase);
                var resultTemplate = resultTemplateBase["data"] as JObject;
                if (resultTemplate["data"] != null && resultTemplate["subject"] != null && !string.IsNullOrEmpty(resultTemplate["data"].ToString()))
                {
                    var emailModel = new JObject()
                    {
                        ["Subject"] = resultTemplate["subject"],
                        ["To"] = user.email,
                        ["Type"] = "Email",
                        ["Message"] = resultTemplate["data"],
                        ["OTPType"] = template_key,
                        ["Duration"] = (60 * 15).ToString(), // TODO : Need to move to config, right now confugure for 15 minutes
                        ["message"] = message
                    };
                    var result = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/otp/send", "", emailModel, null);
                    return result["code"].ToString() == "1";
                }
                else
                {
                    _logger.Error($"Error while processing the template. Request :{templateRequest.ToString() }", null, resultTemplateBase);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending welcome email to {user.email}. Error:{ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> SendForgetUsernamesEmailAsync(List<UserModel> users, string message = null)
        {
            _logger.Debug($"Sending forgetusername email to {users.First().email}");
            var templateRequest = new JObject()
            {
                ["key"] = "forgetuser_name",
                ["usernames"] = string.Join("<br>", users.Select(f => f.user_name).ToList()),
                ["appname"] = ApplicationConfig.AppName,
                ["appurl"] = ApplicationConfig.AppEndpoint,
                ["message"] = message
            };

            var resultTemplateBase = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/template/process", "", templateRequest, null);
            _logger.Debug("template/process response", resultTemplateBase);
            var resultTemplate = resultTemplateBase["data"] as JObject;
            if (resultTemplate["data"] != null && resultTemplate["subject"] != null && !string.IsNullOrEmpty(resultTemplate["data"].ToString()))
            {
                var emailModel = new JObject()
                {
                    ["Subject"] = resultTemplate["subject"],
                    ["To"] = users.First().email,
                    ["Message"] = resultTemplate["data"],
                    ["message"] = message
                };
                var result = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/send", "", emailModel, null);
                _logger.Debug("SendForgetUsernamesEmailWithOTPAsync: /notifier/send", result);
                return result["code"].ToString() == "1";
            }

            else
            {
                _logger.Error($"Error while processing the template. Request :{templateRequest.ToString() }", null, resultTemplateBase);
                return false ;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(UserModel user, string message = null)
        {
            try
            { 
                _logger.Debug($"Sending welcome email to {user.email}");
                var templateRequest = new JObject()
                {
                    ["key"] = "registration_confirmation",
                    ["userdisplayname"] = user.GetDisplayName(),
                    ["userloginemail"] = user.email,
                    ["userlogin"] = user.email,
                    ["appname"] =ApplicationConfig.AppName,
                    ["appurl"] = ApplicationConfig.AppEndpoint,
                    ["message"] = message
                };

               var resultTemplateBase  = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/template/process", "", templateRequest, null);
                _logger.Debug("template/process response", resultTemplateBase);
                var resultTemplate = resultTemplateBase["data"] as JObject;
                if (resultTemplate["data"] !=null  && resultTemplate["subject"] !=null && !string.IsNullOrEmpty(resultTemplate["data"].ToString()))
                {
                    var emailModel = new JObject()
                    {
                        ["Subject"] = resultTemplate["subject"],
                        ["To"] = user.email,
                        ["Message"] = resultTemplate["data"]
                    };
                    var result = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/send", "", emailModel, null);

                    return result["code"].ToString() == "1";
                }
                else
                {
                    _logger.Error($"Error while processing the template. Request :{templateRequest.ToString() }",null, resultTemplateBase);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending welcome email to {user.email}. Error:{ex.Message}", ex);
                return false;
            }
        }
        public async Task<bool> SendWelcomeEmailWithOTPLoginAsync(UserModel user, string message = null)
        {
            try
            {
                _logger.Debug($"Sending welcome email to {user.email}");
                var template_key = "registration_with_email_otp";
                var templateRequest = new JObject()
                {
                    ["key"] = template_key,
                    ["userdisplayname"] = user.GetDisplayName(),
                    ["userloginemail"] = user.email,
                    ["userlogin"] = user.user_name,
                    ["appname"] = ApplicationConfig.AppName,
                    ["appurl"] = ApplicationConfig.AppEndpoint,
                    ["message"] = message
                };

                var resultTemplateBase = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/template/process", "", templateRequest, null);
                _logger.Debug("template/process response", resultTemplateBase);
                var resultTemplate = resultTemplateBase["data"] as JObject;
                if (resultTemplate["data"] != null  && resultTemplate["subject"]  !=null && !string.IsNullOrEmpty(resultTemplate["data"].ToString()))
                {
                    var emailModel = new JObject()
                    {
                        ["Subject"] = resultTemplate["subject"],
                        ["To"] = user.email,
                        ["Type"] = "Email",
                        ["Message"] = resultTemplate["data"],
                        ["OTPType"] = template_key,
                        ["Duration"] = (60*24*1).ToString() // TODO : Need to move to config, right now confugure for 2 days 
                    };
                    var result = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/otp/send", "", emailModel, null);
                    return result["code"].ToString() == "1";
                }
                else
                {
                    _logger.Error($"Error while processing the template. Request :{templateRequest.ToString() }",null, resultTemplateBase);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending welcome email to {user.email}. Error:{ex.Message}", ex);
                return false;
            }
        }
        public async Task<bool> SendMobileAuthRegistrationOTPAsync(MobileAuthRegisterResponse mobileAuth, string message = null)
        {
            try
            {
                var mobileNo = mobileAuth.mobile_number;
                var otpReqeust = new JObject()
                {
                    ["To"] = mobileNo,
                    ["Message"] = "Account Activation OTP is {{OTP}} ",
                    ["Type"] = "SMS",
                    ["OTPType"] = "mobile_auth_activation",
                    ["SecurityToken"] = mobileAuth.validation_token,
                    ["message"] = message
                };
                var result = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/otp/send", null, otpReqeust);
                return result["code"].ToString() == "1";
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending mobile_auth_registration to {mobileAuth.mobile_number}. Error:{ex.Message}", ex);
                return false;
            }
        }
    }
}
