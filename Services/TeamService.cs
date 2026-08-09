using TaskFlowBackend.DTOs.Events;
using TaskFlowBackend.DTOs.Teams;
using TaskFlowBackend.Enums;
using TaskFlowBackend.Helpers;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepo;
        private readonly ITeamMemberRepository _memberRepo;
        private readonly ITeamInvitationRepository _invitationRepo;
        private readonly IWorkspaceRepository _workspaceRepo;
        private readonly IWorkspaceMemberRepository _workspaceMemberRepo;
        private readonly IBoardStatusRepository _boardStatusRepo;
        private readonly IUserRepository _userRepo;
        private readonly IEventPublisherService _eventPublisher;

        public TeamService(
            ITeamRepository teamRepo,
            ITeamMemberRepository memberRepo,
            ITeamInvitationRepository invitationRepo,
            IWorkspaceRepository workspaceRepo,
            IWorkspaceMemberRepository workspaceMemberRepo,
            IBoardStatusRepository boardStatusRepo,
            IUserRepository userRepo,
            IEventPublisherService eventPublisher)
        {
            _teamRepo = teamRepo;
            _memberRepo = memberRepo;
            _invitationRepo = invitationRepo;
            _workspaceRepo = workspaceRepo;
            _workspaceMemberRepo = workspaceMemberRepo;
            _boardStatusRepo = boardStatusRepo;
            _userRepo = userRepo;
            _eventPublisher = eventPublisher;
        }

        public async Task<List<TeamResponseDto>> GetMyTeamsAsync(Guid userId, bool excludeWorkspace = false)
        {
            var teams = new List<Team>();
            if(excludeWorkspace)
            {
                teams = await _teamRepo.GetByUserMembershipAsync(userId);
                return teams.Select(MapToDto).ToList();
            }
            else
            {
                var workspace = await GetWorkspaceOrThrowAsync(userId);
                teams = await _teamRepo.GetByWorkspaceIdForUserAsync(workspace.Id, userId);
            }
            return teams.Select(MapToDto).ToList();
        }

        public async Task<TeamResponseDto> CreateTeamAsync(CreateTeamRequestDto dto, Guid userId)
        {
            var workspace = await GetWorkspaceOrThrowAsync(userId);

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Color = dto.Color,
                WorkspaceId = workspace.Id,
                AdminId = userId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _teamRepo.CreateAsync(team);

            var members = new List<TeamMember>
            {
                new TeamMember { TeamId = created.Id, UserId = userId, Role = TeamRole.Admin, JoinedAt = DateTime.UtcNow }
            };

            if (dto.MemberIds != null)
            {
                foreach (var m in dto.MemberIds.Where(m => m.UserId != userId))
                {
                    var wsMember = await _workspaceMemberRepo.GetByUserIdAsync(workspace.Id, m.UserId);
                    if (wsMember == null) continue;
                    members.Add(new TeamMember { TeamId = created.Id, UserId = m.UserId, Role = m.Role, JoinedAt = DateTime.UtcNow });
                }
            }

            await _memberRepo.AddRangeAsync(members);
            await _boardStatusRepo.SeedDefaultsAsync(created.Id);

            var fullTeam = await _teamRepo.GetByIdAsync(created.Id);

            // await PublishTeamCreatedEventsAsync(created, workspace, members, userId);

            return MapToDto(fullTeam!);
        }

        private async Task PublishTeamCreatedEventsAsync(Team team, Workspace workspace, List<TeamMember> members, Guid creatorId)
        {
            var creator = await _userRepo.GetUserByIdAsync(creatorId);

            await _eventPublisher.PublishAsync(RoutingKeys.TeamCreated, new TeamCreatedEvent
            {
                To = creator?.Email ?? string.Empty,
                TeamName = team.Name,
                CreatedBy = creator?.Name ?? string.Empty
            });

            foreach (var member in members.Where(m => m.UserId != creatorId))
            {
                var user = await _userRepo.GetUserByIdAsync(member.UserId);
                if (user == null) continue;

                await _eventPublisher.PublishAsync(RoutingKeys.MemberAdded, new MemberAddedEvent
                {
                    To = user.Email,
                    WorkspaceName = workspace.Name,
                    MemberName = user.Name,
                    InvitedBy = creator?.Name ?? string.Empty
                });
            }
        }

        public async Task<TeamResponseDto> GetTeamAsync(Guid teamId, Guid userId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId) ?? throw new NotFoundException("Team not found.");
            if (!team.Members.Any(m => m.UserId == userId))
                throw new ForbiddenException("You are not a member of this team.");
            return MapToDto(team);
        }

        public async Task<TeamStatsDto> GetStatsAsync(Guid userId)
        {
            var workspace = await GetWorkspaceOrThrowAsync(userId);
            var teams = await _teamRepo.GetByWorkspaceIdForUserAsync(workspace.Id, userId);
            return new TeamStatsDto
            {
                TotalTeams = teams.Count,
                TotalMembers = teams.Sum(t => t.Members.Count),
                PendingInvites = teams.Sum(t => t.Invitations.Count(i => i.Status == InvitationStatus.Pending))
            };
        }

        public async Task<TeamResponseDto> UpdateDetailsAsync(Guid teamId, UpdateTeamRequestDto dto, Guid userId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId) ?? throw new NotFoundException("Team not found.");
            if (team.AdminId != userId) throw new ForbiddenException("Only the team admin can update team details.");

            if (dto.Name != null) team.Name = dto.Name;
            if (dto.Description != null) team.Description = dto.Description;
            if (dto.Color != null) team.Color = dto.Color;
            team.UpdatedAt = DateTime.UtcNow;

            var updated = await _teamRepo.UpdateAsync(team);
            return MapToDto(updated);
        }

        public async Task<TeamResponseDto> SyncMembersAsync(Guid teamId, List<TeamMemberUpdateDto> incoming, Guid userId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId) ?? throw new NotFoundException("Team not found.");
            if (team.AdminId != userId) throw new ForbiddenException("Only the team admin can update team members.");

            if (!incoming.Any(m => m.UserId == team.AdminId))
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "members", Code = "CANNOT_REMOVE_ADMIN", Message = "The team admin cannot be removed from the team." }
                });

            var workspace = await GetWorkspaceOrThrowAsync(userId);

            foreach (var m in incoming)
            {
                var wsMember = await _workspaceMemberRepo.GetByUserIdAsync(workspace.Id, m.UserId);
                if (wsMember == null)
                    throw new ValidationException("Validation failed.", new List<ApiError>
                    {
                        new ApiError { Field = "members", Code = "NOT_WORKSPACE_MEMBER", Message = $"User {m.UserId} is not a member of this workspace." }
                    });
            }

            var currentMembers = team.Members.ToList();
            var incomingIds = incoming.ToDictionary(m => m.UserId, m => m.Role);
            var currentIds = currentMembers.ToDictionary(m => m.UserId, m => m);

            var toRemove = currentMembers.Where(m => !incomingIds.ContainsKey(m.UserId)).ToList();

            var toAdd = incoming
                .Where(m => !currentIds.ContainsKey(m.UserId))
                .Select(m => new TeamMember { TeamId = teamId, UserId = m.UserId, Role = m.Role, JoinedAt = DateTime.UtcNow })
                .ToList();

            var toUpdate = incoming
                .Where(m => currentIds.ContainsKey(m.UserId) && currentIds[m.UserId].Role != m.Role)
                .Select(m => { currentIds[m.UserId].Role = m.Role; return currentIds[m.UserId]; })
                .ToList();

            await _memberRepo.SyncAsync(toAdd, toRemove, toUpdate);

            if (team.Admin.Settings.IsTeamMemberNotificationEnabled)
            {   
                await PublishMemberAddedEventsAsync(toAdd, workspace, userId);
            }

            var refreshed = await _teamRepo.GetByIdAsync(teamId);
            return MapToDto(refreshed!);
        }

        private async Task PublishMemberAddedEventsAsync(List<TeamMember> added, Workspace workspace, Guid invitedByUserId)
        {
            if (added.Count == 0) return;

            var inviter = await _userRepo.GetUserByIdAsync(invitedByUserId);

            foreach (var member in added)
            {
                var user = await _userRepo.GetUserByIdAsync(member.UserId);
                if (user == null || !user.Settings.NotificationOnMemberAddToTeam) continue;

                await _eventPublisher.PublishAsync(RoutingKeys.MemberAdded, new MemberAddedEvent
                {
                    To = user.Email,
                    WorkspaceName = workspace.Name,
                    MemberName = user.Name,
                    InvitedBy = inviter?.Name ?? string.Empty
                });
            }
        }

        public async Task DeleteTeamAsync(Guid teamId, Guid userId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId) ?? throw new NotFoundException("Team not found.");
            if (team.AdminId != userId) throw new ForbiddenException("Only the team admin can delete the team.");
            await _teamRepo.DeleteAsync(team);
        }

        public async Task<TeamInvitationResponseDto> InviteToTeamAsync(Guid teamId, TeamInviteRequestDto dto, Guid userId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId) ?? throw new NotFoundException("Team not found.");
            if (team.AdminId != userId) throw new ForbiddenException("Only the team admin can invite members.");

            var existing = await _invitationRepo.GetByEmailAndTeamAsync(teamId, dto.Email);
            if (existing != null)
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "email", Code = "INVITE_ALREADY_PENDING", Message = "A pending invitation for this email already exists." }
                });

            var invitation = new TeamInvitation
            {
                Id = Guid.NewGuid(),
                TeamId = teamId,
                InvitedBy = userId,
                Email = dto.Email,
                Role = dto.Role,
                Status = InvitationStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _invitationRepo.AddAsync(invitation);
            return new TeamInvitationResponseDto
            {
                Id = created.Id,
                TeamId = created.TeamId,
                Email = created.Email,
                Role = created.Role.ToString(),
                Status = created.Status.ToString().ToLower(),
                ExpiresAt = created.ExpiresAt,
                CreatedAt = created.CreatedAt
            };
        }

        public async Task RemoveMemberAsync(Guid teamId, Guid targetUserId, Guid requesterId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId) ?? throw new NotFoundException("Team not found.");
            if (team.AdminId != requesterId) throw new ForbiddenException("Only the team admin can remove members.");
            if (team.AdminId == targetUserId)
                throw new ValidationException("Validation failed.", new List<ApiError>
                {
                    new ApiError { Field = "userId", Code = "CANNOT_REMOVE_ADMIN", Message = "The team admin cannot be removed." }
                });

            var member = team.Members.FirstOrDefault(m => m.UserId == targetUserId)
                ?? throw new NotFoundException("Member not found in this team.");

            await _memberRepo.RemoveAsync(member);
        }

        public async Task<int> GetTeamCountAsync(Guid userId)
        {
            return await _teamRepo.GetCountByUserMembershipAsync(userId);
        }

        private async Task<Workspace> GetWorkspaceOrThrowAsync(Guid userId)
        {
            var workspace = await _workspaceRepo.GetByOwnerIdAsync(userId);
            if (workspace == null) throw new NotFoundException("Workspace not found.");
            return workspace;
        }

        private static TeamResponseDto MapToDto(Team team) => new()
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            Color = team.Color,
            WorkspaceId = team.WorkspaceId,
            AdminId = team.AdminId,
            PendingInvites = team.Invitations.Count(i => i.Status == InvitationStatus.Pending),
            Members = team.Members.Select(m => new TeamMemberSummaryDto
            {
                UserId = m.UserId,
                Name = m.User.Name,
                AvatarInitials = m.User.AvatarInitials,
                AvatarUrl = m.User.AvatarUrl ?? "",
                Role = m.Role.ToString()
            }).ToList(),
            StatusTaskCounts = team.Tasks.GroupBy(task => task.Status.Name).ToDictionary(g => g.Key, g => g.Count()),
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt
        };
    }
}
