using Microsoft.Extensions.Caching.Memory;
using ServiceContracts;
using System.Text.RegularExpressions;

namespace Services
{
    /// <summary>
    /// Implements in-memory caching service using IMemoryCache
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly HashSet<string> _cacheKeys;
        private readonly object _lockObject = new object();

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _cacheKeys = new HashSet<string>();
        }

        /// <summary>
        /// Gets a value from cache by key
        /// </summary>
        public bool TryGetValue<T>(string key, out T? value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = default;
                return false;
            }

            return _memoryCache.TryGetValue(key, out value);
        }

        /// <summary>
        /// Sets a value in cache with optional absolute expiration
        /// </summary>
        public void Set<T>(string key, T value, int? absoluteExpirationMinutes = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            lock (_lockObject)
            {
                var cacheOptions = new MemoryCacheEntryOptions();

                if (absoluteExpirationMinutes.HasValue && absoluteExpirationMinutes > 0)
                {
                    cacheOptions.AbsoluteExpirationRelativeToNow = 
                        TimeSpan.FromMinutes(absoluteExpirationMinutes.Value);
                }

                _memoryCache.Set(key, value, cacheOptions);
                _cacheKeys.Add(key);
            }
        }

        /// <summary>
        /// Removes a specific key from cache
        /// </summary>
        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            lock (_lockObject)
            {
                _memoryCache.Remove(key);
                _cacheKeys.Remove(key);
            }
        }

        /// <summary>
        /// Removes all cache entries matching a pattern
        /// Supports wildcard matching with * character
        /// Example: "countries_*" removes all keys starting with "countries_"
        /// </summary>
        public void RemoveByPattern(string keyPattern)
        {
            if (string.IsNullOrWhiteSpace(keyPattern))
                return;

            lock (_lockObject)
            {
                // Convert wildcard pattern to regex
                string regexPattern = "^" + Regex.Escape(keyPattern).Replace("\\*", ".*") + "$";
                var regex = new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                var keysToRemove = _cacheKeys.Where(k => regex.IsMatch(k)).ToList();

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.Remove(key);
                }
            }
        }
    }
}
