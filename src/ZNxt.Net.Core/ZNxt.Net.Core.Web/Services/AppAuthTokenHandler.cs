using IdentityServer4.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZNxt.Identity.Services;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Web.Models;
using ZNxt.Net.Core.Web.Services;
using ZNxt.Net.Core.Web.Services.SSO;

namespace ZNxt.Net.Core.Web.Services
{
    public static class AppAuthTokenHelper
    {
        public static Dictionary<string, string> ClientIdEncryptionBits = new Dictionary<string, string>()
        {
            ["m"] = "0",
            ["v"] = "0",
            ["p"] = "0",
            ["z"] = "1",
            ["x"] = "1",
            ["y"] = "1",
            ["w"] = "2",
            ["r"] = "2",
            ["t"] = "2",
            ["6"] = "3",
            ["4"] = "3",
            ["2"] = "3",
            ["d"] = "4",
            ["e"] = "4",
            ["f"] = "4",
            ["g"] = "5",
            ["h"] = "5",
            ["i"] = "5",
            ["j"] = "6",
            ["k"] = "6",
            ["l"] = "6",
            ["8"] = "7",
            ["n"] = "7",
            ["1"] = "7",
            ["q"] = "8",
            ["R"] = "8",
            ["s"] = "8",
            ["T"] = "9",
            ["7"] = "9",
            ["9"] = "9"
        };
    }
    public class AppAuthTokenHandler : IAppAuthTokenHandler
    {
        protected const string collection = "app_temp_token";
        protected readonly IDBService _dBService;
        protected readonly ILogger _logger;
        private readonly IEncryption _encryption;
        private readonly long AppTokenValidationDuration = 1000 * 60; // one minutes in 
        private readonly IOAuthClientService _oAuthClientService;
        private readonly ZNxtUserStore _userService;
        private readonly IInMemoryCacheService _inMemoryCacheService;
        private readonly IApiGatewayService _apiGateway;
        public AppAuthTokenHandler(IDBService dBService, ILogger logger, IEncryption encryption, IOAuthClientService oAuthClientService, ZNxtUserStore users, IInMemoryCacheService inMemoryCacheService, IApiGatewayService apiGateway)
        {
            _dBService = dBService;
            _logger = logger;
            _encryption = encryption;
            _oAuthClientService = oAuthClientService;
            _userService = users;
            _inMemoryCacheService = inMemoryCacheService;
            _apiGateway = apiGateway;

            var data = CommonUtility.GetAppConfigValue("AppTokenValidationDuration");
            if (!string.IsNullOrEmpty(data))
            {
                long duration;
                if (long.TryParse(data, out duration))
                {
                    AppTokenValidationDuration = duration;
                }
            }
        }
        public virtual TimeSpan GetLoginDuration()
        {
            return TimeSpan.FromDays(1);
        }

        public AppTokenModel GetTokenModel(string OAuthClientId, string token)
        {
            try
            {
                _logger.Debug($"Calling GetTokenModel token: {token}, OAuthClientId:{OAuthClientId}");

                var clientid = GetClientId(token);
                _logger.Debug($"Client id {clientid}");
                var client = _oAuthClientService.GetClient(clientid);
                if (client == null || client.Client == null)
                {
                    _logger.Error($"client id not found {client}");
                    return null;
                }

                var model = DecryptToken(client, token);
                model.tenant_id = client.TenantId.ToString();
                if (model != null)
                {
                    bool validate = ValidateTokenExpiry(model);
                    if (validate)
                    {
                        JObject filter = new JObject();
                        filter["url_token"] = token;

                        // create user id
                        var dbdata = _dBService.Get(collection, new ZNxt.Net.Core.Model.RawQuery(filter.ToString()));
                        if (dbdata == null || !dbdata.Any())
                        {
                            JObject data = model.ToJObject();
                            data["url_token"] = token;
                            data["user_name"] = GetUserName(model);
                            data[CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
                            if (_dBService.Write(collection, data))
                            {

                                return model;
                            }
                            else
                            {
                                _logger.Error($"Error while writting data to db Token : {token}", null, data);
                                return null;
                            }
                        }
                        else
                        {
                            _logger.Error($"token already  used  : {token}");
                            return null;
                        }
                    }
                    else
                    {
                        _logger.Error($"Token expired  {token}, model : { model.ToJObject()} ");
                        return null;
                    }
                }
                else
                {
                    _logger.Error($"Error on DecryptToken {token}");
                    return null;
                }
            }
            catch (Exception ex)
            {

                _logger.Error($"error : token {token}. {ex.Message}", ex);
                return null;
            }
        }
        
        private bool ValidateTokenExpiry(AppTokenModel model)
        {
            return ((DateTime.UtcNow - model.created_on).TotalMilliseconds < AppTokenValidationDuration);
        }

        private AppTokenModel DecryptToken(OAuthClient client, string token)
        {

            var trimtoken = token.Substring(20, token.Length - 20);
            _logger.Debug($"DecryptToken id {trimtoken}");
            var data = _encryption.Decrypt(trimtoken, client.EncryptionKey);
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<AppTokenModel>(data);
            return model;
        }

        private string GetClientId(string token)
        {
            string clientid = "";
            if (token.Length > 20)
            {
                clientid = token.Substring(0, 20).Replace("#","");
            }
            return clientid;
        }

        public bool IsInAction()
        {
            return true;
        }

        public virtual string LoginFailRedirect()
        {
            
            return CommonUtility.GetAppConfigValue(CommonConst.CommonValue.APP_TOKEN_UNAUTHORIZED_PAGE);
        }

        public bool Validate(string username, string token)
        {
            JObject filter = new JObject();
            filter["url_token"] = token;
            filter["user_name"] = username;
            var dbdata = _dBService.Get(collection, new ZNxt.Net.Core.Model.RawQuery(filter.ToString()));
            if (dbdata == null || !dbdata.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public UserModel GetUser(AppTokenModel token)
        {
            _logger.Debug($"GetUser id {GetUserName(token)}", token.ToJObject());

            return new UserModel()
            {
                user_id = GetUserName(token)
            };

            //return _userService.FindByUsername(GetUserName(token));
        }

        private static string GetUserName(AppTokenModel token)
        {
            return $"{token.tenant_id}_{token.user_id}";
        }
    }
}
