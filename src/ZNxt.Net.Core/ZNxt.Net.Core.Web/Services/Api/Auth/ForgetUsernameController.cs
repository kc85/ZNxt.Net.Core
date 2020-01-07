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
    public class ForgetUsernameController
    {
        private readonly IZNxtUserService _znxtUserService;
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        private readonly IApiGatewayService _apiGatewayService;
        public ForgetUsernameController(IZNxtUserService zNxtUserService,IApiGatewayService apiGatewayService, ILogger logger, IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _znxtUserService = zNxtUserService;
            _logger = logger;
            _apiGatewayService = apiGatewayService;
        }
        [Route("/user/forgetusername", CommonConst.ActionMethods.POST,CommonConst.CommonValue.ACCESS_ALL)]
        public JObject ForgetUsername()
        {
            try
            {   
                var email = _httpContextProxy.GetQueryString(CommonConst.CommonField.EMAIL);
                if (email == null)
                {
                    return _responseBuilder.BadRequest();
                }
                var users =  _znxtUserService.GetUsersByEmail(email);
                if (users.Any())
                {
                    _logger.Debug($"Sending forgetusername email to {email}");
                    var templateRequest = new JObject()
                    {
                        ["key"] = "forgetuser_name",
                        ["usernames"] = string.Join("<br>", users.Select(f => f.user_name).ToList()),
                        ["appname"] = ApplicationConfig.AppName,
                        ["appurl"] = ApplicationConfig.AppEndpoint
                    };

                    var resultTemplateBase = _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/template/process", "", templateRequest, null).GetAwaiter().GetResult();
                    _logger.Debug("template/process response", resultTemplateBase);
                    var resultTemplate = resultTemplateBase["data"] as JObject;
                    if (resultTemplate["data"] != null && resultTemplate["subject"] != null && !string.IsNullOrEmpty(resultTemplate["data"].ToString()))
                    {
                        var emailModel = new JObject()
                        {
                            ["Subject"] = resultTemplate["subject"],
                            ["To"] = email,
                            ["Message"] = resultTemplate["data"]
                        };
                        var result = _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/send", "", emailModel, null).GetAwaiter().GetResult();

                        if (result["code"].ToString() == "1")
                        {
                            return _responseBuilder.Success();
                        }
                    }
                    else
                    {
                        _logger.Error($"Error while processing the template. Request :{templateRequest.ToString() }", null, resultTemplateBase);
                        return _responseBuilder.ServerError(); ;
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
    }
}
