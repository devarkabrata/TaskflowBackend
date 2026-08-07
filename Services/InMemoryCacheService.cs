using System.Collections.Concurrent;
using System.Text.Json;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    // Temporary in-process stand-in for RedisCacheService while Redis is not wired up.
    // State is not shared across instances and is lost on restart.
    public class InMemoryCacheService : IRedisCacheService
    {
        private class Entry
        {
            public string Json { get; set; } = "";
            public DateTime ExpiresAt { get; set; }
        }

        private readonly ConcurrentDictionary<string, Entry> _store = new();

        public Task SetAsync<T>(string key, T value, TimeSpan expiry)
        {
            var entry = new Entry
            {
                Json = JsonSerializer.Serialize(value),
                ExpiresAt = DateTime.UtcNow.Add(expiry)
            };
            _store[key] = entry;
            return Task.CompletedTask;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            if (_store.TryGetValue(key, out var entry))
            {
                if (entry.ExpiresAt > DateTime.UtcNow)
                {
                    return Task.FromResult(JsonSerializer.Deserialize<T>(entry.Json));
                }

                _store.TryRemove(key, out _);
            }

            return Task.FromResult<T?>(default);
        }

        public Task DeleteAsync(string key)
        {
            _store.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            if (_store.TryGetValue(key, out var entry))
            {
                if (entry.ExpiresAt > DateTime.UtcNow)
                {
                    return Task.FromResult(true);
                }

                _store.TryRemove(key, out _);
            }

            return Task.FromResult(false);
        }
    }
}
