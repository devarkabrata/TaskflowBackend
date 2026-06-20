# TaskFlow — Django Service Handoff

This document is for the team building the **Task/Board Service** in Django. It covers everything you need: what you own, what .NET owns, how to set up, how services communicate, and the contracts both sides must follow.

---

## Architecture Overview

TaskFlow is split into two independent backend services, each with its own PostgreSQL database hosted on Supabase.

```
Frontend (React MFEs)
        │
        ├── /api/auth/*       →  .NET Shell Service  (Supabase Project 1)
        ├── /api/users/*      →  .NET Shell Service
        ├── /api/people/*     →  .NET Shell Service
        ├── /api/teams/*      →  .NET Shell Service
        │
        ├── /api/tasks/*      →  Django Task/Board Service  (Supabase Project 2)
        ├── /api/board/*      →  Django Task/Board Service
        ├── /api/dashboard/*  →  Django Task/Board Service
        ├── /api/activity/*   →  Django Task/Board Service  (MongoDB)
        ├── /api/notifications/* → Django Task/Board Service (MongoDB)
        └── /api/preferences/*   → Django Task/Board Service (MongoDB)
```

**Rule:** Each service owns its tables exclusively. If you need data from the other service's tables, make an HTTP API call — never share a database connection.

---

## What Django Owns

### PostgreSQL Tables (Supabase Project 2)

#### `board_statuses`
```sql
CREATE TABLE board_statuses (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  team_id     UUID NOT NULL,                -- plain UUID, references .NET teams table
  name        TEXT NOT NULL,
  description TEXT,
  position    INTEGER NOT NULL DEFAULT 0,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE (team_id, name)
);
```
> `team_id` has no DB-level foreign key — it references a team in the .NET database. Validate existence via API call when creating.

