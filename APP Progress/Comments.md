# Comments API — Progress

Route prefix: `/api/comments`
Auth: All endpoints require `Authorization: Bearer <jwt>`

Requester must be a member of the task's team for every endpoint below. Update/delete are additionally restricted to the comment's own author.

---

## GET /api/comments?taskId={taskId}

List comments on a task, oldest first.

### Success Response (200)

```json
{
  "status": true,
  "code": 200,
  "result": [
    {
      "id": "uuid",
      "taskId": "uuid",
      "author": {
        "id": "uuid",
        "name": "Alice",
        "email": "alice@example.com",
        "avatarUrl": null
      },
      "body": "This looks good, ready for review.",
      "createdAt": "2026-07-17T00:00:00Z",
      "updatedAt": "2026-07-17T00:00:00Z"
    }
  ],
  "message": "Comments fetched successfully."
}
```

### Errors

- `404` — Task not found
- `403` — Not a member of the task's team

---

## POST /api/comments?taskId={taskId}

Add a comment to a task.

### Request Body

```json
{
  "body": "This looks good, ready for review."
}
```

### Success Response (201)

Returns `CommentResponseDto`.

### Errors

- `404` — Task not found
- `403` — Not a member of the task's team
- `422` — Body is required

---

## PUT /api/comments/:id

Edit a comment's body. Author only.

### Request Body

```json
{
  "body": "This looks good, ready for review (edited)."
}
```

### Success Response (200)

Returns `CommentResponseDto`.

### Errors

- `404` — Comment not found
- `403` — Not a member of the task's team, or not the comment's author
- `422` — Body is required

---

## DELETE /api/comments/:id

Delete a comment. Author only.

### Success Response (204)

### Errors

- `404` — Comment not found
- `403` — Not a member of the task's team, or not the comment's author
