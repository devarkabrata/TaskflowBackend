# Taskflow Backend — API Flow Reference

> Synthesized from `APIRequirements/`. This is the backend team's single navigable view of the full API contract before any implementation begins.

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
| Primary DB | PostgreSQL — structured, relational, transactional data |
| Secondary DB | MongoDB Atlas — flexible, document-oriented, append-only data |
| Auth | Cookie-based sessions (no Authorization header) |
| Image Storage | Cloudinary (client-side signed upload; URLs stored in DB) |
| API Style | REST, JSON, standard response envelope on every endpoint |

### Service Map

11 service groups, all rooted at `/api/*`:

| # | Service | Base Path | Database | Auth Required |
|---|---|---|---|---|
| 1 | Auth | `/api/auth` | PostgreSQL | Mixed (Public + Auth) |
| 2 | Task | `/api/tasks` | PostgreSQL | Auth |
| 3 | Board | `/api/board` | PostgreSQL | Auth |
| 4 | People / Workspace | `/api/people` | PostgreSQL | Auth |
| 5 | Team | `/api/teams` | PostgreSQL | Auth |
| 6 | Dashboard | `/api/dashboard` | PostgreSQL (computed) | Auth |
| 7 | User | `/api/users` | PostgreSQL | Auth |
| 8 | Project | `/api/projects` | PostgreSQL | Auth |
| 9 | Activity | `/api/activity` | MongoDB | Auth |
| 10 | Notification | `/api/notifications` | MongoDB | Auth |
| 11 | User Preferences | `/api/preferences` | MongoDB | Auth |

### Database Assignment Rule

```
New service — ask:
  ├─ Has foreign keys to users or tasks?          → PostgreSQL
  ├─ Needs a transaction spanning multiple tables? → PostgreSQL
  ├─ Append-only event stream?                    → MongoDB
  └─ Per-user settings blob (variable shape)?     → MongoDB
```

---

## 2. Authentication & Session

### Mechanism

Cookie-based session. No JWT in `Authorization` headers. The Shell sets cookies on the shared domain; all MFE zones receive them automatically on every request.

### Cookie Spec

Four cookies set on every successful `login` or `signup`:

| Cookie | HttpOnly | JS-readable | Purpose |
|---|---|---|---|
| `taskflow_session` | Yes | No | Session token — auth gating across all zones |
| `taskflow_name` | No | Yes | Display name — Sidebar workspace indicator + user card |
| `taskflow_email` | No | Yes | Email — Sidebar user card |
| `taskflow_title` | No | Yes | Designation — People listing, Settings profile |

All cookies: `Path=/; SameSite=Lax; Secure=true (production); Domain=.taskflow.app`
`taskflow_session` Max-Age: `604800` (7 days, sliding).

### Login Flow

```
Browser  →  POST /api/auth/login  { email, password }
         ←  200 + Set-Cookie: taskflow_session=...
                    taskflow_name=...
                    taskflow_email=...
                    taskflow_title=...
         ←  { user: { id, name, email, title, avatarInitials } }

All subsequent requests carry cookies automatically.
```

### Logout Flow

```
Browser  →  POST /api/auth/logout
         ←  200 + Set-Cookie: taskflow_session=; Max-Age=0  (clears all 4 cookies)
```

### Internal Session Verification (Microservice Pattern)

Each downstream service validates the cookie using one of two patterns:

```
Option A — network call:
  Task Service  →  GET /api/auth/verify  (internal, not browser-exposed)
                   Forwards Cookie header
                ←  200 { userId, email }  |  401

Option B — shared JWT secret:
  Each service verifies the session token locally — no network hop.
```

---

## 3. Standard Response Envelope

Every endpoint — success or failure — returns the same JSON wrapper.

### Success (single object)