#### `tasks`
```sql
CREATE TABLE tasks (
  id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  task_number         INTEGER NOT NULL,
  team_id             UUID NOT NULL,               -- plain UUID, references .NET teams
  status_id           UUID NOT NULL REFERENCES board_statuses(id) ON DELETE RESTRICT,
  assignee_ids        UUID[] NOT NULL DEFAULT '{}', -- array of user UUIDs from .NET
  created_by          UUID NOT NULL,               -- plain UUID, references .NET users
  title               TEXT NOT NULL,
  description         TEXT,
  priority            TEXT NOT NULL DEFAULT 'medium', -- 'high' | 'medium' | 'low'
  label               TEXT,                        -- 'feature'|'bug'|'design'|'docs'|'infra'|'refactor'
  expected_completion DATE,
  progress            SMALLINT NOT NULL DEFAULT 0,  -- 0–100
  image_urls          TEXT[] NOT NULL DEFAULT '{}',
  deleted_at          TIMESTAMPTZ,                 -- soft delete
  created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

#### `comments`
```sql
CREATE TABLE comments (
  id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  task_id          UUID NOT NULL REFERENCES tasks(id) ON DELETE CASCADE,
  author_id        UUID NOT NULL,               -- plain UUID, references .NET users
  body             TEXT NOT NULL,
  image_urls       TEXT[] NOT NULL DEFAULT '{}',
  image_public_ids TEXT[] NOT NULL DEFAULT '{}', -- parallel array with image_urls
  created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

### MongoDB Collections (MongoDB Atlas)
- `activity_logs` — append-only event timeline (entity, actor, action, diff, timestamp)
- `notifications` — per-user inbox (recipientId, type, payload, read flag)
- `user_preferences` — one doc per user (theme, sidebarCollapsed, defaultTaskFilter)
- `audit_trail` — all mutating API calls, 90-day TTL

---

## What .NET Owns (APIs You Can Call)

The .NET service manages identity, workspaces, and teams. You will need to call these endpoints to validate references or enrich responses.

**Base URL:** `http://localhost:5000` (dev) — production URL TBD

### Endpoints Available to Django

| Method | Path | Use Case |
|---|---|---|
| `GET` | `/api/users?ids=u1,u2,u3` | Batch fetch user display data for enriching task/comment responses |
| `GET` | `/api/users/:id` | Get single user |
| `GET` | `/api/teams/:id` | Verify a team exists before creating board status or task |
| `GET` | `/api/teams/:id/members` | Verify a user is a member of a team |
| `GET` | `/api/teams?ids=t1,t2` | Batch fetch team display data (name, color) |

> Always use **batch calls** (comma-separated IDs) — never call in a loop per item. See the "Cross-Service Patterns" section below.

---

## Django APIs to Build

### Task Service — `/api/tasks`

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/tasks` | List tasks with filters (teamId, statusId, priority, assigneeId) |
| `POST` | `/api/tasks` | Create a task |
| `GET` | `/api/tasks/:id` | Get single task |
| `PATCH` | `/api/tasks/:id` | Update task fields |
| `DELETE` | `/api/tasks/:id` | Soft delete (set deleted_at) |
| `GET` | `/api/tasks/stats` | Aggregate counts by status for current user |
| `PATCH` | `/api/tasks/:id/status` | Drag-drop status change |

### Board Service — `/api/board`

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/board/:teamId` | Get all statuses + first 5 tasks per status |
| `GET` | `/api/board/:teamId/status/:statusId/tasks` | Paginated tasks for one column |
| `POST` | `/api/board/:teamId/statuses` | Create a new board status |
| `PATCH` | `/api/board/:teamId/statuses/:statusId` | Edit a status |
| `DELETE` | `/api/board/:teamId/statuses/:statusId` | Delete status — soft-deletes its tasks |

### Dashboard — `/api/dashboard`

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/dashboard/stats` | totalTasks, inProgress, completed, boardItems, completionRate |

### MongoDB-backed Services

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/activity` | Recent activity feed |
| `GET` | `/api/activity/tasks/:taskId` | Task timeline |
| `GET` | `/api/notifications` | Unread notifications |
| `PATCH` | `/api/notifications/:id/read` | Mark one read |
| `PATCH` | `/api/notifications/read-all` | Mark all read |
| `GET` | `/api/preferences` | Get user preferences |
| `PATCH` | `/api/preferences` | Update preferences |

### Internal Endpoints (called by .NET only)

These are NOT exposed to the frontend. Secured with `X-Internal-Secret` header.

| Method | Path | Triggered When |
|---|---|---|
| `POST` | `/internal/teams/:teamId/archive` | .NET deletes a team → soft-delete all tasks |
| `POST` | `/internal/tasks/unassign` | .NET removes a team member → remove from assignee_ids |

---

## Project Setup

### Prerequisites
- Python 3.11+
- pip
- Git
- A Supabase account (ask the .NET team to share the org invite)
- MongoDB Atlas access (ask the .NET team for the Atlas connection string)

---

### Step 1 — Create Supabase Project 2

1. Go to [supabase.com](https://supabase.com) → New Project
2. Name it: `taskflow-django` (or similar)
3. Choose the same region as the .NET project for lowest latency
4. After creation, go to: **Settings → Database → Connection string → URI**
5. Copy the URI — you'll need it in `.env`

---

### Step 2 — Scaffold the Django Project

```bash
# Create project folder (outside the .NET repo)
mkdir TaskFlowDjango
cd TaskFlowDjango

# Create and activate virtual environment
python -m venv venv

# Windows
venv\Scripts\activate

# macOS/Linux
source venv/bin/activate

# Install all dependencies
pip install django \
            djangorestframework \
            psycopg2-binary \
            djangorestframework-simplejwt \
            mongoengine \
            python-decouple \
            requests \
            django-cors-headers

# Save dependencies
pip freeze > requirements.txt

# Create Django project
django-admin startproject config .

# Create service apps
python manage.py startapp tasks
python manage.py startapp board
python manage.py startapp dashboard
python manage.py startapp activity
python manage.py startapp notifications
python manage.py startapp preferences
```

Your folder structure should look like:
```
TaskFlowDjango/
├── venv/
├── config/
│   ├── __init__.py
│   ├── settings.py
│   ├── urls.py
│   └── wsgi.py
├── tasks/
├── board/
├── dashboard/
├── activity/
├── notifications/
├── preferences/
├── manage.py
└── requirements.txt
```

---

### Step 3 — Create `.env` File

Create a `.env` file at the project root (never commit this):

```env
# Django
SECRET_KEY=your-django-secret-key-here
DEBUG=True

# Supabase Project 2 (Django DB)
DATABASE_URL=postgresql://postgres:[PASSWORD]@db.[REF].supabase.co:5432/postgres

# MongoDB Atlas
MONGODB_URI=mongodb+srv://[USER]:[PASSWORD]@[CLUSTER].mongodb.net/taskflow

# JWT — must match .NET appsettings exactly
JWT_SECRET_KEY=taskflow-super-secret-key-at-least-32-chars!!
JWT_ISSUER=TaskFlowBackend
JWT_AUDIENCE=TaskFlowBackendUsers

# Cross-service communication
DOTNET_SERVICE_URL=http://localhost:5000
INTERNAL_SERVICE_SECRET=<generate-a-random-256-bit-key-shared-with-dotnet-team>
```

Add `.env` to `.gitignore`:
```bash
echo ".env" >> .gitignore
echo "venv/" >> .gitignore
echo "__pycache__/" >> .gitignore
```

---

### Step 4 — Configure `settings.py`

Replace the default `settings.py` content with:

```python
from pathlib import Path
from datetime import timedelta
from decouple import config

BASE_DIR = Path(__file__).resolve().parent.parent

SECRET_KEY = config('SECRET_KEY')
DEBUG = config('DEBUG', default=False, cast=bool)
ALLOWED_HOSTS = ['*']

INSTALLED_APPS = [
    'django.contrib.contenttypes',
    'django.contrib.auth',
    'django.contrib.postgres',        # required for ArrayField (UUID arrays)
    'rest_framework',
    'corsheaders',
    'tasks',
    'board',
    'dashboard',
    'activity',
    'notifications',
    'preferences',
]

MIDDLEWARE = [
    'corsheaders.middleware.CorsMiddleware',
    'django.middleware.security.SecurityMiddleware',
    'django.middleware.common.CommonMiddleware',
]

ROOT_URLCONF = 'config.urls'

# PostgreSQL — Supabase Project 2
DATABASES = {
    'default': {
        'ENGINE': 'django.db.backends.postgresql',
        'NAME': 'postgres',
        'USER': 'postgres',
        'PASSWORD': config('DATABASE_URL').split(':')[2].split('@')[0],
        'HOST': config('DATABASE_URL').split('@')[1].split(':')[0],
        'PORT': '5432',
        'OPTIONS': {'sslmode': 'require'},
    }
}

# Simpler alternative — use dj-database-url:
# pip install dj-database-url
# import dj_database_url
# DATABASES = {'default': dj_database_url.config(default=config('DATABASE_URL'))}

# DRF
REST_FRAMEWORK = {
    'DEFAULT_AUTHENTICATION_CLASSES': (
        'rest_framework_simplejwt.authentication.JWTAuthentication',
    ),
    'DEFAULT_PERMISSION_CLASSES': (
        'rest_framework.permissions.IsAuthenticated',
    ),
}

# JWT — must match .NET JwtSettings exactly
SIMPLE_JWT = {
    'ALGORITHM': 'HS256',
    'SIGNING_KEY': config('JWT_SECRET_KEY'),
    'AUDIENCE': config('JWT_AUDIENCE'),
    'ISSUER': config('JWT_ISSUER'),
    'ACCESS_TOKEN_LIFETIME': timedelta(minutes=60),
    'VERIFY_EXPIRATION': True,
    'USER_ID_FIELD': 'id',
    'USER_ID_CLAIM': 'sub',       # .NET puts user ID in 'sub' claim
}

# CORS — allow the frontend origin
CORS_ALLOW_ALL_ORIGINS = DEBUG   # True in dev only; set explicit origins in prod

# Internationalization
LANGUAGE_CODE = 'en-us'
USE_TZ = True
TIME_ZONE = 'UTC'

# Cross-service config
DOTNET_SERVICE_URL = config('DOTNET_SERVICE_URL', default='http://localhost:5000')
INTERNAL_SERVICE_SECRET = config('INTERNAL_SERVICE_SECRET')
```

---

### Step 5 — Configure `urls.py`

```python
# config/urls.py
from django.urls import path, include

urlpatterns = [
    path('api/tasks/',      include('tasks.urls')),
    path('api/board/',      include('board.urls')),
    path('api/dashboard/',  include('dashboard.urls')),
    path('api/activity/',   include('activity.urls')),
    path('api/notifications/', include('notifications.urls')),
    path('api/preferences/',   include('preferences.urls')),
    path('internal/',       include('internal.urls')),   # .NET → Django internal calls
]
```

---

### Step 6 — Custom Auth User (Important)

Django's default auth system expects a `User` table in its own DB. Since users live in the .NET DB, you need a lightweight custom user class that reads from the JWT claims only — **no database table needed**.

```python
# config/auth.py
from rest_framework_simplejwt.authentication import JWTAuthentication
from rest_framework_simplejwt.exceptions import InvalidToken

class JWTUser:
    """Lightweight user object populated from JWT claims. No DB lookup."""
    def __init__(self, payload):
        self.id = payload.get('sub')          # UUID string
        self.email = payload.get('email', '')
        self.name = payload.get('name', '')
        self.is_authenticated = True
        self.is_anonymous = False

class TaskFlowJWTAuthentication(JWTAuthentication):
    def get_user(self, validated_token):
        return JWTUser(validated_token)
```

```python
# config/settings.py — update REST_FRAMEWORK
REST_FRAMEWORK = {
    'DEFAULT_AUTHENTICATION_CLASSES': (
        'config.auth.TaskFlowJWTAuthentication',   # use custom class
    ),
    'DEFAULT_PERMISSION_CLASSES': (
        'rest_framework.permissions.IsAuthenticated',
    ),
}
```

In any view, access the current user like:
```python
user_id = request.user.id       # UUID string from JWT sub claim
user_email = request.user.email
```

---

### Step 7 — Run Migrations

```bash
python manage.py makemigrations tasks board dashboard
python manage.py migrate
```

This creates the `board_statuses`, `tasks`, and `comments` tables in **Supabase Project 2 only**.

---

### Step 8 — Verify Setup

```bash
python manage.py runserver 8000
```

Test with a JWT token obtained from the .NET service:
```bash
curl -H "Authorization: Bearer <token-from-dotnet-login>" \
     http://localhost:8000/api/tasks/
```

Should return `200` with an empty task list — not `401`.

---

### JWT Configuration (Critical — Must Match .NET)

The JWT token is **issued by .NET** and **validated by Django**. Both must use the exact same values:

| Setting | Value |
|---|---|
| Algorithm | `HS256` |
| Secret Key | Same string as `.NET appsettings → JwtSettings.Key` |
| Issuer | Same string as `.NET appsettings → JwtSettings.Issuer` |
| Audience | Same string as `.NET appsettings → JwtSettings.Audience` |
| User ID claim | `sub` (standard JWT subject claim) |

> Get the exact values from the .NET team — do not generate new ones.

---

### Internal Secret (for .NET → Django calls)

```python
# core/decorators.py
import os
from functools import wraps
from django.http import JsonResponse

def internal_only(view_func):
    @wraps(view_func)
    def wrapper(request, *args, **kwargs):
        secret = request.headers.get('X-Internal-Secret', '')
        if secret != os.environ.get('INTERNAL_SERVICE_SECRET', ''):
            return JsonResponse({'status': False, 'code': 403, 'message': 'Forbidden'}, status=403)
        return view_func(request, *args, **kwargs)
    return wrapper
```

Usage on internal views:
```python
@internal_only
def archive_team_tasks(request, team_id):
    Task.objects.filter(team_id=team_id).update(deleted_at=now())
    ...
```

---

## API Response Format

**Every endpoint must return the same response envelope as .NET:**

### Success
```json
{
  "status": true,
  "code": 200,
  "result": { },
  "message": "Task created successfully.",
  "errors": [],
  "devMessage": "",
  "requestId": "req_abc123",
  "timestamp": "2026-06-20T10:00:00.000Z"
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
  "requestId": "req_abc123",
  "timestamp": "2026-06-20T10:00:00.000Z"
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
    { "field": "title", "code": "REQUIRED", "message": "Title is required." }
  ],
  "devMessage": "only populated outside production",
  "requestId": "req_abc123",
  "timestamp": "2026-06-20T10:00:00.000Z"
}
```

> Build a reusable `ApiResponse` helper in Django that wraps all responses in this shape. Field names are **camelCase** in JSON.

---

## Cross-Service API Call Patterns

### Enriching Task Responses (Django → .NET)

When returning a task list, you need assignee names and team display data. Use **one batched call** — never call per item.

```python
import requests

DOTNET_URL = os.environ.get('DOTNET_SERVICE_URL')

def fetch_users(user_ids: list[str]) -> dict:
    """Returns {user_id: {name, avatarInitials, avatarUrl}}"""
    if not user_ids:
        return {}
    ids_param = ','.join(user_ids)
    resp = requests.get(f'{DOTNET_URL}/api/users', params={'ids': ids_param}, timeout=5)
    users = resp.json().get('result', {}).get('data', [])
    return {u['id']: u for u in users}

def fetch_teams(team_ids: list[str]) -> dict:
    """Returns {team_id: {name, color}}"""
    if not team_ids:
        return {}
    ids_param = ','.join(team_ids)
    resp = requests.get(f'{DOTNET_URL}/api/teams', params={'ids': ids_param}, timeout=5)
    teams = resp.json().get('result', {}).get('data', [])
    return {t['id']: t for t in teams}
```

**Usage in a view:**
```python
tasks = Task.objects.filter(team_id=team_id, deleted_at__isnull=True)

# Collect unique IDs
all_user_ids = list({str(uid) for t in tasks for uid in t.assignee_ids} |
                    {str(t.created_by) for t in tasks})
all_team_ids = list({str(t.team_id) for t in tasks})

# One call each
users_map = fetch_users(all_user_ids)
teams_map = fetch_teams(all_team_ids)

# Enrich
for task in tasks:
    task._assignees = [users_map.get(str(uid)) for uid in task.assignee_ids]
    task._team = teams_map.get(str(task.team_id))
```

---

## Design Decisions to Follow

| Decision | Rule |
|---|---|
| **Enums** | Store as plain text strings — never as integers. Use `TextChoices` in Django models. |
| **UUIDs** | All primary keys are UUIDs (`uuid.uuid4()`). Cross-service references are plain UUID fields with no FK constraint. |
| **Assignees** | Stored as `UUID[]` array on the tasks table (PostgreSQL ArrayField). No junction table. |
| **Soft delete** | Tasks are soft-deleted by setting `deleted_at = now()`. All list queries must filter `deleted_at__isnull=True`. |
| **Timestamps** | All timestamps in UTC. Use `auto_now_add` and `auto_now` on Django models. |
| **Image arrays** | `image_urls` and `image_public_ids` are parallel arrays — always update them together. Index `i` in `image_public_ids` is the Cloudinary ID for `image_urls[i]`. |
| **Board defaults** | When .NET creates a team, it calls Django to seed 3 default board statuses: Backlog (pos 1), In Progress (pos 2), Done (pos 3). |
| **Pagination** | Default: `page=1`, `limit=20`. Board columns use `limit=5` for initial load, then load-more. |
| **Task identifier** | `task_number` is team-scoped sequential integer. Human-readable ID like `TF-001` is assembled in the response (prefix + task_number). |

---

## Data Consistency Rules

Since there are no cross-DB foreign keys, apply these rules at the application layer:

| Scenario | What Django Does |
|---|---|
| Creating a board status | Verify `team_id` exists → `GET .NET/api/teams/:id`. Return `404` if not found. |
| Creating a task | Verify `team_id` exists and `status_id` belongs to that team. |
| Assigning a user | Verify user exists via `GET .NET/api/users/:id`. |
| .NET deletes a team | `.NET calls POST /internal/teams/:teamId/archive` → Django sets `deleted_at` on all tasks for that team. |
| .NET removes a team member | `.NET calls POST /internal/tasks/unassign` → Django removes user from all `assignee_ids` arrays in that team. |
| Deleting the last board status | Return `422` — a team must always have at least one status. |

---

## Questions?

Contact the .NET team for:
- The production JWT secret key and Supabase Project 1 connection details
- The `INTERNAL_SERVICE_SECRET` value (shared, never committed to git)
- Confirming which .NET endpoints are ready for you to call
