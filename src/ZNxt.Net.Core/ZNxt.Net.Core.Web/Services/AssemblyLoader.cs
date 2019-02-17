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

        public AssemblyLoader(IDBService dbService,ILogger logger)
        {
            _dbProxy = dbService;
            _logger = logger;
        }
        
        public Type GetType(string assemblyName, string executeType)
        {
            _logger.Info(string.Format("GetType: {0}, executeType: {1}", assemblyName, executeType));
            var assembly = Load(assemblyName.Trim());
            return assembly.GetType(executeType.Trim());
        }

        public Assembly Load(string assemblyName)
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

        private byte[] GetAsssemblyFromDB(string assemblyName)
        {
            _logger.Info(string.Format("Loading Assemmbly:{0}, from Download ", assemblyName));

            var dataResponse = _dbProxy.Get(CommonConst.Collection.DLLS, new RawQuery( GetFilter(assemblyName)));

            if (dataResponse.Count > 0)
            {
                var assemblyData = dataResponse[0][CommonConst.CommonField.DATA].ToString();
                return System.Convert.FromBase64String(assemblyData);
            }
            return null;
        }

        private static string GetFilter(string path)
        {
            return "{ $and: [ { is_override:{ $ne: true}  }, {'" + CommonConst.CommonField.FILE_PATH + "':  {$regex :'^" + path.ToLower() + "$','$options' : 'i'}}] }";
        }

        private Assembly GetFromAppDomain(string fullName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if ((assembly.ManifestModule).ScopeName == fullName)
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}
