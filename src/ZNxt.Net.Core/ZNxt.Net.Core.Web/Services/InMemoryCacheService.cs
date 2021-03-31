using Microsoft.Extensions.Caching.Memory;
using System;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Services
{
    public class InMemoryCacheService : IInMemoryCacheService
    {
        private static object _lockcacheitem = new object();
        private static IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        public InMemoryCacheService(IMemoryCache memoryCache)
        {
            if (_memoryCache == null)
            {
                _memoryCache = memoryCache;
            }
        }
        public T Get<T>(string key)
        {
            object value = null;
            if (_memoryCache.TryGetValue(GetKey(key, typeof(T)), out value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else
            {
                return default(T);
            }
        }

        public void Put<T>(string key, T data, int duration = 5)
        {
         
            Put(key, data, TimeSpan.FromMinutes(duration));
        }
        public void Put<T>(string key, T data, TimeSpan timeSpan, Action<object, object, EvictionReason, object> callback = null)
        {
            lock (_lockcacheitem)
            {
                var options = new MemoryCacheEntryOptions()
                  .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddSeconds(timeSpan.TotalSeconds)).RegisterPostEvictionCallback(
                (echoKey, removevalue, reason, substate) =>
                {
                    callback?.Invoke(echoKey, removevalue, reason, substate);
                });

                _memoryCache.Set<T>(
                GetKey(key, typeof(T)),
                data,
                options
                );
            }
        }
        private string GetKey(string key, Type obj)
        {
            return $"{key}:::{obj.FullName}";
        }

    }
}
