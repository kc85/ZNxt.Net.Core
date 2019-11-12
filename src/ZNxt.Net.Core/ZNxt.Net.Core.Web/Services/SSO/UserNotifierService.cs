using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Consts;

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

        public async Task<bool> SendWelcomeEmailAsync(UserModel user)
        {
            try
            { 
                _logger.Debug($"Sending welcome email to {user.email}");
                var templeteRequest = new JObject()
                {
                    ["key"] = "registration_confirmation",
                    ["userdisplayname"] = user.name,
                    ["userloginemail"] = user.email,
                    ["userlogin"] = user.email
                };

               var resultTemplete  = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/template/process", "", templeteRequest, null);

                if (resultTemplete["data"] !=null && !string.IsNullOrEmpty(resultTemplete["data"].ToString()))
                {
                    var emailModel = new JObject()
                    {
                        ["Subject"] = "Registration confirmation ZNxt.App",
                        ["To"] = user.email,
                        ["Message"] = resultTemplete["data"]
                    };
                    var result = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/send", "", emailModel, null);

                    return result["code"].ToString() == "1";
                }
                else
                {
                    _logger.Error($"Error while processing the templete. Request :{templeteRequest.ToString() }");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending welcome email to {user.email}. Error:{ex.Message}", ex);
                return false;
            }
        }
        public async Task<bool> SendWelcomeEmailWithOTPLoginAsync(UserModel user)
        {
            try
            {
                _logger.Debug($"Sending welcome email to {user.email}");
                var templeteRequest = new JObject()
                {
                    ["key"] = "registration_with_email_otp",
                    ["userdisplayname"] = user.name,
                    ["userloginemail"] = user.email,
                    ["userlogin"] = user.email
                };

                var resultTemplete = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/template/process", "", templeteRequest, null);

                if (resultTemplete["data"] != null && !string.IsNullOrEmpty(resultTemplete["data"].ToString()))
                {
                    var emailModel = new JObject()
                    {
                        ["Subject"] = "Registration confirmation ZNxt.App",
                        ["To"] = user.email,
                        ["Message"] = resultTemplete["data"]
                    };
                    var result = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/send", "", emailModel, null);

                    return result["code"].ToString() == "1";
                }
                else
                {
                    _logger.Error($"Error while processing the templete. Request :{templeteRequest.ToString() }");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending welcome email to {user.email}. Error:{ex.Message}", ex);
                return false;
            }
        }
    }
}
