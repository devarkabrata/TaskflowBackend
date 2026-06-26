# Workspace / People API — Progress

Route prefix: `/api/people`  
Auth: All endpoints require `Authorization: Bearer <jwt>`

---

## GET /api/people

Fetch the workspace people list (active members + pending invitations combined).

### Query Parameters

| Parameter | Type   | Required | Description                                |
|-----------|--------|----------|--------------------------------------------|
| search    | string | No       | Filter by name or email                    |
| status    | string | No       | `active` or `pending` (default: both)      |
| teamId    | guid   | No       | Filter active members by team              |
| page      | int    | No       | Page number (default: 1)                   |
| limit     | int    | No       | Items per page (default: 20)               |

### Success Response (200)

```json
{
  "status": true,
  "code": 200,
  "result": [
    {
      "id": "uuid",
      "name": "Alice Smith",
      "email": "alice@example.com",
      "title": "Frontend Developer",
      "avatarInitials": "AS",
      "avatarUrl": "https://...",
      "teamIds": ["uuid1", "uuid2"],
      "status": "active"
    },
    {
      "id": "uuid",
      "name": "",
      "email": "bob@example.com",
      "title": "",
      "avatarInitials": "",
      "avatarUrl": null,
      "teamIds": [],
      "status": "pending"
    }
  ],
  "message": "People fetched successfully.",
  "errors": [],
  "devMessage": "",
  "requestId": "trace-id",
  "timestamp": "2026-06-26T00:00:00Z"
}
```

### Error Responses

| Code | Trigger                          |
|------|----------------------------------|
| 401  | Missing or invalid Bearer token  |
| 404  | No workspace found for this user |

---

## GET /api/people/stats

Returns workspace member statistics.

### Success Response (200)

```json
{
  "status": true,
  "code": 200,
  "result": {
    "totalMembers": 12,
    "active": 10,
    "pendingInvites": 2,
    "totalTeams": 3
  },
  "message": "Stats fetched successfully.",
  "errors": [],
  "devMessage": "",
  "requestId": "trace-id",
  "timestamp": "2026-06-26T00:00:00Z"
}
```

### Error Responses

| Code | Trigger                          |
|------|----------------------------------|
| 401  | Missing or invalid Bearer token  |
| 404  | No workspace found for this user |

---

## POST /api/people/invite

Invite a user to the workspace by email. If they already have a pending invite, resets the expiry and returns 200. If they are already an active member, returns 409.

### Request Body

```json
{
  "email": "newmember@example.com"
}
```

| Field | Type   | Required | Validation          |
|-------|--------|----------|---------------------|
| email | string | Yes      | Valid email address  |

### Success Response — New Invite (201)

```json
{
  "status": true,
  "code": 201,
  "result": {
    "id": "uuid",
    "email": "newmember@example.com",
    "status": "pending",
    "expiresAt": "2026-07-03T00:00:00Z"
  },
  "message": "Invitation sent successfully.",
  "errors": [],
  "devMessage": "",
  "requestId": "trace-id",
  "timestamp": "2026-06-26T00:00:00Z"
}
```

### Success Response — Resend (200)

Same shape as above with `"message": "Invitation resent successfully."` and `code: 200`.

### Error Responses

| Code | Trigger                                        |
|------|------------------------------------------------|
| 401  | Missing or invalid Bearer token                |
| 404  | No workspace found for this user               |
| 409  | Email belongs to an existing active member     |
| 422  | Email field fails validation                   |

---

## PATCH /api/people/{userId}

Update a workspace member's profile (currently: title only).

### URL Parameters

| Parameter | Type | Required | Description              |
|-----------|------|----------|--------------------------|
| userId    | guid | Yes      | The target member's UUID |

### Request Body

```json
{
  "title": "Senior Developer"
}
```

| Field | Type   | Required | Validation                      |
|-------|--------|----------|---------------------------------|
| title | string | No       | Max 200 characters               |

### Success Response (200)

```json
{
  "status": true,
  "code": 200,
  "result": {
    "id": "uuid",
    "name": "Alice Smith",
    "email": "alice@example.com",
    "title": "Senior Developer",
    "avatarInitials": "AS",
    "avatarUrl": "https://...",
    "teamIds": ["uuid1"],
    "status": "active"
  },
  "message": "Member updated successfully.",
  "errors": [],
  "devMessage": "",
  "requestId": "trace-id",
  "timestamp": "2026-06-26T00:00:00Z"
}
```

### Error Responses

| Code | Trigger                                |
|------|----------------------------------------|
| 401  | Missing or invalid Bearer token        |
| 404  | No workspace found / member not found  |
| 422  | Title exceeds 200 characters           |

---

## DELETE /api/people/{userId}

Remove a member from the workspace. Also removes them from all teams within the workspace.

### URL Parameters

| Parameter | Type | Required | Description              |
|-----------|------|----------|--------------------------|
| userId    | guid | Yes      | The target member's UUID |

### Success Response (200)

```json
{
  "status": true,
  "code": 200,
  "result": null,
  "message": "Member removed successfully.",
  "errors": [],
  "devMessage": "",
  "requestId": "trace-id",
  "timestamp": "2026-06-26T00:00:00Z"
}
```

### Error Responses

| Code | Trigger                                |
|------|----------------------------------------|
| 401  | Missing or invalid Bearer token        |
| 404  | No workspace found / member not found  |
