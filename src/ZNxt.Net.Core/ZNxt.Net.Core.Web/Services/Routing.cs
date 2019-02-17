using System;
using System.Collections.Generic;
using System.Linq;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services
{
    public class Routing : IRouting
    {
        public IDBService _dbService;
        private List<RoutingModel> _routes;
        public Routing(IDBService dbService)
        {
            _dbService = dbService;
            LoadRoutes();
        }

        private void LoadRoutes()
        {
            _routes = new List<RoutingModel>();
            List<Type> routeclasses = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                routeclasses.AddRange(
                        assembly.GetTypes()
                                    .Where(t =>!t.IsAbstract)
                                     .Distinct()
                                     .ToList());
            }


            foreach (Type routeClass in routeclasses)
            {
                System.Reflection.MemberInfo [] info = routeClass.GetMethods();
                foreach (var mi in info)
                {
                    object[] routes = mi.GetCustomAttributes(typeof(Route), true);
                    if (routes.Length != 0)
                    {
                        var r = (Route)routes.First();
                        _routes.Add(new RoutingModel() {
                            Method = r.Method,
                            Route = r.RoutePath.ToLower(),
                            ExecultAssembly = routeClass.Assembly.ManifestModule.Name,
                            ExecuteMethod = mi.Name,
                            ExecuteType = routeClass.FullName,
                            ContentType = r.ContentType,
                            auth_users = r.AuthUsers
                        });
                    }
                }
            }
        }
        public RoutingModel GetRoute(string method, string url)
        {
            return _routes.FirstOrDefault(f => f.Method == method && f.Route == url);
        }

        public List<RoutingModel> GetRoutes()
        {
            return _routes;
        }
    }
}
