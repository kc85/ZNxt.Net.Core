using System;
using System.Collections.Generic;
using System.Text;

namespace MyModule.EComm.Consts
{
    public static class ECommConsts
    {
        public const string SERVICE_NAME = "MyModule.EComm";
        public const string SERVICE_API_PREFIX = "/ecom";
        public static string GetServiceInfo()
        {
            return $"Name: {SERVICE_NAME}, Api Prefix: {SERVICE_API_PREFIX}";
        }
    }
}
