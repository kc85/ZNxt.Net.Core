using Newtonsoft.Json.Linq;
using System;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using System.Linq;

namespace ZNxt.Net.Core.Module.TemplateEngine.Services.Api
{
    public class TemplateEngineController : Core.Services.ApiBaseService
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IDBService _dbService;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        private const string _collection= "template";


        public TemplateEngineController(IResponseBuilder responseBuilder, IDBService dbService, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler)
            : base(httpContextProxy, dBService, logger, responseBuilder)
        {
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
        }
        [Route("/template/add", CommonConst.ActionMethods.POST, CommonConst.CommonValue.SYS_ADMIN)]
        public JObject Add()
        {
            return _responseBuilder.Success();

        }
        [Route("/template/update", CommonConst.ActionMethods.POST, CommonConst.CommonValue.SYS_ADMIN)]
        public JObject Update()
        {
            return _responseBuilder.Success();
        }
        [Route("/template/delete", CommonConst.ActionMethods.POST, CommonConst.CommonValue.SYS_ADMIN)]
        public JObject Delete()
        {
            return _responseBuilder.Success();
        }
        [Route("/template/process", CommonConst.ActionMethods.POST, CommonConst.CommonField.API_AUTH_TOKEN)]
        public JObject Process()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<JObject>();
                if (request != null && request["key"] != null)
                {

                    var data = _dbService.Get(_collection, new RawQuery("{'key':'" + request["key"].ToString() + "','override_by':'none'}"));
                    if (data.Count == 1 && data.First()["data"] != null)
                    {
                        var template = data.First()["data"].ToString();
                        var subject = data.First()["subject"] != null ?  data.First()["subject"].ToString() : "";

                        request.Remove("key");
                        foreach (var item in request)
                        {
                            subject = subject.Replace("{{" + item.Key.ToString() + "}}", item.Value.ToString());
                            template = template.Replace("{{" + item.Key.ToString() + "}}", item.Value.ToString());
                        }

                        JObject response = new JObject();
                        response["data"] = template;
                        response["subject"] = subject;
                        var apiresponse = _responseBuilder.Success(response);
                        _logger.Debug("templete process response", apiresponse);
                        return apiresponse;
                    }
                    else
                    {
                        _logger.Error($"key not found {request["key"]}");
                        return _responseBuilder.NotFound(); ;
                    }
                }
                else
                {
                    _logger.Error("Bad request");
                    return _responseBuilder.BadRequest(); ;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError() ;
            }
        }
        [Route("/template/get", CommonConst.ActionMethods.GET, CommonConst.CommonValue.SYS_ADMIN)]
        public JObject Get()
        {
            return GetPaggedData(_collection,null, "{'override_by':'none'}");
        }
    }
}
