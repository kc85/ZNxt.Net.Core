using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services
{
    public class Routing : IRouting
    {
        public IDBService _dbService;
        private ILogger _logger;
        private List<RoutingModel> _routes;
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IApiGatewayService _apiGatewayService;
        public Routing(IDBService dbService, ILogger logger, IAssemblyLoader assemblyLoader,IApiGatewayService apiGatewayService)
        {
            _dbService = dbService;
            _logger = logger;
            _assemblyLoader = assemblyLoader;
            _apiGatewayService = apiGatewayService;
            LoadRoutes();
        }

        private void LoadRoutes()
        {
            _routes = new List<RoutingModel>();
            List<Type> routeclasses = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {

                try
                {
                    var data = assembly.GetTypes()
                                        .Where(t => !t.IsAbstract)
                                         .Distinct()
                                         .ToList();

                    routeclasses.AddRange(data);

                }
                catch (ReflectionTypeLoadException ex)
                {
                    _logger.Error(ex.Message, ex);
                }
            }


            foreach (Type routeClass in routeclasses)
            {
                System.Reflection.MemberInfo[] info = routeClass.GetMethods();
                foreach (var mi in info)
                {
                    object[] routes = mi.GetCustomAttributes(typeof(Route), true);
                    if (routes.Length != 0)
                    {
                        var r = (Route)routes.First();
                        if (GetRoute(r.Method, r.RoutePath) == null)
                        {
                            _routes.Add(new RoutingModel()
                            {
                                Method = r.Method,
                                Route = r.RoutePath.ToLower(),
                                ExecultAssembly = routeClass.Assembly.FullName,
                                ExecuteMethod = mi.Name,
                                ExecuteType = routeClass.FullName,
                                ContentType = r.ContentType,
                                auth_users = r.AuthUsers,
                                module_name = routeClass.Assembly.GetName().Name,
                            });
                        }
                    }
                }
            }

            // from DB 
            try
            {
                if (!string.IsNullOrEmpty(ApplicationConfig.ConnectionString))
                {
                    var filter = "{" + CommonConst.CommonField.IS_OVERRIDE + " : " + CommonConst.CommonValue.FALSE + "}";
                    var dataResponse = _dbService.Get(CommonConst.Collection.SERVER_ROUTES, new RawQuery(filter));
                    foreach (var routeData in dataResponse)
                    {
                        var route = Newtonsoft.Json.JsonConvert.DeserializeObject<RoutingModel>(routeData.ToString());
                        var dbroute = GetRoute(route.Method, route.Route);
                        if (dbroute == null)
                        {
                            _routes.Remove(dbroute);
                        }
                        _routes.Add(route);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while loading route from db {ex.Message}", ex);
            }
        }
        public void ReLoadRoutes()
        {
            LoadRoutes();
        }
        public RoutingModel GetRoute(string method, string url)
        {
            var route =  _routes.FirstOrDefault(f => f.Method == method && f.Route == url);
            
            return route;
        }

        public List<RoutingModel> GetRoutes()
        {
            return _routes;
        }

        public void PushAssemblyRoute(Assembly assembly)
        {
            if (!string.IsNullOrEmpty(ApplicationConfig.ConnectionString))
            {
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fvi.FileVersion;
                string modulename = assembly.GetName().Name;

                _dbService.Delete(CommonConst.Collection.SERVER_ROUTES, new RawQuery(new JObject() { [CommonConst.CommonField.MODULE_NAME] = modulename }.ToString()));
                if (assembly.GetName().Name != "ZNxt.Net.Core.Module.Gateway" && !string.IsNullOrEmpty(ApplicationConfig.ApiGatewayEndpoint))
                {
                    _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/gateway/installroute", "", new JObject()
                    {
                        [CommonConst.CommonField.NAME] = modulename
                    }, null, ApplicationConfig.ApiGatewayEndpoint).GetAwaiter().GetResult();
                }
                foreach (var route in _assemblyLoader.GetRoulesFromAssembly(assembly))
                {
                    try
                    {
                        var data = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(route));
                        data[CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
                        data[CommonConst.CommonField.MODULE_NAME] = modulename;
                        data[CommonConst.CommonField.VERSION] = version;
                        data[CommonConst.CommonField.ÌS_OVERRIDE] = false;
                        data[CommonConst.CommonField.OVERRIDE_BY] = CommonConst.CommonValue.NONE;
                        data[CommonConst.CommonField.KEY] = $"{route.Method}:{route.Route}";
                        WriteToDB(data, CommonConst.Collection.SERVER_ROUTES);
                        data[CommonConst.CommonField.MODULE_ENDPOINT] = ApplicationConfig.AppEndpoint;
                        if (assembly.GetName().Name != "ZNxt.Net.Core.Module.Gateway" && !string.IsNullOrEmpty(ApplicationConfig.ApiGatewayEndpoint))
                        {
                            _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/gateway/installroute", "", data, null, ApplicationConfig.ApiGatewayEndpoint).GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error InstallRoutes route:{route}", ex);
                    }
                }
                ReLoadRoutes();
            }
        }
        private void WriteToDB(JObject joData, string collection)
        {
            
            if (!_dbService.Write(collection, joData))
            {
                _logger.Error(string.Format("Error while uploading file data {0}", joData.ToString()), null);
            }
        }

    }
}
