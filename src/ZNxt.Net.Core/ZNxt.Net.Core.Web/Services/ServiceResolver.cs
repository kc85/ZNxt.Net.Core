using System;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Services
{
    public class ServiceResolver : IServiceResolver
    {
        private IServiceProvider _serviceProvider;
        public ServiceResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public T Resolve<T>()
        {
            return (T)_serviceProvider.GetService(typeof(T));             
        }
        public object Resolve(Type type)
        {
            return _serviceProvider.GetService(type);
        }
    }
}
