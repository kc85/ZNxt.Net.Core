using System;
using System.Reflection;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IAssemblyLoader
    {
        Type GetType(string assemblyName, string executeType);
        Assembly Load(string assemblyName);
    }
}
