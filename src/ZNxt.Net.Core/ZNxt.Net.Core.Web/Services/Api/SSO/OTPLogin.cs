using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services.Api.SSO
{
    public class OTPLogin
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        private readonly IDBService _dBService;
        private readonly IApiGatewayService _apiGatewayService;
        private readonly IZNxtUserService _ZNxtUserService;
        public OTPLogin(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IZNxtUserService ZNxtUserService)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _ZNxtUserService = ZNxtUserService;
            _logger = logger;
            _dBService = dBService;
        }
        [Route("/sso/otplogin/required", CommonConst.ActionMethods.POST, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject IsOTPLoginRequired()
        {
            try
            {
                var user_name = _httpContextProxy.GetQueryString("user_name");
                if (!string.IsNullOrEmpty(user_name))
                {
                    var data = _ZNxtUserService.GetUserByUsername(user_name);
                    if (data!=null)
                    {
                        if (data.roles.Where(f => f == "init_login_email_otp").Any())
                        {
                            return _responseBuilder.Success();
                        }
                        else
                        {
                            return _responseBuilder.BadRequest();
                        }
                    }
                    else
                    {
                        return _responseBuilder.BadRequest();
                    }
                }
                else
                {
                    return _responseBuilder.BadRequest();
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
