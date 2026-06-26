using TaskFlowBackend.DTOs.Workspaces;
using TaskFlowBackend.Enums;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.CustomException;
using TaskFlowBackend.Helpers.Pagination;
using TaskFlowBackend.Models;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IWorkspaceRepository _workspaceRepo;
        private readonly IWorkspaceMemberRepository _memberRepo;
        private readonly IWorkspaceInvitationRepository _invitationRepo;
        private readonly IUserRepository _userRepo;

        public WorkspaceService(
            IWorkspaceRepository workspaceRepo,
            IWorkspaceMemberRepository memberRepo,
            IWorkspaceInvitationRepository invitationRepo,
            IUserRepository userRepo)
        {
            _workspaceRepo = workspaceRepo;
            _memberRepo = memberRepo;
            _invitationRepo = invitationRepo;
            _userRepo = userRepo;
        }

        public async Task<Workspace> CreateDefaultWorkspaceAsync(Guid userId, string userName)
        {
            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = $"{userName}'s Workspace",
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _workspaceRepo.CreateAsync(workspace);

            var member = new WorkspaceMember
            {
                Id = Guid.NewGuid(),
                WorkspaceId = created.Id,
                UserId = userId,
                Status = WorkspaceMemberStatus.Active,
                JoinedAt = DateTime.UtcNow
            };

            await _memberRepo.AddAsync(member);
            return created;
        }

        public async Task<PagedResult<PeopleListItemDto>> GetPeopleAsync(
            Guid requestingUserId, string? search, string? status, Guid? teamId, int page, int limit)
        {
            var workspace = await GetWorkspaceOrThrowAsync(requestingUserId);
            var pagination = new PaginationParams { Page = page, Limit = limit };

            bool includeActive = string.IsNullOrEmpty(status) || status.Equals("active", StringComparison.OrdinalIgnoreCase);
            bool includePending = string.IsNullOrEmpty(status) || status.Equals("pending", StringComparison.OrdinalIgnoreCase);

            // Active only — DB-level pagination
            if (includeActive && !includePending)
            {
                var (members, total) = await _memberRepo.GetMembersAsync(workspace.Id, search, teamId, pagination);
                var dtos = await MapMembersToDtosAsync(workspace.Id, members);
                return new PagedResult<PeopleListItemDto> { Items = dtos, TotalCount = total, Page = page, Limit = limit };
            }

            // Pending only — DB-level pagination (teamId filter irrelevant for invitations)
            if (includePending && !includeActive && teamId == null)
            {
                var (invitations, total) = await _invitationRepo.GetAllPendingAsync(workspace.Id, search, pagination);
                var dtos = invitations.Select(MapInvitationToListItemDto).ToList();
                return new PagedResult<PeopleListItemDto> { Items = dtos, TotalCount = total, Page = page, Limit = limit };
            }

            // Combined — fetch all from both sources, merge, in-memory paginate
            var (allMembers, memberTotal) = await _memberRepo.GetMembersAsync(workspace.Id, search, teamId);
            var memberDtos = await MapMembersToDtosAsync(workspace.Id, allMembers);
            var combined = new List<PeopleListItemDto>(memberDtos);
            var combinedTotal = memberTotal;

            if (teamId == null)
            {
                var (allInvitations, invTotal) = await _invitationRepo.GetAllPendingAsync(workspace.Id, search);
                combined.AddRange(allInvitations.Select(MapInvitationToListItemDto));
                combinedTotal += invTotal;
            }

            var paged = combined.Skip(pagination.Skip).Take(pagination.Limit).ToList();
            return new PagedResult<PeopleListItemDto> { Items = paged, TotalCount = combinedTotal, Page = page, Limit = limit };
        }

        public async Task<PeopleStatsDto> GetStatsAsync(Guid requestingUserId)
        {
            var workspace = await GetWorkspaceOrThrowAsync(requestingUserId);
            var (total, active, pendingInvites, totalTeams) = await _memberRepo.GetStatsAsync(workspace.Id);
            return new PeopleStatsDto
            {
                TotalMembers = total,
                Active = active,
                PendingInvites = pendingInvites,
                TotalTeams = totalTeams
            };
        }

        public async Task<(WorkspaceInvitationResponseDto dto, bool isNew)> InviteAsync(Guid requestingUserId, string email)
        {
            var workspace = await GetWorkspaceOrThrowAsync(requestingUserId);

            var existingUser = await _userRepo.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                var existingMember = await _memberRepo.GetByUserIdAsync(workspace.Id, existingUser.Id);
                if (existingMember != null)
                    throw new ValidationException("Validation failed.", new List<ApiError>
                    {
                        new ApiError { Field = "email", Code = "ALREADY_MEMBER", Message = "This user is already a member of the workspace." }
                    });
            }

            var existing = await _invitationRepo.GetPendingByEmailAsync(workspace.Id, email);
            if (existing != null)
            {
                existing.ExpiresAt = DateTime.UtcNow.AddDays(7);
                existing.UpdatedAt = DateTime.UtcNow;
                var updated = await _invitationRepo.UpdateAsync(existing);
                return (MapInvitationToDto(updated), false);
            }

            var invitation = new WorkspaceInvitation
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspace.Id,
                InvitedBy = requestingUserId,
                Email = email,
                Status = InvitationStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var created = await _invitationRepo.CreateAsync(invitation);
            return (MapInvitationToDto(created), true);
        }

        public async Task<PeopleListItemDto> UpdateMemberAsync(Guid requestingUserId, Guid targetUserId, UpdateMemberRequestDto dto)
        {
            var workspace = await GetWorkspaceOrThrowAsync(requestingUserId);
            var member = await _memberRepo.GetByUserIdAsync(workspace.Id, targetUserId);
            if (member == null)
                throw new NotFoundException("Member not found in this workspace.");

            if (dto.Title != null)
            {
                var user = await _userRepo.GetUserByIdAsync(targetUserId);
                if (user != null)
                {
                    user.Title = dto.Title;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepo.UpdateUserAsync(user);
                }
            }

            var refreshed = await _memberRepo.GetByUserIdAsync(workspace.Id, targetUserId);
            var teamIdsMap = await _memberRepo.GetUserTeamIdsAsync(workspace.Id, new List<Guid> { targetUserId });

            return new PeopleListItemDto
            {
                Id = refreshed!.UserId,
                Name = refreshed.User.Name,
                Email = refreshed.User.Email,
                Title = refreshed.User.Title,
                AvatarInitials = refreshed.User.AvatarInitials,
                AvatarUrl = refreshed.User.AvatarUrl,
                TeamIds = teamIdsMap.TryGetValue(targetUserId, out var ids) ? ids : new List<Guid>(),
                Status = "active"
            };
        }

        public async Task RemoveMemberAsync(Guid requestingUserId, Guid targetUserId)
        {
            var workspace = await GetWorkspaceOrThrowAsync(requestingUserId);
            var member = await _memberRepo.GetByUserIdAsync(workspace.Id, targetUserId);
            if (member == null)
                throw new NotFoundException("Member not found in this workspace.");

            await _memberRepo.RemoveAsync(workspace.Id, targetUserId);
        }

        private async Task<Workspace> GetWorkspaceOrThrowAsync(Guid userId)
        {
            var workspace = await _workspaceRepo.GetByOwnerIdAsync(userId);
            if (workspace == null)
                throw new NotFoundException("Workspace not found.");
            return workspace;
        }

        private async Task<List<PeopleListItemDto>> MapMembersToDtosAsync(Guid workspaceId, List<WorkspaceMember> members)
        {
            if (!members.Any()) return new List<PeopleListItemDto>();

            var userIds = members.Select(m => m.UserId).ToList();
            var teamIdsMap = await _memberRepo.GetUserTeamIdsAsync(workspaceId, userIds);

            return members.Select(m => new PeopleListItemDto
            {
                Id = m.UserId,
                Name = m.User.Name,
                Email = m.User.Email,
                Title = m.User.Title,
                AvatarInitials = m.User.AvatarInitials,
                AvatarUrl = m.User.AvatarUrl,
                TeamIds = teamIdsMap.TryGetValue(m.UserId, out var ids) ? ids : new List<Guid>(),
                Status = "active"
            }).ToList();
        }

        private static PeopleListItemDto MapInvitationToListItemDto(WorkspaceInvitation i) => new()
        {
            Id = i.Id,
            Name = string.Empty,
            Email = i.Email,
            Title = string.Empty,
            AvatarInitials = string.Empty,
            AvatarUrl = null,
            TeamIds = new List<Guid>(),
            Status = "pending"
        };

        private static WorkspaceInvitationResponseDto MapInvitationToDto(WorkspaceInvitation i) => new()
        {
            Id = i.Id,
            Email = i.Email,
            Status = i.Status.ToString().ToLower(),
            ExpiresAt = i.ExpiresAt
        };
    }
}
