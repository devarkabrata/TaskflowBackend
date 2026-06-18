using Microsoft.EntityFrameworkCore;
using TaskFlowBackend.Enums;
using TaskFlowBackend.Models;

namespace TaskFlowBackend.Data.Fluent
{
    public class FluentAPIConfigurations
    {
        public static void ConfigureComment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("comments");

                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(c => c.Body)
                    .IsRequired();

                // Parallel arrays — ImagePublicIds[i] is the Cloudinary public_id for ImageUrls[i]
                entity.Property(c => c.ImageUrls)
                    .HasColumnType("text[]")
                    .HasDefaultValueSql("'{}'::text[]");

                entity.Property(c => c.ImagePublicIds)
                    .HasColumnType("text[]")
                    .HasDefaultValueSql("'{}'::text[]");

                entity.Property(c => c.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(c => c.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(c => c.Task)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(c => c.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Author)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public static void ConfigureTaskItem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.ToTable("tasks");

                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(t => t.TaskNumber)
                    .IsRequired();

                entity.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(t => t.Description);

                entity.Property(t => t.Priority)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasDefaultValue(Priority.Medium);

                // Nullable label — a task can have one label or none
                entity.Property(t => t.Label)
                    .HasConversion<string>();

                entity.Property(t => t.Progress)
                    .IsRequired()
                    .HasColumnType("smallint")
                    .HasDefaultValue(0);

                // Multiple assignees stored as uuid[] — no join table needed
                entity.Property(t => t.AssigneeIds)
                    .HasColumnType("uuid[]")
                    .HasDefaultValueSql("'{}'::uuid[]");

                entity.Property(t => t.ExpectedCompletion);

                entity.Property(t => t.DeletedAt);

                entity.Property(t => t.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(t => t.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(t => t.Status)
                    .WithMany(bs => bs.Tasks)
                    .HasForeignKey(t => t.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Team)
                    .WithMany(tm => tm.Tasks)
                    .HasForeignKey(t => t.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Creator)
                    .WithMany(u => u.CreatedTasks)
                    .HasForeignKey(t => t.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public static void ConfigureTeamInvitation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamInvitation>(entity =>
            {
                entity.ToTable("invitations");

                entity.HasKey(ti => ti.Id);
                entity.Property(ti => ti.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(ti => ti.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                // One pending invite per email per team
                entity.HasIndex(ti => new { ti.TeamId, ti.Email })
                    .IsUnique();

                entity.Property(ti => ti.Role)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasDefaultValue(TeamRole.Developer);

                entity.Property(ti => ti.Status)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasDefaultValue(InvitationStatus.Pending);

                entity.Property(ti => ti.ExpiresAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW() + INTERVAL '7 days'");

                entity.Property(ti => ti.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(ti => ti.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(ti => ti.Team)
                    .WithMany(t => t.Invitations)
                    .HasForeignKey(ti => ti.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ti => ti.Sender)
                    .WithMany(u => u.SentTeamInvitations)
                    .HasForeignKey(ti => ti.InvitedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public static void ConfigureTeamMember(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamMember>(entity =>
            {
                entity.ToTable("team_members");

                // Composite PK — no separate Id column, matches the DB schema
                entity.HasKey(tm => new { tm.TeamId, tm.UserId });

                entity.Property(tm => tm.Role)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasDefaultValue(TeamRole.Developer);

                entity.Property(tm => tm.JoinedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(tm => tm.Team)
                    .WithMany(t => t.Members)
                    .HasForeignKey(tm => tm.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tm => tm.User)
                    .WithMany(u => u.TeamMemberships)
                    .HasForeignKey(tm => tm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public static void ConfigureBoardStatus(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoardStatus>(entity =>
            {
                entity.ToTable("board_statuses");

                entity.HasKey(bs => bs.Id);
                entity.Property(bs => bs.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(bs => bs.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(bs => bs.Description)
                    .HasMaxLength(300);

                entity.Property(bs => bs.Position)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(bs => bs.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(bs => bs.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                // A team cannot have two statuses with the same name
                entity.HasIndex(bs => new { bs.TeamId, bs.Name })
                    .IsUnique();

                // Index for ordering queries — GET /api/board/:teamId returns statuses sorted by position
                entity.HasIndex(bs => new { bs.TeamId, bs.Position });

                entity.HasOne(bs => bs.Team)
                    .WithMany(t => t.BoardStatuses)
                    .HasForeignKey(bs => bs.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public static void ConfigureTeam(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("teams");

                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.Description)
                    .HasMaxLength(500);

                // Hex color — always 7 chars e.g. "#6155DD"
                entity.Property(t => t.Color)
                    .IsRequired()
                    .HasMaxLength(7)
                    .HasDefaultValue("#6155DD");

                entity.Property(t => t.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(t => t.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(t => t.Workspace)
                    .WithMany(w => w.Teams)
                    .HasForeignKey(t => t.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Two separate FKs to User — must name the FK columns explicitly
                entity.HasOne(t => t.Admin)
                    .WithMany(u => u.AdminTeams)
                    .HasForeignKey(t => t.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Creator)
                    .WithMany(u => u.CreatedTeams)
                    .HasForeignKey(t => t.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public static void ConfigureWorkspaceInvitation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkspaceInvitation>(entity =>
            {
                entity.ToTable("workspace_invitations");

                entity.HasKey(wi => wi.Id);
                entity.Property(wi => wi.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(wi => wi.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                // One pending invite per email per workspace
                entity.HasIndex(wi => new { wi.WorkspaceId, wi.Email })
                    .IsUnique();

                entity.Property(wi => wi.Status)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasDefaultValue(InvitationStatus.Pending);

                entity.Property(wi => wi.ExpiresAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW() + INTERVAL '7 days'");

                entity.Property(wi => wi.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(wi => wi.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(wi => wi.Workspace)
                    .WithMany(w => w.Invitations)
                    .HasForeignKey(wi => wi.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wi => wi.Sender)
                    .WithMany(u => u.SentWorkspaceInvitations)
                    .HasForeignKey(wi => wi.InvitedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public static void ConfigureWorkspaceMember(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkspaceMember>(entity =>
            {
                entity.ToTable("workspace_members");

                entity.HasKey(wm => wm.Id);
                entity.Property(wm => wm.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                // Store enum as string ("active" | "pending")
                entity.Property(wm => wm.Status)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasDefaultValue(WorkspaceMemberStatus.Pending);

                // Nullable — null until invite is accepted
                entity.Property(wm => wm.JoinedAt);

                // One user can only appear once per workspace
                entity.HasIndex(wm => new { wm.WorkspaceId, wm.UserId })
                    .IsUnique();

                entity.HasOne(wm => wm.Workspace)
                    .WithMany(w => w.Members)
                    .HasForeignKey(wm => wm.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wm => wm.User)
                    .WithMany(u => u.WorkspaceMemberships)
                    .HasForeignKey(wm => wm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public static void ConfigureWorkspace(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Workspace>(entity =>
            {
                entity.ToTable("workspaces");

                entity.HasKey(w => w.Id);
                entity.Property(w => w.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(w => w.Name)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(w => w.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(w => w.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                // One User can own many Workspaces (multi-workspace is future scope)
                entity.HasOne(w => w.Owner)
                    .WithMany(u => u.OwnedWorkspaces)
                    .HasForeignKey(w => w.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public static void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.Title)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasDefaultValue(string.Empty);

                entity.Property(u => u.AvatarInitials)
                    .IsRequired()
                    .HasMaxLength(2);

                entity.Property(u => u.AvatarUrl)
                    .HasMaxLength(500);

                entity.Property(u => u.AvatarPublicId)
                    .HasMaxLength(300);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(u => u.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");
            });
        }
    }
}