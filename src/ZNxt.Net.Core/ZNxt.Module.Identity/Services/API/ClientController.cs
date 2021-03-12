using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Module.Identity.Services.API;
using ZNxt.Module.Identity.Services.API.Models;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.Services.API
{
    public class ClientController : IdentityControllerBase
    {
        private const string CollectionName = "oauth_clients";
        public ClientController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler, IApiGatewayService apiGatewayService, IZNxtUserService zNxtUserService)
       : base(responseBuilder, logger, httpContextProxy, dBService, keyValueStorage, staticContentHandler, apiGatewayService, zNxtUserService)
        {

        }

        [Route("/sso/oauthclient", CommonConst.ActionMethods.GET, CommonConst.CommonField.SYS_ADMIN_ROLE)]
        public JObject GetOAuthClient()
        {
            try
            {
                return GetPaggedData(CollectionName, null, null, null, new List<string>() { "client_id", "name", "id", "allowed_scopes", "is_active", "client_secret" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
        [Route("/sso/oauthclient/byname", CommonConst.ActionMethods.GET, CommonConst.CommonField.API_AUTH_TOKEN)]
        public JObject GetOAuthClientByName()
        {
            try
            {
                JObject filter = new JObject()
                {
                    ["name"] = _httpContextProxy.GetQueryString("name"),
                    ["is_active"] = true
                };
                var data = _dBService.Get(CollectionName, new RawQuery(filter.ToString()));
                if (data.Any())
                {
                    return _responseBuilder.Success(data.First());
                }
                else
                {
                    return _responseBuilder.NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
        [Route("/sso/oauthclient/byid", CommonConst.ActionMethods.GET, CommonConst.CommonField.API_AUTH_TOKEN)]
        public JObject GetOAuthClientById()
        {
            try
            {
                JObject filter = new JObject()
                {
                    ["client_id"] = _httpContextProxy.GetQueryString("client_id"),
                    ["is_active"] = true
                };
                var data = _dBService.Get(CollectionName, new RawQuery(filter.ToString()));
                if (data.Any())
                {
                    return _responseBuilder.Success(data.First());
                }
                else
                {
                    return _responseBuilder.NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
        [Route("/sso/oauthclient/edit", CommonConst.ActionMethods.POST, CommonConst.CommonField.SYS_ADMIN_ROLE)]
        public JObject EditClient()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<OAuthClientModel>();
                _logger.Debug("Validation model");
                var results = new Dictionary<string, string>();
                if (request.IsValidModel(out results))
                {

                    JObject filter = new JObject()
                    {
                        ["id"] = request.id
                    };
                    if (_dBService.Get(CollectionName, new RawQuery(filter.ToString())).Any())
                    {
                        if (_dBService.Update(CollectionName, new RawQuery(filter.ToString()), request.ToJObject(), false, MergeArrayHandling.Replace) == 1)
                        {
                            return _responseBuilder.Success();
                        }
                        else
                        {
                            return _responseBuilder.ServerError();
                        }
                    }
                    else
                    {
                        JObject errors = new JObject();
                        errors["Error"] = $"client not found, {request.name}";
                        return _responseBuilder.NotFound(errors);
                    }
                }
                else
                {
                    _logger.Debug("Model validation fail");
                    JObject errors = new JObject();
                    foreach (var error in results)
                    {
                        errors[error.Key] = error.Value;
                    }
                    return _responseBuilder.BadRequest(errors);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
        [Route("/sso/oauthclient/add", CommonConst.ActionMethods.POST, CommonConst.CommonField.SYS_ADMIN_ROLE)]
        public JObject AddClient()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<OAuthClientModel>();
                _logger.Debug("Validation model");
                var results = new Dictionary<string, string>();
                if (request.IsValidModel(out results))
                {
                   
                    JObject filter = new JObject()
                    {
                        ["client_id"] = request.client_id
                    };
                    if (!_dBService.Get(CollectionName, new RawQuery(filter.ToString())).Any())
                    {
                        request.client_secret = CommonUtility.RandomString(15);
                        request.is_active = true;
                        request.id = CommonUtility.GetNewID();
                        if (!request.allowed_scopes.Any())
                        {
                            request.allowed_scopes.Add(CommonConst.CommonField.USER_ROLE);
                        }
                        if (_dBService.Write(CollectionName, request.ToJObject()))
                        {
                            return _responseBuilder.Success(request.ToJObject());
                        }
                        else
                        {
                            return _responseBuilder.ServerError();
                        }
                    }
                    else
                    {
                        JObject errors = new JObject();
                        errors["Error"] = $"Duplicate client id, {request.client_id}";
                        return _responseBuilder.BadRequest(errors);
                    }
                }
                else
                {
                    _logger.Debug("Model validation fail");
                    JObject errors = new JObject();
                    foreach (var error in results)
                    {
                        errors[error.Key] = error.Value;
                    }
                    return _responseBuilder.BadRequest(errors);
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
