namespace TaskFlowBackend.Models
{
    public class Settings
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int DaysToArchieve { get; set; } = 2;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
