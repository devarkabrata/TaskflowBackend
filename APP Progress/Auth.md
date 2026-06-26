# Auth Endpoints

Base URL: `http://localhost:5000/api/auth`

---

### POST /signup

**Description:** Register a new user account.
**Auth:** None

**Headers:**
| Key | Value |
|-----|-------|
| Content-Type | application/json |

**Request Body:**
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| name | string | Yes | 2–200 characters |
| email | string | Yes | Valid email, max 255 characters |
| password | string | Yes | Min 8 characters, max 100 |
| confirmPassword | string | Yes | Must match `password` |
| title | string | No | Max 200 characters |

**Example Request:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "secret123",
  "confirmPassword": "secret123",
  "title": "Software Engineer"
}
```

**Success Response — 200:**
```json
{
  "status": true,
  "code": 201,
  "result": {
    "id": "uuid",
    "name": "John Doe",
    "title": "Software Engineer",
    "email": "john@example.com",
    "avatarInitials": "JD"
  },
  "message": "User Signed up Successfully",
  "errors": [],
  "devMessage": "",
  "requestId": "",
  "timestamp": ""
}
```

**Error Responses:**
| Code | Trigger |
|------|---------|
| 422 | Validation failed (missing fields, password mismatch, invalid email) |
| 409 | Email already exists |

---

### POST /login

**Description:** Authenticate a user and receive a JWT access token + refresh token.
**Auth:** None

**Headers:**
| Key | Value |
|-----|-------|
| Content-Type | application/json |

**Request Body:**
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| email | string | Yes | Valid email |
| password | string | Yes | Required |

**Example Request:**
```json
{
  "email": "john@example.com",
  "password": "secret123"
}
```

**Success Response — 200:**
```json
{
  "status": true,
  "code": 200,
  "result": {
    "token": "<jwt_access_token>",
    "refreshToken": "<refresh_token>",
    "user": {
      "id": "uuid",
      "name": "John Doe",
      "email": "john@example.com",
      "title": "Software Engineer",
      "avatarInitials": "JD",
      "avatarUrl": null
    }
  },
  "message": "Login successful.",
  "errors": [],
  "devMessage": "",
  "requestId": "",
  "timestamp": ""
}
```

**Error Responses:**
| Code | Trigger |
|------|---------|
| 422 | Validation failed |
| 401 | Invalid credentials |

---

### PATCH /refresh

**Description:** Exchange a valid refresh token for a new JWT access token.
**Auth:** None

**Headers:**
| Key | Value |
|-----|-------|
| Content-Type | application/json |

**Request Body:**
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| refreshToken | string | Yes | Required |

**Example Request:**
```json
{
  "refreshToken": "<refresh_token>"
}
```

**Success Response — 200:**
```json
{
  "status": true,
  "code": 200,
  "result": {
    "token": "<new_jwt_access_token>",
    "refreshToken": "<same_refresh_token>",
    "user": null
  },
  "message": "Token refreshed successfully.",
  "errors": [],
  "devMessage": "",
  "requestId": "",
  "timestamp": ""
}
```

**Error Responses:**
| Code | Trigger |
|------|---------|
| 422 | Validation failed (missing refreshToken) |
| 401 | Refresh token expired or not found in Redis |
