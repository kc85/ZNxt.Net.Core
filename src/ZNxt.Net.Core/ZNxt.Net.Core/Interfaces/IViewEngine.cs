using System.Collections.Generic;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IViewEngine
    {
        string Compile(string inputTemplete, string key, Dictionary<string, dynamic> dataModel);
    }
}