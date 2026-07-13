using System.Net;
using System.Net.Http.Headers;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class AvatarStorageService : IAvatarStorageService
    {
        private const long MaxFileSizeBytes = 1 * 1024 * 1024; // 1 MB

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/png", "image/jpeg", "image/jpg", "image/webp", "image/gif"
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AvatarStorageService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<(string Url, string StoragePath)> UploadAsync(IFormFile file, Guid userId)
        {
            if (file == null || file.Length == 0)
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "file", Code = "FILE_REQUIRED", Message = "An image file is required." }
                });

            if (file.Length > MaxFileSizeBytes)
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "file", Code = "FILE_TOO_LARGE", Message = "Image must be 1MB or smaller." }
                });

            if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "file", Code = "INVALID_FILE_TYPE", Message = "Only image files (png, jpeg, webp, gif) are allowed." }
                });

            var bucket = _config["SupabaseStorage:Bucket"];
            var extension = Path.GetExtension(file.FileName);
            var storagePath = $"avatars/{userId}/{Guid.NewGuid()}{extension}";

            var client = _httpClientFactory.CreateClient("SupabaseStorage");

            using var content = new StreamContent(file.OpenReadStream());
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            var response = await client.PostAsync($"/storage/v1/object/{bucket}/{storagePath}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "file", Code = "UPLOAD_FAILED", Message = $"Failed to upload image to storage: {error}" }
                });
            }

            var baseUrl = _config["SupabaseStorage:Url"]!.TrimEnd('/');
            var publicUrl = $"{baseUrl}/storage/v1/object/public/{bucket}/{storagePath}";

            return (publicUrl, storagePath);
        }

        public async Task DeleteAsync(string storagePath)
        {
            if (string.IsNullOrWhiteSpace(storagePath)) return;

            var bucket = _config["SupabaseStorage:Bucket"];
            var client = _httpClientFactory.CreateClient("SupabaseStorage");

            var response = await client.DeleteAsync($"/storage/v1/object/{bucket}/{storagePath}");

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "file", Code = "DELETE_FAILED", Message = $"Failed to delete image from storage: {error}" }
                });
            }
        }
    }
}