```json
{
  "status": true,
  "code": 200,
  "result": { "id": "uuid", "...": "..." },
  "message": "Task updated successfully.",
  "errors": [],
  "dev_message": "",
  "requestId": "req_01J3K9X2M4N5P6Q7R8S9T0",
  "timestamp": "2026-06-11T10:00:00.000Z"
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
  "dev_message": "",
  "requestId": "req_01J3K9X2M4N5P6Q7R8S9T0",
  "timestamp": "2026-06-11T10:00:00.000Z"
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
    { "field": "email",   "code": "INVALID_FORMAT", "message": "Enter a valid email address." },
    { "field": "dueDate", "code": "REQUIRED",        "message": "Due date is required."        }
  ],
  "dev_message": "ValidationError thrown at TaskService.create() — only outside production",
  "requestId": "req_01J3K9X2M4N5P6Q7R8S9T0",
  "timestamp": "2026-06-11T10:00:00.000Z"
}
```

### Field Reference

| Field | Type | Notes |
|---|---|---|
| `status` | `boolean` | `true` = success, `false` = any error |
| `code` | `number` | Mirrors the HTTP status code |
| `result` | `object \| null` | The payload. `null` on any failure — never absent |
| `message` | `string` | User-displayable string. `""` if nothing to show |
| `errors` | `array` | `[]` on success. Each item: `{ field?, code, message }` |
| `dev_message` | `string` | Stack trace / internal detail. Always `""` in production |
| `requestId` | `string` | Unique request ID — use to correlate logs across services |
| `timestamp` | `string` | ISO 8601 UTC — when the response was generated |

### Standard HTTP Error Codes

| Code | When |
|---|---|
| `400` | Malformed request body / bad JSON |
| `401` | No session cookie or expired session |
| `403` | Valid session but insufficient permissions |
| `404` | Resource not found |
| `409` | Conflict — duplicate invite, duplicate email |
| `422` | Validation failed — `errors[]` populated |
| `429` | Rate limit exceeded |
| `500` | Unexpected server error |
| `502` | Upstream service unreachable (API gateway) |

---

## 4. Service Catalogue

### 4.1 Auth Service — `/api/auth`

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/signup` | Public | Register → sets session cookies |
| POST | `/api/auth/login` | Public | Email + password → sets session cookies |
| POST | `/api/auth/logout` | Auth | Clears all session cookies |
| GET | `/api/auth/me` | Auth | Returns current user with workspace + team memberships |

#### `POST /api/auth/signup`
**Request:** `{ name, email, password, title? }`
`title` = resolved designation (if user picked "Other", send the free-text value here).
**Response `201`:** `{ ok: true, user: { id, name, email, title, avatarInitials } }` + sets 4 cookies.
**Error `409`:** email already registered.

#### `POST /api/auth/login`
**Request:** `{ email, password }`
**Response `200`:** `{ user: { id, name, email, title, avatarInitials } }` + sets 4 cookies.

#### `GET /api/auth/me`
**Response `200`:**
```json
{
  "id": "uuid",
  "name": "string",
  "email": "string",
  "title": "string",
  "avatarInitials": "AC",
  "avatarUrl": null,
  "workspaces": [{ "workspaceId": "ws_1", "role": "owner", "status": "active", "joinedAt": "..." }],
  "teams": [
    { "teamId": "team_1", "workspaceId": "ws_1", "role": "admin",     "joinedAt": "..." },
    { "teamId": "team_2", "workspaceId": "ws_1", "role": "developer", "joinedAt": "..." }
  ]
}
```
`workspaces[]` and `teams[]` are the authoritative membership arrays — all access control decisions derive from these.

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
| PATCH | `/api/tasks/:id/status` | Auth | Drag-drop: change task status (Board MFE) |

#### Query Params for `GET /api/tasks`

```
statusId=stat_1          — filter by status (only meaningful with teamId)
priority=high|medium|low — filter by priority
teamId=team_1|team_2     — omit for "My Tasks" view
assigneeId=uuid
projectId=uuid
sprintId=uuid
page=1&limit=20
```
Soft-deleted tasks (`deleted_at IS NOT NULL`) are always excluded.

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
  "statusId": "stat_1",
  "label": "feature",
  "assigneeId": "uuid",
  "teamId": "team_1",
  "expectedCompletion": "2026-06-20",
  "progress": 0,
  "imageUrls": ["https://res.cloudinary.com/..."],
  "projectId": "uuid (optional)",
  "sprintId": "uuid (optional)"
}
```
Image upload happens client → Cloudinary (signed upload). Returned secure URLs are passed in `imageUrls`.

