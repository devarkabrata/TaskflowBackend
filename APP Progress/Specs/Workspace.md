# Workspace Feature Spec

## Overview
Manages the workspace-level member directory — all users (active + pending) who belong to a workspace. Drives the `PeopleScreen` in the frontend shell. Route base: `/api/people`.

A workspace is **auto-created on every new user signup** named `"{UserName}'s Workspace"`. The creator is immediately added as a workspace member with `status: Active`.

---

## Models Involved
- `Workspace` (exists in DB + AppDBContext + FluentAPI)
- `WorkspaceMember` (exists in DB + AppDBContext + FluentAPI)
- `WorkspaceInvitation` (exists in DB + AppDBContext + FluentAPI)
- `User` (exists — needed for member enrichment and title update)
- `TeamMember` (exists — needed for teamIds on member list items)

---

## Repositories to Create

### IWorkspaceRepository
```
GetByIdAsync(Guid id) → Workspace?
GetByOwnerIdAsync(Guid ownerId) → Workspace?
CreateAsync(Workspace workspace) → Workspace
```

### IWorkspaceMemberRepository
```
GetMembersAsync(Guid workspaceId, string? search, string? status, Guid? teamId, int page, int limit)
    → returns enriched data: User fields + List<Guid> TeamIds from TeamMembers table
GetByUserIdAsync(Guid workspaceId, Guid userId) → WorkspaceMember?
GetStatsAsync(Guid workspaceId) → (int total, int active, int pending, int totalTeams)
AddAsync(WorkspaceMember member) → WorkspaceMember
UpdateAsync(WorkspaceMember member) → WorkspaceMember
RemoveAsync(Guid workspaceId, Guid userId) → bool
```

### IWorkspaceInvitationRepository
```
GetPendingByEmailAsync(Guid workspaceId, string email) → WorkspaceInvitation?
CreateAsync(WorkspaceInvitation invitation) → WorkspaceInvitation
UpdateAsync(WorkspaceInvitation invitation) → WorkspaceInvitation
```

---

## Service: IWorkspaceService

### CreateDefaultWorkspaceAsync(Guid userId, string userName)
- Called by AuthService on signup
- Creates Workspace: Name = "{userName}'s Workspace", OwnerId = userId
- Creates WorkspaceMember: WorkspaceId = new workspace Id, UserId = userId, Status = Active, JoinedAt = DateTime.UtcNow

### GetPeopleAsync(Guid workspaceId, string? search, string? status, Guid? teamId, int page, int limit) → List<PeopleListItemDto>
- Returns active WorkspaceMembers enriched with User data and their TeamIds
- Filters: search (name/email substring), status (active/pending), teamId
- Pending invitations appear in list with status = "pending" and no user data fields except Email

### GetStatsAsync(Guid workspaceId) → PeopleStatsDto
- TotalMembers = count of WorkspaceMembers
- Active = count where Status = Active
- PendingInvites = count of pending WorkspaceInvitations
- TotalTeams = count of Teams in workspace

### InviteAsync(Guid workspaceId, Guid invitedBy, string email) → (WorkspaceInvitationResponseDto result, bool isNew)
- Check if active WorkspaceMember with that email exists → throw ValidationException 409 "ALREADY_MEMBER"
- Check if pending WorkspaceInvitation exists → resend (reset ExpiresAt to now+7days, save) → return existing (isNew=false)
- Else → create new WorkspaceInvitation → return (isNew=true)

### UpdateMemberAsync(Guid workspaceId, Guid userId, UpdateMemberRequestDto dto) → PeopleListItemDto
- Verify member exists in workspace → 404 if not
- Update User.Title via IUserRepository
- Return updated member as PeopleListItemDto

### RemoveMemberAsync(Guid workspaceId, Guid userId) → void
- Verify member exists → 404 if not
- Remove WorkspaceMember row
- Also remove all TeamMember rows for this user in teams belonging to this workspace

---

## DTOs to Create

### PeopleListItemDto
```csharp
Guid Id              // UserId (for active) or InvitationId (for pending)
string Name          // empty string for pending invitations
string Email
string Title         // empty string for pending
string AvatarInitials // empty for pending
string? AvatarUrl
List<Guid> TeamIds   // empty list for pending
string Status        // "active" | "pending"
```

### PeopleStatsDto
```csharp
int TotalMembers
int Active
int PendingInvites
int TotalTeams
```

### UpdateMemberRequestDto
```csharp
[StringLength(200)] string? Title
```

### Existing DTOs (reuse, no changes needed):
- `WorkspaceInviteRequestDto` — { Email }
- `WorkspaceInvitationResponseDto` — { Id, Email, Status, ExpiresAt }

---

## Endpoints

### GET /api/people
- Auth: Bearer
- Query params: teamId (Guid?), status (string?), search (string?), page (int, default 1), limit (int, default 50)
- Success: 200, returns List<PeopleListItemDto>
- Errors: 401 if not authenticated

### GET /api/people/stats
- Auth: Bearer
- Success: 200, returns PeopleStatsDto
- Errors: 401

### POST /api/people/invite
- Auth: Bearer
- Request body: WorkspaceInviteRequestDto { email }
- Success: 201 if new invite created, 200 if resent (existing pending reset)
- Returns: WorkspaceInvitationResponseDto
- Errors: 422 validation, 409 if already active member

### PATCH /api/people/{userId:guid}
- Auth: Bearer
- Request body: UpdateMemberRequestDto { title? }
- Success: 200, returns PeopleListItemDto
- Errors: 404 if not a workspace member, 422 validation

### DELETE /api/people/{userId:guid}
- Auth: Bearer
- Success: 200
- Errors: 404 if not a workspace member, 401

---

## Controller: PeopleController
- Route: `api/people`
- All routes: `[Authorize]`
- Workspace resolved from JWT sub → `IWorkspaceService` resolves workspace via `IWorkspaceRepository.GetByOwnerIdAsync(userId)`
- Extract userId: `Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!)`

---

## AuthService Change
`Services/AuthService.cs` — inject `IWorkspaceService`, call `CreateDefaultWorkspaceAsync(user.Id, user.Name)` after `_userService.CreateUser(...)` in `SignupAsync`.

---

## Business Rules
- `GET /api/people` unified list: active members + pending invitations together
- Resend invite: same email + pending → reset ExpiresAt, return 200 (NOT 409)
- Invite to existing active member → 409 ALREADY_MEMBER
- Delete member → removes from workspace + removes from all teams in that workspace
- Workspace resolved always by `GetByOwnerIdAsync(requestingUserId)` (v1: one workspace per user)
- PATCH updates User.Title, not a WorkspaceMember field

---

## Redis Usage
None for this feature.

---

## No EF migration needed
All models already in AppDBContext with Fluent API configured.
