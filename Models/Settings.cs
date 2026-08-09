namespace TaskFlowBackend.Models
{
    public class Settings
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int DaysToArchieve { get; set; } = 2;

        // Notification settings
        public bool NotificationOnMemberAddToWorkspace { get; set; } = false;
        public bool NotificationOnMemberAddToTeam { get; set; } = false;
        public bool NotificationOnTaskAssignment { get; set; } = false;

        // Additional notification settings
        public bool IsTeamMemberNotificationEnabled { get; set; } = false;
        public bool IsWorkspaceMemberNotificationEnabled { get; set; } = false;
        public bool IsTaskCreationNotificationEnabled { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
