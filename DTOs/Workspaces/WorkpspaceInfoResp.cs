namespace TaskFlowBackend.DTOs.Workspaces
{
    public class WorkspaceInfoRespDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserInfo Owner {get; set;} = new();
        public List<TeamsInfo> Teams {get; set;} = new();
        public List<UserInfo> Members {get; set;} = new();
    }

    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    public class TeamsInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
