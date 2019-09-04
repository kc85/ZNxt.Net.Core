using System;
using System.Collections.Generic;
using ZNxt.Net.Core.Consts;

namespace ZNxt.Net.Core.Model
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class Route : System.Attribute
    {
        public string Method { get; set; }
        public string RoutePath { get; set; }
        public string ContentType { get; set; }
        public List<string> AuthUsers { get; set; } 

        public Route(string routePath, string method = CommonConst.ActionMethods.GET, string authUsers = CommonConst.CommonValue.ACCESS_NONE, string contentType = CommonConst.CONTENT_TYPE_APPLICATION_JSON)
        {
            this.Method = method;
            this.RoutePath = routePath;
            this.ContentType = contentType;
            this.AuthUsers = new List<string>();
            this.AuthUsers.AddRange(authUsers.Split(','));
        }
    }
}
