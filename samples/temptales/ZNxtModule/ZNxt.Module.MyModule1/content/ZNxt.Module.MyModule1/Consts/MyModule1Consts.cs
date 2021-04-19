using System;
using System.Collections.Generic;
using System.Text;

namespace ZNxt.Module.MyModule1.Consts
{
    public static class MyModule1Consts
    {
        public const string SERVICE_NAME = "ZNxt.Module.MyModule1";
        public const string SERVICE_API_PREFIX = "/mdl1";
        public static string GetServiceInfo()
        {
            return $"Name: {SERVICE_NAME}, Api Prefix: {SERVICE_API_PREFIX}";
        }
    }
}
