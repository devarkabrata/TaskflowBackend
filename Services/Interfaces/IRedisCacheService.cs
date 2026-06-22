namespace TaskFlowBackend.Services.Interfaces
{
    public interface IRedisCacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan expiry);
        Task<T?> GetAsync<T>(string key);
        Task DeleteAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}