#### `PATCH /api/tasks/:id` — Partial update
```json
{ "statusId": "stat_3", "progress": 80 }
```

#### `PATCH /api/tasks/:id/status` — Drag-drop
```json
{ "statusId": "stat_2" }
```
**`403`** if a `developer` attempts to move a task they do not own.

---

### 4.3 Board Service — `/api/board`

Drives the **Board MFE** (`mfe-board`). Statuses are dynamic per team.

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/board/:teamId` | Auth | Team's statuses + first 5 tasks per column |
| GET | `/api/board/:teamId/status/:statusId/tasks` | Auth | Load more tasks for one column |
| POST | `/api/board/:teamId/statuses` | Auth | Create a status column (admin / pm) |
| PATCH | `/api/board/:teamId/statuses/:statusId` | Auth | Edit a status (admin / pm / tl) |
| DELETE | `/api/board/:teamId/statuses/:statusId` | Auth | Delete status; soft-deletes its tasks (admin / pm / tl) |

#### `GET /api/board/:teamId` Response
```json
{
  "statuses": [
    { "id": "stat_1", "name": "Backlog", "description": "Not yet started", "position": 0, "totalTasks": 8, "tasks": [ /* up to 5 */ ] },
    { "id": "stat_2", "name": "In Progress", "description": null, "position": 1, "totalTasks": 3, "tasks": [ /* up to 5 */ ] }
  ]
}
```

#### Load More — `GET /api/board/:teamId/status/:statusId/tasks`
Query: `page=2&limit=10`
Response: paginated envelope (`data`, `count`, `total`, `page`, `limit`, `totalPages`).

#### `POST /api/board/:teamId/statuses`
```json
{ "name": "Code Review", "description": "Awaiting PR approval" }
```
New status `position = max + 1`. **`403`** if caller is not `admin` or `pm`.

#### `DELETE /api/board/:teamId/statuses/:statusId`
Soft-deletes all tasks in the column.
**`422`** if this is the team's last remaining status.
Response: `{ "ok": true, "softDeletedTaskCount": 8 }`

#### Role Restrictions (Board)

| Action | Allowed Roles |
|---|---|
| Create status | admin, pm |
| Edit status | admin, pm, tl |
| Delete status | admin, pm, tl |
| Drag-drop own task | all (developer only their own) |
| Drag-drop any task | admin, pm, tl |

---

### 4.4 People / Workspace Service — `/api/people`

Drives the **Shell** — `PeopleScreen`. Manages workspace-level member directory (distinct from team-scoped `/api/teams/:id/members`).

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/people` | Auth | List all workspace members (active + pending) |
| GET | `/api/people/stats` | Auth | Aggregate counts (total, active, pending, teams) |
| POST | `/api/people/invite` | Auth | Invite someone to the workspace by email |
| PATCH | `/api/people/:userId` | Auth | Update member title / role |
| DELETE | `/api/people/:userId` | Auth | Remove member from workspace (and all teams) |

#### Query Params for `GET /api/people`
```
teamId=team_1          — filter by team membership
status=active|pending  — filter by status
search=string          — name or email substring
page=1&limit=50
```

#### `GET /api/people/stats` Response
```json
{ "totalMembers": 5, "active": 4, "pendingInvites": 1, "totalTeams": 3 }
```

#### `POST /api/people/invite`
**Request:** `{ "email": "colleague@example.com" }`
**Response `201`:** `{ id, email, status: "pending", expiresAt }` — invitation expires in 7 days.
**`409`** if already an active member or has a pending invite.
Re-sending to the same email resets the expiry and returns `200`.

---

### 4.5 Team Service — `/api/teams`

Drives the **Shell** — Teams section.

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

#### `GET /api/teams/stats` Response
```json
{ "totalTeams": 2, "totalMembers": 4, "pendingInvites": 1 }
```

