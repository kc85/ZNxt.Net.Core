using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Module.Identity.MySql.Services.API.Models;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.MySql.Services.API
{
    public class AppTokenAuthController : ZNxt.Net.Core.Services.ApiBaseService
    {
        protected readonly IResponseBuilder _responseBuilder;
        protected readonly IHttpContextProxy _httpContextProxy;
        protected readonly IAppAuthTokenHandler _appAuthTokenHandler;
        protected readonly ILogger _logger;
        public AppTokenAuthController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IAppAuthTokenHandler appAuthTokenHandler)
       : base(httpContextProxy, dBService, logger, responseBuilder)
        {

            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _appAuthTokenHandler = appAuthTokenHandler;
        }

        [Route("/sso/authapptoken", CommonConst.ActionMethods.POST, CommonConst.CommonField.API_AUTH_TOKEN)]
        public JObject AuthAppTokenValidate()
        {
            try
            {
                _logger.Debug("calling AuthAppTokenValidate");
                var token = _httpContextProxy.GetRequestBody<JObject>();
                var data = _appAuthTokenHandler.GetTokenModel("znxt", token["token"]?.ToString());
                if (data != null)
                {
                    var user = _appAuthTokenHandler.GetUser(data);
                    if (user != null)
                    {
                        return _responseBuilder.Success(user.ToJObject());
                    }
                    else
                    {
                        _logger.Error("AuthAppTokenValidate GetUser fail", null, data.ToJObject());
                        return _responseBuilder.Unauthorized();
                    }
                }
                else
                {
                    _logger.Error("AuthAppTokenValidate Token validation fail", null, token);
                    return _responseBuilder.Unauthorized();
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
