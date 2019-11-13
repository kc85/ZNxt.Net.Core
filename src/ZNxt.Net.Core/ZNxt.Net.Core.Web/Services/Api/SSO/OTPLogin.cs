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
        public OTPLogin(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
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
                    var data = _dBService.Get(CommonConst.Collection.USERS, new RawQuery("{'email': '" + user_name + "','is_enabled':'true'}"));
                    if (data.Any())
                    {
                        if ((data.First()["roles"] as JArray).Where(f => f.ToString() == "init_login_email_otp").Any())
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