#### `POST /api/teams` Request
```json
{
  "name": "Frontend Team",
  "description": "optional",
  "color": "#6155DD",
  "memberIds": [{ "userId": "u2", "role": "developer" }]
}
```
`color` = required hex from the 8-swatch picker.
Creator is automatically added as `admin` server-side — do not include in `memberIds`.
Three default `BoardStatus` rows are seeded on creation: **Backlog** (pos 1), **In Progress** (pos 2), **Done** (pos 3).

#### `POST /api/teams/:id/invite`
```json
{ "email": "colleague@example.com", "role": "developer", "addToWorkspace": false }
```
`addToWorkspace: true` also creates a `workspace_invitation` — invitee joins both on acceptance.
**`409`** if a pending invite already exists for that email + team.

#### `PATCH /api/teams/:id/members/:userId`
```json
{ "role": "pm" }
```
**`422`** if attempting to demote the only `admin` on the team.

#### `DELETE /api/teams/:id/members/:userId`
Removes from team only — workspace membership untouched. Assignee field on tasks becomes `null`.
**`422`** if attempting to remove the only `admin`.

---

### 4.6 Dashboard Service — `/api/dashboard`

Drives `WelcomeScreen` — the 4-card stats row.

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
`totalTasks` = all tasks where `assigneeId` = current user.
`boardItems` = tasks in the active sprint across all projects.

---

### 4.7 User Service — `/api/users`

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/users` | Auth | List users (used by assignee picker) |
| GET | `/api/users/:id` | Auth | Get user profile |
| PATCH | `/api/users/:id` | Auth | Update own profile |

#### `GET /api/users` Response
```json
{ "data": [{ "id": "uuid", "name": "Alice Chen", "email": "alice@...", "avatarInitials": "AC" }] }
```

---

### 4.8 Project Service — `/api/projects`

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/projects` | Auth | List projects for current user |
| POST | `/api/projects` | Auth | Create project |
| GET | `/api/projects/:id` | Auth | Get project details |
| GET | `/api/projects/:id/sprints` | Auth | List sprints for a project |
| POST | `/api/projects/:id/sprints` | Auth | Create sprint |
| PATCH | `/api/projects/:id/sprints/:sprintId` | Auth | Update sprint (activate, complete) |

> Projects and Sprints are deferred from v1 scope but the endpoints are specified for future implementation.

---

### 4.9 Activity Service — `/api/activity`

Backed by **MongoDB** `activity_logs`. Read-only from the frontend.

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/activity` | Auth | Recent activity feed (current user) |
| GET | `/api/activity/tasks/:taskId` | Auth | Activity timeline for a specific task |

#### `GET /api/activity/tasks/:taskId` Response
```json
{
  "data": [
    {
      "id": "mongo-objectid",
      "action": "status_changed",
      "actor": { "id": "uuid", "name": "Alice Chen", "avatarInitials": "AC" },
      "diff": { "status": { "from": "todo", "to": "in-progress" } },
      "timestamp": "2026-06-11T10:00:00Z"
    }
  ]
}
```

---

### 4.10 Notification Service — `/api/notifications`

Backed by **MongoDB** `notifications`.

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/notifications` | Auth | List unread notifications |
| PATCH | `/api/notifications/:id/read` | Auth | Mark one notification as read |
| PATCH | `/api/notifications/read-all` | Auth | Mark all as read |

---

### 4.11 User Preferences Service — `/api/preferences`

