namespace ServiceContracts
{
    /// <summary>
    /// Represents a caching service for managing in-memory cache operations
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a value from cache by key
        /// </summary>
        /// <typeparam name="T">Type of the cached value</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">The cached value if found</param>
        /// <returns>True if the key exists in cache; otherwise false</returns>
        bool TryGetValue<T>(string key, out T? value);

        /// <summary>
        /// Sets a value in cache with optional expiration
        /// </summary>
        /// <typeparam name="T">Type of the value to cache</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="absoluteExpirationMinutes">Optional absolute expiration time in minutes</param>
        void Set<T>(string key, T value, int? absoluteExpirationMinutes = null);

        /// <summary>
        /// Removes a value from cache
        /// </summary>
        /// <param name="key">Cache key to remove</param>
        void Remove(string key);

        /// <summary>
        /// Clears all cache entries matching a pattern
        /// </summary>
        /// <param name="keyPattern">Pattern to match keys (supports wildcard *)</param>
        void RemoveByPattern(string keyPattern);
    }
}
