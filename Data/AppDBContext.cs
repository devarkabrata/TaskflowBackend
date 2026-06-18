using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Data.Fluent;
using TaskFlowBackend.Models;



namespace TaskFlowBackend.Data
{
    public class AppDBContext : DbContext
    {
        // Initiating DB Context from EF Core
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
        public DbSet<WorkspaceInvitation> WorkspaceInvitations => Set<WorkspaceInvitation>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<BoardStatus> BoardStatuses => Set<BoardStatus>();
        public DbSet<TeamInvitation> TeamInvitations => Set<TeamInvitation>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<Comment> Comments => Set<Comment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API configurations
            FluentAPIConfigurations.ConfigureUser(modelBuilder);
            FluentAPIConfigurations.ConfigureWorkspace(modelBuilder);
            FluentAPIConfigurations.ConfigureWorkspaceMember(modelBuilder);
            FluentAPIConfigurations.ConfigureWorkspaceInvitation(modelBuilder);
            FluentAPIConfigurations.ConfigureTeam(modelBuilder);
            FluentAPIConfigurations.ConfigureTeamMember(modelBuilder);
            FluentAPIConfigurations.ConfigureBoardStatus(modelBuilder);
            FluentAPIConfigurations.ConfigureTeamInvitation(modelBuilder);
            FluentAPIConfigurations.ConfigureTaskItem(modelBuilder);
            FluentAPIConfigurations.ConfigureComment(modelBuilder);
        }
    }
}
