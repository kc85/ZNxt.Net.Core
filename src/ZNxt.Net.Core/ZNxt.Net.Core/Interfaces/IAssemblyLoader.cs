using System;
using System.Collections.Generic;
using System.Reflection;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IAssemblyLoader
    {
        Type GetType(string assemblyName, string executeType);
        Assembly Load(string assemblyName);
        List<RoutingModel> GetRoulesFromAssembly(Assembly assembly);

    }
}
