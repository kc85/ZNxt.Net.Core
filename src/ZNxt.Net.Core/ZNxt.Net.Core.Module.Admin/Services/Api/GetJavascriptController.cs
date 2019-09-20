using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Helpers;
using System.Linq;

namespace ZNxt.Net.Core.Module.Admin.Services.Api
{
   
    public class GetJavascriptController
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly ILogger _logger;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly IDBService _dBService;
        private readonly IKeyValueStorage _keyValueStorage;
        private readonly IStaticContentHandler _staticContentHandler;
        public GetJavascriptController(IResponseBuilder responseBuilder,ILogger logger, IHttpContextProxy httpContextProxy,IDBService dBService,IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler)
        {
            _httpContextProxy = httpContextProxy;
            _responseBuilder = responseBuilder;
            _logger = logger;
            _dBService = dBService;
            _keyValueStorage = keyValueStorage;
            _staticContentHandler = staticContentHandler;
        }
        [Route("/admin/js", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL, "application/javascript")]
        public string GetJS()
        {

            try
            {
                _logger.Debug("Calling Get JS ");
                var path = _httpContextProxy.GetQueryString("path");
                if (string.IsNullOrEmpty(path))
                {
                    _logger.Error("Path is missing in the query string");
                    return null;
                }
                var filterQuery = "{" + CommonConst.CommonField.FILE_PATH + ":/.js$/i}";
                var data = _dBService.Get(CommonConst.Collection.STATIC_CONTECT, new RawQuery(filterQuery), new List<string> { CommonConst.CommonField.FILE_PATH, CommonConst.CommonField.DISPLAY_ID });
                _logger.Debug("Fetch value from Get JS");

                var listOfArrays = new StringBuilder();
                var queryRecords = data.Select(l => new
                {
                    length = l[CommonConst.CommonField.FILE_PATH].ToString().Length,
                    file_path = l[CommonConst.CommonField.FILE_PATH].ToString(),
                    id = l[CommonConst.CommonField.DISPLAY_ID].ToString()
                }).OrderBy(o => o.length).ToList();

                _logger.Debug("Apply by Order by Get JS");
                foreach (var item in queryRecords)
                {
                    if (!string.IsNullOrEmpty(item.file_path) && item.file_path.IndexOf(path) == 0)
                    {
                        string jspath = item.file_path;
                        listOfArrays.AppendLine(string.Empty);
                        listOfArrays.AppendLine(string.Format("/***** File: {0}   ******/", item.file_path));
                        var content = _staticContentHandler.GetStringContentAsync(item.file_path).GetAwaiter().GetResult();
                        if (!string.IsNullOrEmpty(content))
                        {
                            listOfArrays.AppendLine(string.Empty);
                            listOfArrays.Append($"/* */{content.Remove(0,1)}/* */");
                        }
                    }
                }

                return listOfArrays.ToString();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetJs {0}", ex.Message), ex);
                return $"/****Error {ex.Message} , {ex.StackTrace }****/";
            }
        }
        [Route("/admin/menu", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL, "application/javascript")]
        public string GetMenu()
        {

            try
            {
                _logger.Debug("Calling Get Menu");
                var response = new StringBuilder();
                var data = _dBService.Get("ui_routes", new RawQuery(CommonConst.Filters.IS_OVERRIDE_FILTER));
                response.AppendLine($"var __menus = {data.ToString()};");
                return response.ToString();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetJs {0}", ex.Message), ex);
                return $"/****Error {ex.Message} , {ex.StackTrace }****/";
            }
        }
        [Route("/admin/user", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL, "application/javascript")]
        public string GetUser()
        {

            try
            {
                _logger.Debug("Calling Get User");
                var response = new StringBuilder();
                var user = _httpContextProxy.User;
                if(user== null)
                {
                    user = new UserModel();
                }
                response.AppendLine($"var __userData = {Newtonsoft.Json.JsonConvert.SerializeObject(user)};");
                return response.ToString();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetJs {0}", ex.Message), ex);
                return $"/****Error {ex.Message} , {ex.StackTrace }****/";
            }
        }
    }
}
