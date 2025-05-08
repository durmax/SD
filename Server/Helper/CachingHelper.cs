using Microsoft.Extensions.Caching.Memory;
using System;

namespace sd.Api.Helper
{
    public class CachingHelper
    {
        private readonly IMemoryCache _cache;

        public CachingHelper(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T SetValue<T>(string key, T input)
        {
            if (!_cache.TryGetValue(key, out T result))
            {
                result = input;

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(key, result, cacheEntryOptions);
            }

            return result;
        }

        public T GetValue<T>(string key)
        {
            return _cache.TryGetValue(key, out T value) ? value : default(T);
        }
    }
}
