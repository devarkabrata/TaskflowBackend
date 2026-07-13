namespace TaskFlowBackend.Services.Interfaces
{
    public interface IAvatarStorageService
    {
        Task<(string Url, string StoragePath)> UploadAsync(IFormFile file, Guid userId);
        Task DeleteAsync(string storagePath);
    }
}
