﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.ContentHandler
{
    public static class ServerPageModelHelper
    {
        public static string ServerSidePageHandler(string requestUriPath, IDBService dbProxy, IHttpContextProxy httpProxy, IViewEngine viewEngine, IActionExecuter actionExecuter, ILogger logger, ISessionProvider sessionProvider,IKeyValueStorage keyValueStorage, Dictionary<string, dynamic> pageModel = null)
        {
            var fi = new FileInfo(requestUriPath);
            var data = ContentHelper.GetStringContent(dbProxy, logger, requestUriPath, keyValueStorage);
            if (data != null)
            {
                if (pageModel == null)
                {
                    pageModel = new Dictionary<string, dynamic>();
                }
                var folderPath = requestUriPath.Replace(fi.Name, "");

                UpdateBaseModel(pageModel, requestUriPath, fi.Name);

                data = viewEngine.Compile(data, requestUriPath, SetDefaultModel(dbProxy, httpProxy, logger, viewEngine, actionExecuter, pageModel, keyValueStorage, sessionProvider,folderPath));
                if (pageModel.ContainsKey(CommonConst.CommonValue.PAGE_TEMPLATE_PATH))
                {
                    FileInfo fiTemplate = new FileInfo(pageModel[CommonConst.CommonValue.PAGE_TEMPLATE_PATH]);
                    var templateFileData = ContentHelper.GetStringContent(dbProxy, logger, pageModel[CommonConst.CommonValue.PAGE_TEMPLATE_PATH], keyValueStorage);
                    pageModel[CommonConst.CommonValue.RENDERBODY_DATA] = data;
                    data = viewEngine.Compile(templateFileData, pageModel[CommonConst.CommonValue.PAGE_TEMPLATE_PATH],
                        ServerPageModelHelper.SetDefaultModel(dbProxy, httpProxy, logger, viewEngine, actionExecuter, pageModel,keyValueStorage, sessionProvider,pageModel[CommonConst.CommonValue.PAGE_TEMPLATE_PATH].Replace(fiTemplate.Name, "")));
                }
                return data;
            }
            else
            {
                return null;
            }
        }

        private static void UpdateBaseModel(Dictionary<string, dynamic> pageModel, string requestUriPath, string pageName)
        {
            var uri = ContentHelper.UnmappedUriPath(requestUriPath);
            if (ContentHelper.IsAdminPage(requestUriPath))
            {
                pageModel[CommonConst.CommonField.BASE_URI] = string.Format("{0}{1}", ApplicationConfig.AppPath, ApplicationConfig.AppBackendPath);
            }
            else
            {
                pageModel[CommonConst.CommonField.BASE_URI] = ApplicationConfig.AppPath;
            }
            pageModel[CommonConst.CommonField.PAGE_NAME] = pageName;
            pageModel[CommonConst.CommonField.URI] = uri;
            AddBaseData(pageModel);
        }

        private static Dictionary<string, dynamic> SetDefaultModel(IDBService dbProxy, IHttpContextProxy httpProxy, ILogger logger, IViewEngine viewEngine, IActionExecuter actionExecuter, Dictionary<string, dynamic> model,IKeyValueStorage keyValueStorage, ISessionProvider sessionProvider,string folderPath = null )
        {
           // ISessionProvider sessionProvider = new SessionProvider(httpProxy, dbProxy, logger);

            if (model == null)
            {
                model = new Dictionary<string, dynamic>();
            }
            model[CommonConst.CommonValue.METHODS] = new Dictionary<string, dynamic>();

            Func<string, string, JArray> getData =
                (string collection, string filter) =>
                {
                    return dbProxy.Get(collection,  new RawQuery(filter));
                };
            Func<string, string> getAppSetting =
               (string key) =>
               {
                   //var response = AppSettingService.Instance.GetAppSettingData(key);
                   //if (string.IsNullOrEmpty(response))
                   //{
                   //    response = ConfigurationManager.AppSettings[key];
                   //}
                   //return response;
                   return string.Empty;
               };
            Func<string, JObject> getSessionValue =
               (string key) =>
               {
                   return sessionProvider.GetValue<JObject>(key);
               };
            Func<string, string> includeTemplate = (string templatePath) =>
            {
                FileInfo fi = new FileInfo(string.Format("c:\\{0}{1}", folderPath, templatePath));
                string path = fi.FullName.Replace("c:", "");
                model[CommonConst.CommonValue.PAGE_TEMPLATE_PATH] = path;
                return string.Empty;
            };
            Func<string, bool> authorized = (string authGroups) =>
            {
                var sessionUser = sessionProvider.GetValue<UserModel>(CommonConst.CommonValue.SESSION_USER_KEY);
                if (sessionUser == null)
                {
                    return false;
                }

                if (!authGroups.Split(',').Where(i => sessionUser.claims.Where(f=>f.Value == i).Any()).Any())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            };
            Func<string, JObject, JObject> ActionExecute =
               (string actionPath, JObject data) =>
               {
                   //var param = ActionExecuterHelper.CreateParamContainer(null, httpProxy, logger, actionExecuter);

                   //if (data != null)
                   //{
                   //    foreach (var item in data)
                   //    {
                   //        Func<dynamic> funcValue = () => { return item.Value; };
                   //        param.AddKey(item.Key, funcValue);
                   //    }
                   //}
                   //return actionExecuter.Exec<JObject>(actionPath, dbProxy, param);
                   return null;
               };

            Func<string, JObject, Dictionary<string, dynamic>> IncludeModel =
               (string includeModelPath, JObject data) =>
               {
                   try
                   {
                       //var param = ActionExecuterHelper.CreateParamContainer(null, httpProxy, logger, actionExecuter);

                       //Dictionary<string, dynamic> modelData = new Dictionary<string, dynamic>();

                       //if (data != null)
                       //{
                       //    foreach (var item in data)
                       //    {
                       //        Func<dynamic> funcValue = () => { return item.Value; };
                       //        param.AddKey(item.Key, funcValue);
                       //    }
                       //}

                       //object response = actionExecuter.Exec(includeModelPath, dbProxy, param);
                       //if (response is Dictionary<string, dynamic>)
                       //{
                       //    return response as Dictionary<string, dynamic>;
                       //}
                       //else
                       //{
                       //    throw new InvalidCastException(string.Format("Invalid respone from {0}", includeModelPath));
                       //}
                       return null;
                   }
                   catch (UnauthorizedAccessException ex)
                   {
                       logger.Error(string.Format("Error While executing Route : {0}, Error : {1}", includeModelPath, ex.Message), ex);
                       throw;
                   }
               };
            model[CommonConst.CommonValue.METHODS]["IncludeModel"] = IncludeModel;

            model[CommonConst.CommonValue.METHODS]["ExecuteAction"] = ActionExecute;

            model[CommonConst.CommonValue.METHODS]["InclueTemplate"] = includeTemplate;

            model[CommonConst.CommonValue.METHODS]["GetData"] = getData;

            Func<JObject> requestBody = () => httpProxy.GetRequestBody<JObject>();
            model[CommonConst.CommonValue.METHODS]["RequestBody"] = requestBody;

            Func<string, string> queryString = (string key) => httpProxy.GetQueryString(key);
            model[CommonConst.CommonValue.METHODS]["QueryString"] = queryString;

            model[CommonConst.CommonValue.METHODS]["AppSetting"] = getAppSetting;

            model[CommonConst.CommonValue.METHODS]["GetSessionData"] = getSessionValue;

            model[CommonConst.CommonValue.METHODS]["Authorized"] = authorized;

            Func<string, JObject, string> includeBlock =
                (string blockPath, JObject blockModel) =>
                {
                    var inputBlockModel = new Dictionary<string, dynamic>();
                    if (blockModel != null)
                    {
                        foreach (var item in blockModel)
                        {
                            inputBlockModel[item.Key] = item.Value;
                        }
                    }
                    if (model != null)
                    {
                        foreach (var item in model)
                        {
                            inputBlockModel[item.Key] = item.Value;
                        }
                    }
                    FileInfo fi = new FileInfo(string.Format("c:\\{0}{1}", folderPath, blockPath));
                    string path = fi.FullName.Replace("c:", "");
                    var data = ContentHelper.GetStringContent(dbProxy, logger, path, keyValueStorage);
                    data = viewEngine.Compile(data, path, SetDefaultModel(dbProxy, httpProxy, logger, viewEngine, actionExecuter, inputBlockModel, keyValueStorage, sessionProvider, path.Replace(fi.Name, "")));
                    return data;
                };
            model[CommonConst.CommonValue.METHODS]["Include"] = includeBlock;

            Func<string> randerBody = () =>
            {
                if (model.ContainsKey(CommonConst.CommonValue.RENDERBODY_DATA))
                {
                    return model[CommonConst.CommonValue.RENDERBODY_DATA];
                }
                else
                {
                    return string.Empty;
                }
            };

            model[CommonConst.CommonValue.METHODS]["RenderBody"] = randerBody;

            return model;
        }

        public static void AddBaseData(Dictionary<string, dynamic> baseData)
        {
            baseData[CommonConst.CommonField.APP_NAME] = ApplicationConfig.AppName;
        }
    }
}
