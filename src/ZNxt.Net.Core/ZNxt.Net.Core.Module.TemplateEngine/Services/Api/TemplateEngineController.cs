﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Helpers;
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
        [Route("/template/add", CommonConst.ActionMethods.POST, "sys_admin")]
        public JObject Add()
        {
            return _responseBuilder.Success();

        }
        [Route("/template/update", CommonConst.ActionMethods.POST, "sys_admin")]
        public JObject Update()
        {
            return _responseBuilder.Success();
        }
        [Route("/template/delete", CommonConst.ActionMethods.POST, "sys_admin")]
        public JObject Delete()
        {
            return _responseBuilder.Success();
        }
        [Route("/template/process", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL, "text/html")]
        public string Process()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<JObject>();
                if (request != null && request["key"] != null)
                {

                    var data = _dbService.Get(_collection, new RawQuery("{'key':'" + request["key"].ToString() + "','override_by':'none'}"));
                    if (data.Count == 1 && data.First()["data"] != null)
                    {
                        var templete = data.First()["data"].ToString();
                        request.Remove("key");
                        foreach (var item in request)
                        {
                            templete = templete.Replace("{{" + item.Key.ToString() + "}}", item.Value.ToString());
                        }
                        return templete;
                    }
                    else
                    {
                        _logger.Error($"key not found {request["key"]}");
                        return null;
                    }
                }
                else
                {
                    _logger.Error("Bad request");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return null;

            }
        }
        [Route("/template/get", CommonConst.ActionMethods.GET, "sys_admin")]
        public JObject Get()
        {
            return GetPaggedData(_collection,null, "{'override_by':'none'}");
        }
    }
}