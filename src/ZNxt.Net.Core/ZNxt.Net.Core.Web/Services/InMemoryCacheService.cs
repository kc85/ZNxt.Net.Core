using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Services
{
    public class InMemoryCacheService : IInMemoryCacheService
    {
        private readonly IMemoryCache _memoryCache;
        public InMemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public T Get<T>(string key)
        {
            object value = null;
            if (_memoryCache.TryGetValue(key, out value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else{
                return default(T);
            }   
        }

        public void Put<T>(string key, T data, int duration = 5)
        {
            object value = null;
            if (!_memoryCache.TryGetValue(key, out value))
            {
                // Key not in cache, so get data.
                value = data;

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromMinutes(duration));

                // Save data in cache.
                _memoryCache.Set(key, data, cacheEntryOptions);
            }
        }
    }
}
