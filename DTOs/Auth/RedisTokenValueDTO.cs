namespace TaskFlowBackend.DTOs
{
    public class RedisTokenValueDTO
    {
        public Guid UserId {get; set;}

        public string Email {get; set;} = string.Empty;

        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

        public string DeviceInfo {get; set;} = string.Empty;
    }
}