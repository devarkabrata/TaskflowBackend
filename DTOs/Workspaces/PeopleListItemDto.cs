namespace TaskFlowBackend.DTOs.Workspaces
{
    public class PeopleListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AvatarInitials { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public List<Guid> TeamIds { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }
}
