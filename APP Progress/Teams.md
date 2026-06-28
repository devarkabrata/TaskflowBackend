# Teams API — Progress

Route prefix: `/api/teams`  
Auth: All endpoints require `Authorization: Bearer <jwt>`

---

## GET /api/teams

List all teams the authenticated user belongs to (within their workspace).

### Success Response (200)

```json
{
  "status": true,
  "code": 200,
  "result": [
    {
      "id": "uuid",
      "name": "Frontend Team",
      "description": "Handles all UI work",
      "color": "#6155DD",
      "workspaceId": "uuid",
      "adminId": "uuid",
      "pendingInvites": 1,
      "members": [
        { "userId": "uuid", "name": "Alice", "avatarInitials": "AL", "role": "Admin" }
      ],
      "createdAt": "2026-06-28T00:00:00Z",
      "updatedAt": "2026-06-28T00:00:00Z"
    }
  ],
  "message": "Teams fetched successfully."
}
```

---

## POST /api/teams

Create a new team. Creator is automatically added as Admin.

### Request Body

```json
{
  "name": "Frontend Team",
  "description": "Handles all UI work",
  "color": "#6155DD",
  "memberIds": [
    { "userId": "uuid", "role": "Developer" }
  ]
}
```

- `memberIds` is optional — initial workspace members to add at creation
- Creator is always added as Admin regardless of `memberIds`

### Success Response (201)

Returns `TeamResponseDto`.

---

## GET /api/teams/stats

Get aggregate stats for all teams the user belongs to.

### Success Response (200)

```json
{
  "status": true,
  "code": 200,
  "result": {
    "totalTeams": 3,
    "totalMembers": 12,
    "pendingInvites": 2
  },
  "message": "Stats fetched successfully."
}
```

---

## GET /api/teams/:id

Get a single team's details. Requester must be a team member.

### Success Response (200)

Returns `TeamResponseDto`.

### Errors

- `403` — Not a team member

---

## PUT /api/teams/:id

Update team details and/or member list. Admin only.

All fields are optional — only provided fields are applied.  
`members` null = leave members untouched. `members` array = full sync (diff applied atomically).

### Request Body

```json
{
  "name": "New Name",
  "description": "Updated description",
  "color": "#FF5733",
  "members": [
    { "userId": "admin-uuid", "role": "Admin" },
    { "userId": "another-uuid", "role": "Developer" }
  ]
}
```

**Member sync rules:**
- Admin must always be present in `members` array (cannot be removed via sync)
- Users must be workspace members
- Removed users are deleted from `team_members`, new users are inserted, role changes are applied — all in one atomic `SaveChanges`

### Success Response (200)

Returns `TeamResponseDto` with updated state.

### Errors

- `403` — Not the team admin
- `422` — Admin not in members array (`CANNOT_REMOVE_ADMIN`)
- `422` — User not a workspace member (`NOT_WORKSPACE_MEMBER`)

---

## DELETE /api/teams/:id

Delete a team and all its members/invitations. Admin only.

### Success Response (204)

---

## POST /api/teams/:id/invite

Send a team invitation by email.

### Request Body

```json
{
  "email": "user@example.com",
  "role": "Developer",
  "addToWorkspace": false
}
```

### Success Response (201)

```json
{
  "status": true,
  "code": 201,
  "result": {
    "id": "uuid",
    "teamId": "uuid",
    "email": "user@example.com",
    "role": "Developer",
    "status": "pending",
    "expiresAt": "2026-07-05T00:00:00Z",
    "createdAt": "2026-06-28T00:00:00Z"
  },
  "message": "Invitation sent successfully."
}
```

### Errors

- `422` — Pending invite already exists for this email (`INVITE_ALREADY_PENDING`)

---

## DELETE /api/teams/:id/members/:userId

Remove a specific member from the team. Admin only. Admin cannot remove themselves.

### Success Response (204)

### Errors

- `403` — Not the team admin
- `422` — Cannot remove admin (`CANNOT_REMOVE_ADMIN`)
- `404` — Member not found in team
