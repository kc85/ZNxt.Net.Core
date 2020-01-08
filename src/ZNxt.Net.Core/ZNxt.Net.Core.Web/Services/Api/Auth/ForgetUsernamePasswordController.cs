using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Web.Helper;
using ZNxt.Net.Core.Web.Services.Api.Auth.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ZNxt.Net.Core.Config;
using System.Linq;

namespace ZNxt.Net.Core.Web.Services.Api.Auth
{
    public class ForgetUsernamePasswordController
    {
        private readonly IZNxtUserService _znxtUserService;
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        private readonly IApiGatewayService _apiGatewayService;
        private readonly IUserNotifierService _userNotifierService;
        public ForgetUsernamePasswordController(IZNxtUserService zNxtUserService, IUserNotifierService userNotifierService, IApiGatewayService apiGatewayService, ILogger logger, IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _znxtUserService = zNxtUserService;
            _userNotifierService = userNotifierService;
            _logger = logger;
            _apiGatewayService = apiGatewayService;
        }
        [Route("/user/forgetusername", CommonConst.ActionMethods.POST, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject ForgetUsername()
        {
            try
            {
                var email = _httpContextProxy.GetQueryString(CommonConst.CommonField.EMAIL);
                if (email == null)
                {
                    return _responseBuilder.BadRequest();
                }
                var users = _znxtUserService.GetUsersByEmail(email);
                if (users.Any())
                {
                    if (_userNotifierService.SendForgetUsernamesEmailAsync(users).GetAwaiter().GetResult())
                    {
                        return _responseBuilder.Success();
                    }
                }
                else
                {
                    _logger.Error($"No user found for email : {email}");
                }
                return _responseBuilder.Success();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
        [Route("/user/forgetpass/sendotp", CommonConst.ActionMethods.POST, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject ForgetPasswordSendOTP()
        {
            try
            {
                var userName = _httpContextProxy.GetQueryString("user_name");
                if (userName == null)
                {
                    return _responseBuilder.BadRequest();
                }
                var user = _znxtUserService.GetUserByUsername(userName);
                if (user != null)
                {
                    if (_userNotifierService.SendForgetpasswordEmailWithOTPAsync(user).GetAwaiter().GetResult())
                    {
                        return _responseBuilder.Success();
                    }
                    else
                    {
                        _logger.Error("Error on SendForgetpasswordEmailWithOTPAsync");
                        return _responseBuilder.ServerError();
                    }
                }
                else
                {
                    _logger.Error($"No user found for user_name : {userName}");
                    return _responseBuilder.NotFound();
                }
                
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
    }
}
