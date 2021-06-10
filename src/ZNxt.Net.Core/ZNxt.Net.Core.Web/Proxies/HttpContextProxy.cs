using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using ZNxt.Net.Core.Consts;

namespace ZNxt.Net.Core.Web.Proxies
{
    public partial class HttpContextProxy : IHttpContextProxy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HttpContextProxy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
       
        //public Dictionary<string, string> ResponseHeaders { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int ResponseStatusCode => throw new NotImplementedException();

        public string ResponseStatusMessage => throw new NotImplementedException();

        public byte[] Response => throw new NotImplementedException();

        public string ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string SessionID => throw new NotImplementedException();

        public DateTime InitDateTime => throw new NotImplementedException();

        public string TransactionId => throw new NotImplementedException();

        public UserModel User
        {
            get
            {

                if (_httpContextAccessor.HttpContext!=null 
                    && _httpContextAccessor.HttpContext.User != null 
                    && _httpContextAccessor.HttpContext.User.Identity != null 
                    && _httpContextAccessor.HttpContext.User.Claims.Any())
                {
                    var user = new UserModel()
                    {
                        first_name = _httpContextAccessor.HttpContext.User.Identity.Name,

                    };
                    user.roles.Add("user");
                    var claims = new List<Claim>();
                    foreach (var claim in _httpContextAccessor.HttpContext.User.Claims)
                    {
                        claims.Add(new Claim(claim.Type, claim.Value));
                        if (claim.Type == "roles")
                        {
                            user.roles = JArray.Parse(claim.Value).Select(f => f.ToString()).ToList();
                        }
                        if (claim.Type == CommonConst.CommonValue.TENANT_KEY)
                        {
                            user.tenants = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TenantModel>>(claim.Value);
                        }
                        if (claim.Type == nameof(user.user_name))
                        {
                            user.user_name = claim.Value;
                        }
                        if (claim.Type == nameof(user.email))
                        {
                            user.email = claim.Value;
                        }
                        if (claim.Type == nameof(user.first_name))
                        {
                            user.first_name = claim.Value;
                        }
                        if (claim.Type == nameof(user.middle_name))
                        {
                            user.middle_name = claim.Value;
                        }
                        if (claim.Type == nameof(user.last_name))
                        {
                            user.last_name = claim.Value;
                        }
                        if (claim.Type == nameof(user.user_type))
                        {
                            user.user_type = claim.Value;
                        }
                        if (claim.Type == nameof(user.user_id))
                        {
                            user.id = user.user_id = claim.Value;
                        }
                    }
                    
                  
                    TenantModel tenant = null;
                    if (user.tenants.Count == 1)
                    {
                        tenant = user.tenants.First();
                    }
                    else if(user.tenants.Count != 0)
                    {
                        var orgkey = GetHeader(CommonConst.CommonValue.TENANT_KEY);
                        if (string.IsNullOrEmpty(orgkey))
                        {
                            orgkey = GetQueryString(CommonConst.CommonValue.TENANT_KEY);
                        }
                        tenant = user.tenants.FirstOrDefault(f => f.tenant_key == orgkey);
                    }
                    if (tenant != null && tenant.groups != null)
                    {
                        foreach (var g in tenant.groups)
                        {
                            user.roles.AddRange(g.roles);
                        }
                    }
                    user.roles = user.roles.Distinct().ToList();
                    user.claims = claims;
                    if (user.user_id == null)
                    {
                        var userid = user.claims.FirstOrDefault(f => f.Key == "sub");
                        if (userid != null)
                            user.id = user.user_id = userid.Value;
                    }
                    return user;
                }
                return null;
            }
        }

        public async Task<string> GetAccessTokenAync()
        {
            var accessToken = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                if (_httpContextAccessor.HttpContext != null
                   && _httpContextAccessor.HttpContext.User != null
                   && _httpContextAccessor.HttpContext.User.Identity != null
                   && _httpContextAccessor.HttpContext.User.Claims.Any())
                {
                    accessToken = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(f => f.Type == "access_token")?.Value;
                }

                if (string.IsNullOrEmpty(accessToken) && _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    accessToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].First().Replace("Bearer ", "");
                }
                else if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                }
            }
            return accessToken;
        }

        public string GetFormData(string key)
        {
            throw new NotImplementedException();
        }

        public string GetHeader(string key)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                return _httpContextAccessor.HttpContext.Request.Headers[key];
            }
            else
            {
                return null;
            }
        }

        public string GetRequestTenantId()
        {
            return GetReponseHeader(CommonConst.CommonField.TENANT_ID);
        }
        public string GetReponseHeader(string key)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                return _httpContextAccessor.HttpContext.Response.Headers[key];
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, string> GetHeaders()
        {
            var header = new Dictionary<string, string>();
            if (_httpContextAccessor.HttpContext != null)
            {
                foreach (var h in _httpContextAccessor.HttpContext.Request.Headers)
                {
                    header.Add(h.Key, h.Value);
                }
            }
            return header;
        }

        public string GetHttpMethod()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                return _httpContextAccessor.HttpContext.Request.Method;
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetMimeType(string fileName)
        {
            throw new NotImplementedException();
        }

        public string GetQueryString(string key)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                return _httpContextAccessor.HttpContext.Request.Query[key];
            }
            else
            {
                return null;
            }
        }
        public string GetQueryString()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                return _httpContextAccessor.HttpContext.Request.QueryString.Value;
            }
            else
            {
                return null;
            }
            }

        public string GetRequestBody()
        {
            if (_httpContextAccessor.HttpContext!=null && _httpContextAccessor.HttpContext.Request.Body != null)
            {
                var requestBody = new StreamReader(_httpContextAccessor.HttpContext.Request.Body).ReadToEndAsync().GetAwaiter().GetResult();
                return requestBody;
            }
            return null;
        }

        public T GetRequestBody<T>()
        {
            string body = GetRequestBody();
            try
            {
                if (!string.IsNullOrEmpty(body))
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(body);
                }
                else
                {
                    return default;
                }
            }
            catch (Exception ex)
            {
                //logger.Error(string.Format("Error while converting data [{0}], Error :  {1}", body, ex.Message), ex);
                return default;
            }
        }

        public string GetURIAbsolutePath()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                return _httpContextAccessor.HttpContext.Request.Path.ToString().ToLower();
            }
            else
            {
                return null;
            }
        }

        public void SetResponse(int statusCode, JObject data = null)
        {
            throw new NotImplementedException();
        }

        public void SetResponse(int statusCode, string data)
        {
            throw new NotImplementedException();
        }

        public void SetResponse(int statusCode, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void SetResponse(string data)
        {
            throw new NotImplementedException();
        }

        public void SetResponse(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void UnloadAppDomain()
        {
            throw new NotImplementedException();
        }
        
    }
}
