using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private IDBService _dbProxy;
        private ILogger _logger;
        public Dictionary<string, byte[]> _loadedAssembly = new Dictionary<string, byte[]>();
        private readonly IKeyValueStorage _keyValueStorage;
        public AssemblyLoader(IDBService dbService, ILogger logger, IKeyValueStorage keyValueStorage)
        {
            _dbProxy = dbService;
            _logger = logger;
            _keyValueStorage = keyValueStorage;
        }
        
        public Type GetType(string assemblyName, string executeType)
        {
            _logger.Info(string.Format("GetType: {0}, executeType: {1}", assemblyName, executeType));
            var assembly = Load(assemblyName.Trim());
            if (assembly != null)
            {
                return assembly.GetType(executeType.Trim());
            }
            else
            {
                return null;
            }
        }

        public Assembly Load(string assemblyName)
        {
            try
            {
                var assembly = GetFromAppDomain(assemblyName);
                if (assembly == null)
                {
                    string localPath = String.Format("{0}{1}", ApplicationConfig.AppBinPath, assemblyName);

                    Byte[] assemblyBytes = null;
                    if (_loadedAssembly.ContainsKey(assemblyName))
                    {
                        assemblyBytes = _loadedAssembly[assemblyName];
                    }
                    else if (File.Exists(localPath))
                    {
                        assemblyBytes = File.ReadAllBytes(localPath);
                        _loadedAssembly[assemblyName] = assemblyBytes;
                    }
                    else
                    {
                        assemblyBytes = GetAsssemblyFromDB(assemblyName);
                        if (assemblyBytes != null)
                        {
                            _loadedAssembly[assemblyName] = assemblyBytes;
                        }
                    }
                    if (assemblyBytes == null)
                    {
                        _logger.Error($"No Assembly found :{assemblyName}", null);
                    }
                    else
                    {
                        assembly = Assembly.Load(assemblyBytes);
                    }
                }
                return assembly;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error While loading Assembly {assemblyName}. {ex.Message}", ex);
                throw;
            }
           
        }

        private byte[] GetAsssemblyFromDB(string assemblyName)
        {
            try
            {
                if (string.IsNullOrEmpty(ApplicationConfig.ConnectionString))
                {
                    return null;
                }
               _logger.Info(string.Format("Loading Assemmbly:{0}, from Download ", assemblyName));

                var dataResponse = _dbProxy.Get(CommonConst.Collection.DLLS, new RawQuery(GetFilter(assemblyName)));

                if (dataResponse.Count > 0)
                {
                    var id = dataResponse[0][CommonConst.CommonField.DISPLAY_ID].ToString();
                    var assemblyData = _keyValueStorage.Get<string>(CommonConst.Collection.DLLS, id);
                    return Convert.FromBase64String(assemblyData);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error on GetAsssemblyFromDB {assemblyName}, Error: {ex.Message}", ex);
            }

            return null;
        }

        private static string GetFilter(string name)
        {
            return "{ $and: [ { is_override:{ $ne: true}  }, {'" + CommonConst.CommonField.NAME + "':  {$regex :'^" + name + "$','$options' : 'i'}}] }";
        }

        private Assembly GetFromAppDomain(string fullName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName == fullName)
                {
                    return assembly;
                }
            }
            return null;
        }

        public List<RoutingModel> GetRoulesFromAssembly(Assembly assembly)
        {
            var routes = new List<RoutingModel>();
            List<Type> routeclasses = new List<Type>();

            routeclasses.AddRange(
                    assembly.GetTypes()
                                .Where(t => !t.IsAbstract)
                                 .Distinct()
                                 .ToList());

            foreach (Type routeClass in routeclasses)
            {
                System.Reflection.MemberInfo[] info = routeClass.GetMethods();
                foreach (var mi in info)
                {
                    object[] objroutes = mi.GetCustomAttributes(typeof(Route), true);
                    if (objroutes.Length != 0)
                    {
                        var r = (Route)objroutes.First();
                        routes.Add(new RoutingModel()
                        {
                            Method = r.Method,
                            Route = r.RoutePath.ToLower(),
                            ExecultAssembly = assembly.FullName,
                            ExecuteMethod = mi.Name,
                            ExecuteType = routeClass.FullName,
                            ContentType = r.ContentType,
                            auth_users = r.AuthUsers
                        });
                        //  userRoles.AddRange(r.AuthUsers);
                    }
                }
            }
            return routes;
        }
    }
}
