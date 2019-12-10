using Newtonsoft.Json.Linq;
using System;
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
                var templateRequest = new JObject()
                {
                    ["key"] = "registration_confirmation",
                    ["userdisplayname"] = user.name,
                    ["userloginemail"] = user.email,
                    ["userlogin"] = user.email
                };

               var resultTemplate  = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/template/process", "", templateRequest, null);

                if (resultTemplate["data"] !=null && !string.IsNullOrEmpty(resultTemplate["data"].ToString()))
                {
                    var emailModel = new JObject()
                    {
                        ["Subject"] = "Registration confirmation ZNxt.App",
                        ["To"] = user.email,
                        ["Message"] = resultTemplate["data"]
                    };
                    var result = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/send", "", emailModel, null);

                    return result["code"].ToString() == "1";
                }
                else
                {
                    _logger.Error($"Error while processing the template. Request :{templateRequest.ToString() }");
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
                var template_key = "registration_with_email_otp";
                var templateRequest = new JObject()
                {
                    ["key"] = template_key,
                    ["userdisplayname"] = $"{user.first_name } {user.middle_name} {user.last_name}",
                    ["userloginemail"] = user.email,
                    ["userlogin"] = user.user_name
                };

                var resultTemplate = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/template/process", "", templateRequest, null);
                
                if (resultTemplate["data"] != null && !string.IsNullOrEmpty(resultTemplate["data"].ToString()))
                {
                    var emailModel = new JObject()
                    {
                        ["Subject"] = "Registration confirmation ZNxt.App",
                        ["To"] = user.email,
                        ["Type"] = "Email",
                        ["Message"] = resultTemplate["data"],
                        ["OTPType"] = template_key,
                        ["Duration"] = (60*24*2).ToString() // TODO : Need to move to config, right now confugure for 2 days 
                    };
                    var result = await _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/otp/send", "", emailModel, null);
                    return result["code"].ToString() == "1";
                }
                else
                {
                    _logger.Error($"Error while processing the template. Request :{templateRequest.ToString() }");
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
