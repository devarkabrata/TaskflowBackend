# Taskflow Backend — API Flow Reference

> This document reflects the **actual implemented architecture**. It is the backend team's single navigable view of the full API contract.
>
> Last updated: 2026-06-27

---

## Table of Contents

1. [Overview](#1-overview)
2. [Authentication & Session](#2-authentication--session)
3. [Standard Response Envelope](#3-standard-response-envelope)
4. [Service Catalogue](#4-service-catalogue)
   - [Auth Service](#41-auth-service--apiauth)
   - [Task Service](#42-task-service--apitasks)
   - [Board Service](#43-board-service--apiboard)
   - [People / Workspace Service](#44-people--workspace-service--apipeople)
   - [Team Service](#45-team-service--apiteams)
   - [Dashboard Service](#46-dashboard-service--apidashboard)
   - [User Service](#47-user-service--apiusers)
   - [Project Service](#48-project-service--apiprojects)
   - [Activity Service](#49-activity-service--apiactivity)
   - [Notification Service](#410-notification-service--apinotifications)
   - [User Preferences Service](#411-user-preferences-service--apipreferences)
5. [Core Data Models](#5-core-data-models)
6. [Frontend → Endpoint Traceability](#6-frontend--endpoint-traceability)
7. [Cross-Cutting Concerns](#7-cross-cutting-concerns)

---

## 1. Overview

### Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8.0 |
| Primary DB | PostgreSQL (Supabase) — structured, relational, transactional data |
| Cache / Session | Redis (Upstash) via StackExchange.Redis — refresh token storage |
| Auth | JWT Bearer token (`Authorization: Bearer <token>`) + refresh token |
| Image Storage | Cloudinary (client-side signed upload; URLs stored in DB) |
| API Style | REST, JSON, standard `ApiResponse<T>` envelope on every endpoint |

### Service Map

11 service groups, all rooted at `/api/*`:

| # | Service | Base Path | Database | Auth Required | Status |
|---|---|---|---|---|---|
| 1 | Auth | `/api/auth` | PostgreSQL + Redis | Mixed | **Done** |
| 2 | Task | `/api/tasks` | PostgreSQL | Auth | Pending |
| 3 | Board | `/api/board` | PostgreSQL | Auth | Pending |
| 4 | People / Workspace | `/api/people` | PostgreSQL | Auth | **Done** |
| 5 | Team | `/api/teams` | PostgreSQL | Auth | Pending |
| 6 | Dashboard | `/api/dashboard` | PostgreSQL (computed) | Auth | Pending |
| 7 | User | `/api/users` | PostgreSQL | Auth | Pending |
| 8 | Project | `/api/projects` | PostgreSQL | Auth | Pending |
| 9 | Activity | `/api/activity` | MongoDB | Auth | Deferred |
| 10 | Notification | `/api/notifications` | MongoDB | Auth | Deferred |
| 11 | User Preferences | `/api/preferences` | MongoDB | Auth | Deferred |

---

## 2. Authentication & Session

### Mechanism

JWT Bearer token. All protected endpoints require:
```
Authorization: Bearer <accessToken>
```

Tokens are issued on login. Access token lifetime: **60 minutes**. Refresh token lifetime: **7 days** (stored in Redis).

### Token Pair

| Token | TTL | Storage | Purpose |
|---|---|---|---|
| Access token (JWT) | 60 min | Client memory / localStorage | Auth gating on every request |
| Refresh token (opaque, 32-byte Base64) | 7 days | Redis (`refresh_token:{token}` key) | Obtain new access token without re-login |

### JWT Claims

| Claim | Value |
|---|---|
| `sub` | User UUID |
| `email` | User email |
| `name` | User display name |
| `jti` | Unique token ID |
| `iat` | Unix timestamp (integer) |
| `avatarUrl` | Cloudinary URL or empty string |
| `title` | User job title or empty string |
| `iss`, `aud`, `exp` | Set by `JwtSecurityToken` constructor — never add manually |

### Login Flow

```
Client  →  POST /api/auth/login  { email, password }
        ←  200 { token, refreshToken }

Client sends on every protected request:
        →  Authorization: Bearer <token>
        ←  200 ...

When token expires:
        →  PATCH /api/auth/refresh  { refreshToken }
        ←  200 { token, refreshToken }
```

### Redis Refresh Token

Key: `refresh_token:{token}`
Value: `{ userId, email, createdAt, deviceInfo }`
TTL: 7 days. Deleted/replaced on refresh.

---

## 3. Standard Response Envelope

Every endpoint — success or failure — returns `ApiResponse<T>` serialized as camelCase JSON.

### Success (single object)

```json
{
  "status": true,
  "code": 200,
  "result": { "id": "uuid", "...": "..." },
  "message": "Task updated successfully.",
  "errors": [],
  "devMessage": "",
  "requestId": "trace-id",
  "timestamp": "2026-06-27T00:00:00.000Z",
  "source": "Dotnet 8.0.0 web api"
}
```

### Success (paginated list)

```json
{
  "status": true,
  "code": 200,
  "result": {
    "data": [],
    "count": 20,
    "total": 142,
    "page": 1,
    "limit": 20,
    "totalPages": 8
  },
  "message": "",
  "errors": [],
  "devMessage": "",
  "requestId": "trace-id",
  "timestamp": "2026-06-27T00:00:00.000Z",
  "source": "Dotnet 8.0.0 web api"
}
```

### Failure

```json
{
  "status": false,
  "code": 422,
  "result": null,
  "message": "Validation failed.",
  "errors": [
    { "field": "email", "code": "INVALID_FORMAT", "message": "Enter a valid email address." },
    { "field": "dueDate", "code": "REQUIRED",      "message": "Due date is required." }
  ],
  "devMessage": "ValidationException thrown at AuthService.SignupAsync()",
  "requestId": "trace-id",
  "timestamp": "2026-06-27T00:00:00.000Z",
  "source": "Dotnet 8.0.0 web api"
}
```

### Field Reference

| Field | Type | Notes |
|---|---|---|
| `status` | `boolean` | `true` = success, `false` = any error |
| `code` | `number` | Mirrors the HTTP status code |
| `result` | `object \| null` | The payload. `null` on any failure — never absent |
| `message` | `string` | User-displayable string. `""` if nothing to show |
| `errors` | `array` | `[]` on success. Each item: `{ field, code, message }` |
| `devMessage` | `string` | Internal detail. Always `""` in production |
| `requestId` | `string` | `HttpContext.TraceIdentifier` — correlates logs |
| `timestamp` | `string` | ISO 8601 UTC |
| `source` | `string` | Always `"Dotnet 8.0.0 web api"` |

### Standard HTTP Error Codes

| Code | When |
|---|---|
| `400` | Malformed request body / bad JSON |
| `401` | Missing or invalid Bearer token |
| `403` | Valid token but insufficient permissions |
| `404` | Resource not found |
| `409` | Conflict — duplicate invite, duplicate email |
| `422` | Validation failed — `errors[]` populated |
| `500` | Unexpected server error |

---

## 4. Service Catalogue

### 4.1 Auth Service — `/api/auth`

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/signup` | Public | Register — creates user + default workspace |
| POST | `/api/auth/login` | Public | Email + password → token pair |
| PATCH | `/api/auth/refresh` | Public | Exchange refresh token for new access token |

#### `POST /api/auth/signup`

**Request**
```json
{ "name": "string", "email": "string", "password": "string", "title": "string (optional)" }
```

**Response `201`**
```json
{
  "result": {
    "id": "uuid",
    "name": "Alice Smith",
    "email": "alice@example.com",
    "title": "Engineer",
    "avatarInitials": "AS"
  }
}
```

Side effects:
- Password BCrypt-hashed before storage
- Default workspace created (`"{name}'s Workspace"`)
- User added to workspace as active member

**Error `409`** — email already registered (`EMAIL_TAKEN`)
**Error `422`** — validation failure

---

#### `POST /api/auth/login`

**Request**
```json
{ "email": "string", "password": "string" }
```

**Response `200`**
```json
{
  "result": {
    "token": "eyJhbGci...",
    "refreshToken": "base64-opaque-string",
    "user": null
  }
}
```

Refresh token stored in Redis with 7-day TTL.

**Error `401`** — email not found or wrong password

---

#### `PATCH /api/auth/refresh`

**Request**
```json
{ "refreshToken": "base64-opaque-string" }
```

**Response `200`**
```json
{
  "result": {
    "token": "eyJhbGci...",
    "refreshToken": "base64-opaque-string"
  }
}
```

**Error `404`** — refresh token not found or expired in Redis

---

### 4.2 Task Service — `/api/tasks`

Drives the **Task MFE** (`mfe-task`).

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/tasks` | Auth | List tasks (filterable + paginated) |
| POST | `/api/tasks` | Auth | Create a task |
| GET | `/api/tasks/:id` | Auth | Get single task |
| PATCH | `/api/tasks/:id` | Auth | Update task fields (partial) |
| DELETE | `/api/tasks/:id` | Auth | Soft-delete task |
| GET | `/api/tasks/stats` | Auth | Aggregate counts by status |
| PATCH | `/api/tasks/:id/status` | Auth | Drag-drop: change task status |

#### Query Params for `GET /api/tasks`

```
statusId=stat_1
priority=high|medium|low
teamId=team_1
assigneeId=uuid
page=1&limit=20
```

Soft-deleted tasks (`deletedAt IS NOT NULL`) are always excluded.

#### `GET /api/tasks/stats` Response
```json
{ "total": 7, "todo": 2, "inProgress": 2, "review": 2, "done": 1 }
```

#### `POST /api/tasks` Request
```json
{
  "title": "string",
  "description": "<p>rich-text body</p>",
  "priority": "high",
  "statusId": "uuid",
  "label": "feature",
  "assigneeId": "uuid",
  "teamId": "uuid",
  "expectedCompletion": "2026-06-20",
  "progress": 0,
  "imageUrls": ["https://res.cloudinary.com/..."]
}
```

#### `PATCH /api/tasks/:id/status`
```json
{ "statusId": "uuid" }
```
**`403`** if a `developer` attempts to move a task they don't own.

---

### 4.3 Board Service — `/api/board`

Drives the **Board MFE** (`mfe-board`).

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/board/:teamId` | Auth | Team's statuses + first 5 tasks per column |
| GET | `/api/board/:teamId/status/:statusId/tasks` | Auth | Load more tasks for one column |
| POST | `/api/board/:teamId/statuses` | Auth | Create a status column (admin / pm) |
| PATCH | `/api/board/:teamId/statuses/:statusId` | Auth | Edit a status (admin / pm / tl) |
| DELETE | `/api/board/:teamId/statuses/:statusId` | Auth | Delete status; soft-deletes its tasks |

#### `GET /api/board/:teamId` Response
```json
{
  "statuses": [
    { "id": "uuid", "name": "Backlog", "description": "Not yet started", "position": 0, "totalTasks": 8, "tasks": [] },
    { "id": "uuid", "name": "In Progress", "description": null, "position": 1, "totalTasks": 3, "tasks": [] }
  ]
}
```

#### `DELETE /api/board/:teamId/statuses/:statusId`
Soft-deletes all tasks in the column.
**`422`** if this is the team's last remaining status.
Response: `{ "softDeletedTaskCount": 8 }`

#### Role Restrictions (Board)

| Action | Allowed Roles |
|---|---|
| Create status | admin, pm |
| Edit status | admin, pm, tl |
| Delete status | admin, pm, tl |
| Drag-drop own task | all (developer: only their own) |
| Drag-drop any task | admin, pm, tl |

---

### 4.4 People / Workspace Service — `/api/people`

Drives the **Shell** `PeopleScreen`. Manages workspace-level member directory (active members + pending invitations). Distinct from `/api/teams/:id/members` which is team-scoped.

Workspace ID is always resolved server-side from the JWT `sub` claim — clients never send a workspace ID.

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/people` | Auth | Paginated list of active members + pending invitations |
| GET | `/api/people/stats` | Auth | Aggregate counts (total, active, pending, teams) |
| POST | `/api/people/invite` | Auth | Invite someone to the workspace by email |
| PATCH | `/api/people/:userId` | Auth | Update member title |
| DELETE | `/api/people/:userId` | Auth | Remove member or cancel pending invitation |

#### Query Params for `GET /api/people`

```
teamId=uuid           — filter by team membership (active members only)
status=active|pending — filter by status (default: both)
search=string         — name or email substring
page=1&limit=20
```

#### `GET /api/people` Response `200`

```json
{
  "result": {
    "data": [
      {
        "id": "uuid",
        "name": "Alice Smith",
        "email": "alice@example.com",
        "title": "Engineer",
        "avatarInitials": "AS",
        "avatarUrl": "https://res.cloudinary.com/...",
        "teamIds": ["uuid1", "uuid2"],
        "status": "active"
      },
      {
        "id": "invitation-uuid",
        "name": "",
        "email": "bob@external.com",
        "title": "",
        "avatarInitials": "",
        "avatarUrl": null,
        "teamIds": [],
        "status": "pending"
      }
    ],
    "count": 2,
    "total": 25,
    "page": 1,
    "limit": 20,
    "totalPages": 2
  }
}
```

> For pending entries, `id` is the invitation UUID (not a user UUID). Pass this as `:userId` in `DELETE /api/people/:userId` to cancel the invitation.

#### `GET /api/people/stats` Response `200`

```json
{
  "result": {
    "totalMembers": 5,
    "active": 4,
    "pendingInvites": 1,
    "totalTeams": 3
  }
}
```

#### `POST /api/people/invite`

**Request**
```json
{ "email": "colleague@example.com" }
```

**Response `201`** — new invitation
```json
{
  "result": {
    "id": "uuid",
    "email": "colleague@example.com",
    "status": "pending",
    "expiresAt": "2026-07-04T00:00:00Z"
  }
}
```

**Response `200`** — email already has a pending invite; expiry reset to +7 days from now (resend).

**Error `409`** — email belongs to an existing active member (`ALREADY_MEMBER`)

---

#### `PATCH /api/people/:userId`

**Request** — any subset of updatable fields
```json
{ "title": "Senior Engineer" }
```

**Response `200`** — updated `PeopleListItemDto` (full member object)

**Error `404`** — member not found in workspace

---

#### `DELETE /api/people/:userId`

Handles two cases:
1. **Active member** — removes `WorkspaceMember` row and strips from all workspace teams
2. **Pending invitation** — hard-deletes the `WorkspaceInvitation` row (cancel invite)

**Response `200`** — `result: null`

**Error `404`** — neither a member nor a pending invitation with that ID

---

### 4.5 Team Service — `/api/teams`

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/teams` | Auth | List teams the current user belongs to |
| POST | `/api/teams` | Auth | Create a new team |
| GET | `/api/teams/:id` | Auth | Get team details with members |
| PATCH | `/api/teams/:id` | Auth | Update name / description / color (admin only) |
| DELETE | `/api/teams/:id` | Auth | Delete team (admin only) |
| GET | `/api/teams/stats` | Auth | Aggregate: total teams, members, pending invites |
| POST | `/api/teams/:id/members` | Auth | Add an existing workspace member to the team |
| POST | `/api/teams/:id/invite` | Auth | Send email invitation to a non-member |
| DELETE | `/api/teams/:id/members/:userId` | Auth | Remove a member (not from workspace) |
| PATCH | `/api/teams/:id/members/:userId` | Auth | Change member role |

#### `POST /api/teams` Request
```json
{
  "name": "Frontend Team",
  "description": "optional",
  "color": "#6155DD",
  "memberIds": [{ "userId": "uuid", "role": "developer" }]
}
```

`color` — required hex from the 8-swatch picker.
Creator is automatically added as `admin` server-side.
Three default `BoardStatus` rows seeded on creation: **Backlog** (pos 1), **In Progress** (pos 2), **Done** (pos 3).

#### `POST /api/teams/:id/invite`
```json
{ "email": "colleague@example.com", "role": "developer", "addToWorkspace": false }
```
`addToWorkspace: true` also creates a `workspace_invitation`.
**`409`** if a pending invite already exists for that email + team.

#### `PATCH /api/teams/:id/members/:userId`
```json
{ "role": "pm" }
```
**`422`** if attempting to demote the only `admin`.

#### `DELETE /api/teams/:id/members/:userId`
Removes from team only — workspace membership untouched.
**`422`** if attempting to remove the only `admin`.

---

### 4.6 Dashboard Service — `/api/dashboard`

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/dashboard/stats` | Auth | Aggregate stats for the current user |

#### Response
```json
{
  "totalTasks": 142,
  "inProgress": 28,
  "completed": 96,
  "boardItems": 18,
  "completionRate": 67
}
```

---

### 4.7 User Service — `/api/users`

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/users` | Auth | List users (assignee picker) |
| GET | `/api/users/:id` | Auth | Get user profile |
| PATCH | `/api/users/:id` | Auth | Update own profile |

---

### 4.8 Project Service — `/api/projects`

> Deferred from v1 scope.

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/projects` | Auth | List projects |
| POST | `/api/projects` | Auth | Create project |
| GET | `/api/projects/:id` | Auth | Get project details |
| GET | `/api/projects/:id/sprints` | Auth | List sprints |
| POST | `/api/projects/:id/sprints` | Auth | Create sprint |
| PATCH | `/api/projects/:id/sprints/:sprintId` | Auth | Update sprint |

---

### 4.9 Activity Service — `/api/activity`

> Backed by MongoDB. Deferred from v1 scope.

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/activity` | Auth | Recent activity feed (current user) |
| GET | `/api/activity/tasks/:taskId` | Auth | Activity timeline for a task |

---

### 4.10 Notification Service — `/api/notifications`

> Backed by MongoDB. Deferred from v1 scope.

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/notifications` | Auth | List unread notifications |
| PATCH | `/api/notifications/:id/read` | Auth | Mark one as read |
| PATCH | `/api/notifications/read-all` | Auth | Mark all as read |

---

### 4.11 User Preferences Service — `/api/preferences`

> Backed by MongoDB. Deferred from v1 scope.

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/preferences` | Auth | Get current user's preferences |
| PATCH | `/api/preferences` | Auth | Update preferences (partial) |

---

## 5. Core Data Models

### PostgreSQL Entities

| Entity | Table | Key Relationships |
|---|---|---|
| User | `users` | — |
| Workspace | `workspaces` | `owner_id → users` |
| WorkspaceMember | `workspace_members` | `workspace_id → workspaces`, `user_id → users` |
| WorkspaceInvitation | `workspace_invitations` | `workspace_id → workspaces`, `invited_by → users` |
| Team | `teams` | `workspace_id → workspaces`, `admin_id → users`, `created_by → users` |
| TeamMember | `team_members` | Composite PK `(team_id, user_id)` |
| TeamInvitation | `invitations` | `team_id → teams`, `invited_by → users` |
| BoardStatus | `board_statuses` | `team_id → teams`; 3 rows seeded on team creation |
| TaskItem | `tasks` | `team_id → teams`, `status_id → board_statuses`; soft-delete via `deleted_at` |
| Comment | `comments` | `task_id → tasks`, `author_id → users` |

### Redis Keys

| Key Pattern | Value | TTL |
|---|---|---|
| `refresh_token:{token}` | `{ userId, email, createdAt, deviceInfo }` | 7 days |

### Core Enums (stored as strings in DB)

| Enum | Values |
|---|---|
| Priority | `High`, `Medium`, `Low` |
| LabelType | `Feature`, `Bug`, `Design`, `Docs`, `Infra`, `Refactor` |
| TeamRole | `Admin`, `PM`, `TL`, `Developer` |
| InvitationStatus | `Pending`, `Accepted`, `Declined`, `Expired` |
| WorkspaceMemberStatus | `Active`, `Pending` |

### v1 Scope Exclusions

| Model | Reason |
|---|---|
| `Sprint` | No sprint concept in v1 |
| `Project` | No project concept in v1 |
| `ActivityLog` | MongoDB — deferred |
| `Notification` | MongoDB — deferred |
| `UserPreferences` | MongoDB — deferred |

---

## 6. Frontend → Endpoint Traceability

### Shell (`shell/`)

| Frontend Element | Endpoint |
|---|---|
| WelcomeScreen — 4 stat cards | `GET /api/dashboard/stats` |
| TeamsScreen — 3 stat cards | `GET /api/teams/stats` |
| TeamsScreen — team list | `GET /api/teams` |
| `/teams/new` — Create Team submit | `POST /api/teams` |
| `/teams/:id` — Edit name / desc / color | `PATCH /api/teams/:id` |
| `/teams/:id` — Add from workspace | `POST /api/teams/:id/members` |
| `/teams/:id` — Change member role | `PATCH /api/teams/:id/members/:userId` |
| `/teams/:id` — Remove member | `DELETE /api/teams/:id/members/:userId` |
| `/teams/:id` — Delete team | `DELETE /api/teams/:id` |
| TeamCard — Invite button | `POST /api/teams/:id/invite` |
| PeopleScreen — 4 stat cards | `GET /api/people/stats` |
| PeopleScreen — member list | `GET /api/people` |
| PeopleScreen — search filter | `GET /api/people?search=...` |
| PeopleScreen — team filter | `GET /api/people?teamId=...` |
| PeopleScreen — status filter | `GET /api/people?status=active\|pending` |
| PeopleScreen — Invite to workspace | `POST /api/people/invite` |
| PeopleScreen — Resend (pending member) | `POST /api/people/invite` → 200, resets expiry |
| PeopleScreen — Remove (active member) | `DELETE /api/people/:userId` |
| PeopleScreen — Remove (pending / cancel invite) | `DELETE /api/people/:invitationId` |
| SettingsScreen — Profile save | `PATCH /api/users/:id` |
| SettingsScreen — Notification toggles | `PATCH /api/preferences` |
| LoginForm — submit | `POST /api/auth/login` |
| SignupForm — submit | `POST /api/auth/signup` |
| Token expiry → silent refresh | `PATCH /api/auth/refresh` |

### Task MFE (`mfe-task/`)

| Frontend Element | Endpoint |
|---|---|
| Task list | `GET /api/tasks` |
| Stats row | `GET /api/tasks/stats` |
| Status filter | `GET /api/tasks?statusId=...` |
| Team filter | `GET /api/tasks?teamId=...` |
| Task row checkbox (mark done) | `PATCH /api/tasks/:id` |
| TaskFormScreen — Team dropdown | `GET /api/teams` |
| TaskFormScreen — Status dropdown | `GET /api/board/:teamId/statuses` |
| TaskFormScreen — submit | `POST /api/tasks` |
| TaskDetailScreen | `GET /api/tasks/:id` |
| TaskDetailScreen — activity | `GET /api/activity/tasks/:taskId` |

### Board MFE (`mfe-board/`)

| Frontend Element | Endpoint |
|---|---|
| Teams list | `GET /api/teams` |
| Kanban columns + first 5 tasks | `GET /api/board/:teamId` |
| Column "Load more" | `GET /api/board/:teamId/status/:statusId/tasks?page&limit` |
| Add Status modal | `POST /api/board/:teamId/statuses` |
| Edit status | `PATCH /api/board/:teamId/statuses/:statusId` |
| Delete status | `DELETE /api/board/:teamId/statuses/:statusId` |
| Drag task to column | `PATCH /api/tasks/:id/status` |

---

## 7. Cross-Cutting Concerns

### Pagination

All list endpoints support `?page=1&limit=20` query params. Default: `page=1, limit=20`.

`PagedResult<T>` is embedded as `result` in `ApiResponse<T>`:

```json
{
  "data": [],
  "count": 20,
  "total": 142,
  "page": 1,
  "limit": 20,
  "totalPages": 8
}
```

| Field | Description |
|---|---|
| `data` | Items on this page |
| `count` | `data.length` — items in this page |
| `total` | Total records across all pages |
| `page` | Current page number |
| `limit` | Items requested per page |
| `totalPages` | `ceil(total / limit)` |

### Soft Delete

TaskItems use `deletedAt TIMESTAMPTZ`. All read queries filter `WHERE deletedAt IS NULL`. Triggered by:
- `DELETE /api/tasks/:id`
- `DELETE /api/board/:teamId/statuses/:statusId` (soft-deletes all tasks in the column)

### Image Uploads

Upload happens **client → Cloudinary** (signed upload). The backend never proxies images. Returned secure URLs are sent in `imageUrls[]` (tasks) or `imageUrls[]` (comments). `publicId` stored alongside URL for deletion support.

### Role-Based Access Control

| Rule | Detail |
|---|---|
| Board status create | `admin` or `pm` on the team |
| Board status edit/delete | `admin`, `pm`, or `tl` |
| Task drag-drop | `developer` can only move tasks assigned to themselves |
| Team edit / delete | `admin` on the team |
| Team always-has-admin | Demoting or removing the only `admin` returns `422` |
| Status always-exists | Deleting the last status column returns `422` |

### Custom Exceptions → HTTP Codes

`ExceptionMiddleware` maps these automatically — throw from services, never from repositories:

| Exception | HTTP Code |
|---|---|
| `NotFoundException` | 404 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `ValidationException` | 422 |
| Unhandled `Exception` | 500 |

### DI Registration Convention

Always register `IFoo, FooService` — never the concrete class alone:
```csharp
builder.Services.AddScoped<IFooRepository, FooRepository>();
builder.Services.AddScoped<IFooService, FooService>();
```
