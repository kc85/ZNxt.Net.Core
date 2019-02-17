using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Web.Helper;
using ZNxt.Net.Core.Web.Services.Api.AuthToken.Model;

namespace ZNxt.Net.Core.Web.Services.Api.AuthToken
{
    public class AuthTokenController
    {
        private const int MAX_KEYS = 5;

        private readonly IDBService _dbService;
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly IDBServiceConfig _dbConfig;
        private readonly IServiceResolver _serviceResolver;
        private readonly ILogger _logger;
        public AuthTokenController(ILogger logger, IDBService dbService, IServiceResolver serviceResolver, IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy, IDBServiceConfig dbConfig)
        {
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _dbConfig = dbConfig;
            _serviceResolver = serviceResolver;
            _logger = logger;
        }

        [Route("/authtoken/generate", CommonConst.ActionMethods.POST)]
        public JObject Generate()
        {
            var data = _httpContextProxy.GetRequestBody<GenerateAuthTokenRequest>();
            var userAccontHelper = _serviceResolver.Resolve<UserAccontHelper>();
            if (userAccontHelper.ValidateUser(data.UserName, data.Password))
            {
                var user = userAccontHelper.GetUser(data.UserName);
                if(GetAuthTokenCount(_dbService, user) < MAX_KEYS)
                {
                    var apikey = GenerateApiKey();
                    var apikeydata = GenerateApiKeyData(user, apikey);

                    if (_dbService.WriteData(CommonConst.Collection.AUTH_TOKEN_COLLECTION, apikeydata))
                    {
                        apikeydata[CommonConst.CommonField.AUTH_TOKEN] = apikey;
                        return _responseBuilder.Success(apikeydata);
                    }
                    else
                    {
                        _logger.Error("Error in writing data");
                        return _responseBuilder.ServerError();
                    }
                }
                else
                {
                    return _responseBuilder.CreateReponse(ApiKeyResponseCode._MAX_AUTH_TOKEN_REACHED);
                }
            }
            else
            {
                return _responseBuilder.Unauthorized();
            }
        }

        private long  GetAuthTokenCount(IDBService dbService, UserModel userData)
        {
            DBQuery query = new DBQuery() { Filters = new FilterQuery() { new Filter(CommonConst.CommonField.USER_ID, userData.user_id) } };
            return _dbService.GetCount(CommonConst.Collection.AUTH_TOKEN_COLLECTION, query.Filters);
            
        }

        private string GenerateApiKey()
        {
            return string.Format("{0}{1}{2}{3}", CommonUtility.RandomString(10), CommonUtility.RandomNumber(6), CommonUtility.RandomString(10), CommonUtility.RandomNumber(6));
        }

        private JObject GenerateApiKeyData(UserModel userData, string key)
        {
            JObject data = new JObject
            {
                [CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID(),
                [CommonConst.CommonField.USER_ID] = userData.user_id,
                [CommonConst.CommonField.AUTH_TOKEN] = _serviceResolver.Resolve<IEncryption>().Encrypt(key),
                [CommonConst.CommonField.IS_ENABLED] = true
            };

            return data;
        }

    }
}
