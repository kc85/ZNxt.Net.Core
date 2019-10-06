using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
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
        public AssemblyLoader(IDBService dbService,ILogger logger, IKeyValueStorage keyValueStorage)
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
                        _logger.Error(string.Format("No Assembly found :{0}", assemblyName), null);
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
                _logger.Info(string.Format("Loading Assemmbly:{0}, from Download ", assemblyName));

                var dataResponse = _dbProxy.Get(CommonConst.Collection.DLLS, new RawQuery(GetFilter(assemblyName)));

                if (dataResponse.Count > 0)
                {
                    var id = dataResponse[0][CommonConst.CommonField.DISPLAY_ID].ToString();
                    var assemblyData = _keyValueStorage.Get<string>(CommonConst.Collection.DLLS, id);
                    return System.Convert.FromBase64String(assemblyData);
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
    }
}