Backed by **MongoDB** `user_preferences`. One document per user.

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/api/preferences` | Auth | Get current user's preferences |
| PATCH | `/api/preferences` | Auth | Update preferences (partial) |

---

## 5. Core Data Models

### PostgreSQL Entities

| Entity | Table | PK Strategy | Key Relationships |
|---|---|---|---|
| User | `users` | UUID | — |
| Team | `teams` | UUID | `owner_id → users` |
| TeamMember | `team_members` | Composite (team_id, user_id) | `team_id → teams`, `user_id → users` |
| BoardStatus | `board_statuses` | UUID | `team_id → teams`; default 3 rows seeded on team creation |
| Task | `tasks` | UUID + `task_number` (int, project-scoped) | `team_id → teams`, `status_id → board_statuses`, `assignee_id → users` (nullable) |
| Label | `labels` | UUID | — (seed data: feature, bug, design, docs, infra, refactor) |
| TaskLabel | `task_labels` | Composite (task_id, label_id) | join table |
| Comment | `comments` | UUID | `task_id → tasks`, `author_id → users` |
| Project | `projects` | UUID | `owner_id → users` |
| ProjectMember | `project_members` | Composite (project_id, user_id) | — |
| Sprint | `sprints` | UUID | `project_id → projects` |
| WorkspaceInvitation | `workspace_invitations` | UUID | `invited_by → users`; UNIQUE on email |
| TeamInvitation | `invitations` | UUID | `team_id → teams`, `invited_by → users`; UNIQUE on (team_id, email) |

### MongoDB Collections

| Collection | Purpose | Indexes |
|---|---|---|
| `activity_logs` | Append-only event timeline per entity | `{ entityId, timestamp }`, `{ actorId, timestamp }` |
| `notifications` | Per-user notification inbox | `{ recipientId, read, createdAt }` |
| `user_preferences` | Per-user settings blob (`_id` = PostgreSQL `users.id`) | `_id` only |
| `audit_trail` | Compliance log of all mutating API calls; 90-day TTL | `{ userId, timestamp }`, `{ service, timestamp }`, TTL on `timestamp` |

### Core Enums

| Enum | Values |
|---|---|
| Priority | `high`, `medium`, `low` |
| LabelType | `feature`, `bug`, `design`, `docs`, `infra`, `refactor` |
| TeamRole | `admin`, `pm`, `tl`, `developer` |
| InvitationStatus | `pending`, `accepted`, `declined`, `expired` |
| WorkspaceMemberStatus | `active`, `pending` |

### v1 Scope Exclusions

The following are intentionally deferred:

| Model | Reason |
|---|---|
| `Sprint` | No sprint concept in v1 |
| `Project` | No project concept in v1 |
| `ActivityLog` | MongoDB — deferred |
| `Notification` | MongoDB — deferred |
| `UserPreferences` | MongoDB — deferred |
| `DashboardStats` | Computed at query time, not persisted |
| `Session` | Cookie-based, managed server-side |

---

## 6. Frontend → Endpoint Traceability

### Shell (`shell/`)

| Frontend Element | Endpoint |
|---|---|
| WelcomeScreen — 4 stat cards (Total Tasks, In Progress, Completed, Board Items) | `GET /api/dashboard/stats` |
| WelcomeScreen — App showcase cards | Client navigation only |
| TeamsScreen — 3 stat cards | `GET /api/teams/stats` |
| TeamsScreen — team list | `GET /api/teams` |
| `/teams/new` — Create Team submit | `POST /api/teams` (includes `color` + `memberIds[]`) |
| `/teams/:id` — Edit name / desc / color | `PATCH /api/teams/:id` |
| `/teams/:id` — Add from workspace | `POST /api/teams/:id/members` |
| `/teams/:id` — Change member role | `PATCH /api/teams/:id/members/:userId` |
| `/teams/:id` — Remove member | `DELETE /api/teams/:id/members/:userId` |
| `/teams/:id` — Delete team | `DELETE /api/teams/:id` |
| TeamCard — Invite button (TeamInviteModal) | `POST /api/teams/:id/invite` |
| PeopleScreen — 4 stat cards | `GET /api/people/stats` |
| PeopleScreen — member list | `GET /api/people` |
| PeopleScreen — search filter | `GET /api/people?search=...` |
| PeopleScreen — team filter | `GET /api/people?teamId=...` |
| PeopleScreen — status filter | `GET /api/people?status=active\|pending` |
| PeopleScreen — Invite to workspace | `POST /api/people/invite` |
| PeopleScreen — Resend (pending member) | `POST /api/people/invite` (re-send → 200, resets expiry) |
| PeopleScreen — Remove (active or pending) | `DELETE /api/people/:userId` |
| SettingsScreen — Profile read | `GET /api/auth/me` |
| SettingsScreen — Profile save | `PATCH /api/users/:id` |
| SettingsScreen — Notification toggles | `PATCH /api/preferences` |
| Sidebar — user card | `GET /api/auth/me` |
| Sidebar — workspace indicator | Derived from `taskflow_name` cookie — no API call |
| Topbar — avatar | `GET /api/auth/me` |
| Topbar — notification bell | Not yet wired |
| LoginForm — submit | `POST /api/auth/login` |
| SignupForm — submit | `POST /api/auth/signup` |

### Task MFE (`mfe-task/`)

| Frontend Element | Endpoint |
|---|---|
| Task list (My Tasks view) | `GET /api/tasks` |
| Stats row (Total / In Progress / In Review / Done) | `GET /api/tasks/stats` |
| Status filter tabs | `GET /api/tasks?statusId=...` |
| Team filter bar | `GET /api/tasks?teamId=...` |
| Task row checkbox (mark done) | `PATCH /api/tasks/:id` `{ statusId: <done-status-id> }` |
| TaskFormScreen — Team dropdown | `GET /api/teams` |
| TaskFormScreen — Status dropdown (per team) | `GET /api/board/:teamId/statuses` |
| TaskFormScreen — submit | `POST /api/tasks` |
| TaskDetailScreen — task data | `GET /api/tasks/:id` |
| TaskDetailScreen — activity timeline | `GET /api/activity/tasks/:taskId` |
| Sidebar — user card | `GET /api/auth/me` |
| Topbar — notification bell | `GET /api/notifications` |

### Board MFE (`mfe-board/`)

| Frontend Element | Endpoint |
|---|---|
| Teams list landing at `/board` | `GET /api/teams` |
| Topbar team-switcher dropdown | `GET /api/teams` |
| Kanban columns + first 5 tasks | `GET /api/board/:teamId` |
| Column "Load more" | `GET /api/board/:teamId/status/:statusId/tasks?page&limit` |
| "+ Add Status" modal submit | `POST /api/board/:teamId/statuses` |
| Edit status (✎) modal submit | `PATCH /api/board/:teamId/statuses/:statusId` |
| Delete status (🗑) | `DELETE /api/board/:teamId/statuses/:statusId` |
| Drag task to another column | `PATCH /api/tasks/:id/status` |
| "+ Add Task" per column | Navigates to `/tasks/new?teamId=&statusId=` → `POST /api/tasks` |
| Sidebar — user card | `GET /api/auth/me` |
| Topbar — notification bell | `GET /api/notifications` |

---

## 7. Cross-Cutting Concerns

### Pagination

All list endpoints support `?page=1&limit=20` query params. Default: `page=1, limit=20`.
Paginated responses always include: `data`, `count`, `total`, `page`, `limit`, `totalPages`.

### Soft Delete

Tasks use `deleted_at TIMESTAMPTZ` for soft deletion. All read queries must filter `WHERE deleted_at IS NULL` unless explicitly recovering deleted tasks. Soft deletion is triggered:
- `DELETE /api/tasks/:id` — explicit user action
- `DELETE /api/board/:teamId/statuses/:statusId` — when a status column is deleted

### Image Uploads

Upload happens **client → Cloudinary** (signed upload). The backend never proxies images. Returned secure URLs are sent to the API in `imageUrls[]` (tasks) or `image_urls[]` (comments). Public IDs are stored alongside URLs for deletion support.

### Role-Based Access Control

| Rule | Detail |
|---|---|
| Board status create | `admin` or `pm` on the team |
| Board status edit/delete | `admin`, `pm`, or `tl` |
| Task drag-drop | `developer` can only move tasks assigned to themselves |
| Team edit / delete | `admin` on the team |
| Team always-has-admin | Demoting or removing the last `admin` returns `422` |
| Status always-exists | Deleting the last status column returns `422` |

### Task Identifier Format

Tasks use a human-readable ID like `TF-001`. This is constructed from:
- `slug` on the `projects` table (e.g., `taskflow` → prefix `TF`)
- `task_number` — a project-scoped sequential integer stored on the `tasks` table

### Invitation Expiry

Both workspace (`workspace_invitations`) and team (`invitations`) invitations expire after **7 days**. Expired invites can be re-sent — the endpoint resets `expires_at` and returns `200`.
