namespace TaskFlowBackend.DTOs.Workspaces
{
    public class PeopleStatsDto
    {
        public int TotalMembers { get; set; }
        public int Active { get; set; }
        public int PendingInvites { get; set; }
        public int TotalTeams { get; set; }
    }
}
