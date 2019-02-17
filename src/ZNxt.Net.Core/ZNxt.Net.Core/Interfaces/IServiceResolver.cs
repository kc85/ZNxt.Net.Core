using System;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IServiceResolver
    {
        T Resolve<T>();
        object Resolve(Type type);
    }
}
