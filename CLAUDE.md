# TaskFlow Backend — CLAUDE.md

## Project Overview

ASP.NET Core 8.0 Web API backend for TaskFlow — a team task management platform. Clean layered architecture: Controllers → Services → Repositories → EF Core (PostgreSQL). Redis for refresh token caching. JWT for auth.

**Stack:** .NET 8, PostgreSQL (Supabase), Redis (Upstash), BCrypt, StackExchange.Redis, EF Core 8, Swagger

---

## Architecture

```
Controllers/        → Route handlers, no business logic
Services/           → Business logic layer
  Interfaces/       → Service contracts
Repository/         → Data access (EF Core queries only)
  Interfaces/       → Repository contracts
Models/             → EF Core entities (DB tables)
DTOs/               → Request/response shapes (never expose Models directly)
Data/               → AppDBContext + FluentAPIConfigurations
Middleware/         → ExceptionMiddleware (global error handling)
Helpers/            → ApiResponse<T>, ApiError, custom exceptions
Enums/              → Shared enum types
APP Progress/       → Per-controller API endpoint tracking (markdown)
```

### Dependency Injection (Program.cs)

| Lifetime | Type | Implementation |
|----------|------|----------------|
| Singleton | IConnectionMultiplexer | ConnectionMultiplexer (Redis) |
| Singleton | IConnection | RabbitMQ.Client connection |
| Scoped | IRedisCacheService | RedisCacheService |
| Scoped | IUserRepository | UserRepository |
| Scoped | ITokenService | TokenService |
| Scoped | IAuthService | AuthService |
| Scoped | IUserService | UserService |
| Scoped | IEventPublisherService | EventPublisherService |

When adding a new service: register `IFoo, FooService` — never register the concrete class alone.

---

## API Response Contract

Every endpoint returns `ApiResponse<T>` (camelCase JSON):

```json
{
  "status": true,
  "code": 200,
  "result": { },
  "message": "Human readable message",
  "errors": [],
  "devMessage": "",
  "requestId": "trace-id",
  "timestamp": "2026-06-26T00:00:00Z"
}
```

For errors, `result` is null and `errors` is a list of `{ field, code, message }`.

**HTTP status codes used:**
- 200 — Success
- 201 — Created (signup)
- 400 — Bad request
- 401 — Unauthorized
- 403 — Forbidden
- 404 — Not found
- 409 — Conflict (e.g. email already exists)
- 422 — Validation failure (model state errors)
- 500 — Unhandled exception

### Validation errors (422)
Model validation is intercepted via `InvalidModelStateResponseFactory` in Program.cs — it returns `ApiResponse<object>.Failure(...)` with the field-level errors list. Do NOT use `BadRequestObjectResult` for validation — use `UnprocessableEntityObjectResult`.

---

## Custom Exceptions

Throw these from services; `ExceptionMiddleware` catches and maps them:

```csharp
throw new NotFoundException("User not found.");
throw new UnauthorizedException("Invalid credentials.");
throw new ForbiddenException("Access denied.");
throw new ValidationException(new List<ApiError> {
    new ApiError { Field = "email", Code = "EMAIL_TAKEN", Message = "Email already in use." }
});
```

Never let a raw `Exception` propagate intentionally — the middleware will catch it as a 500.

---

## Authentication Flow

1. **Signup** → BCrypt hash password → create User in DB
2. **Login** → verify BCrypt hash → generate JWT (60 min) + refresh token (random 32 bytes, Base64)
3. Refresh token stored in Redis: key `refresh_token:{token}` → `RedisTokenValueDTO` (userId, email, createdAt) with 7-day TTL
4. **Refresh** → look up key in Redis → fetch user by userId → generate new JWT
5. Protected routes require `Authorization: Bearer <jwt>` header

JWT claims: `sub` (userId), `email`, `name`, `jti`, `iat` (Unix timestamp), `avatarUrl`, `title`
`iss`, `aud`, `exp` are set by the `JwtSecurityToken` constructor — do NOT add them as manual claims.

---

## Database

**Provider:** PostgreSQL via Npgsql EF Core  
**Connection:** Supabase (SSL required)  
**Migrations:** `dotnet ef migrations add <Name>` / `dotnet ef database update`  
**Config:** All Fluent API in `Data/FluentAPIConfigurations.cs` — no data annotations on models.

### Entities in DbContext

| DbSet | Table | Status |
|-------|-------|--------|
| Users | users | Active |
| Workspaces | workspaces | Active |
| WorkspaceMembers | workspace_members | Active |
| WorkspaceInvitations | workspace_invitations | Active |
| Teams | teams | Active |
| TeamMembers | team_members | Active |
| TeamInvitations | invitations | Active |
| BoardStatuses | — | Not yet added to DbContext |
| TaskItems | — | Not yet added to DbContext |
| Comments | — | Not yet added to DbContext |

### Key DB Constraints
- `users.email` — unique index
- `workspace_members(workspace_id, user_id)` — unique
- `workspace_invitations(workspace_id, email)` — unique
- `team_members(team_id, user_id)` — composite PK
- `team_invitations(team_id, email)` — unique
- All Guid PKs default to `gen_random_uuid()`
- All timestamps default to `NOW()` at DB level

---

## Models Overview

| Model | Key Fields |
|-------|-----------|
| User | Id, Name, Email (unique), Title, AvatarInitials, AvatarUrl, AvatarPublicId, PasswordHash |
| Workspace | Id, Name, OwnerId (→ User) |
| WorkspaceMember | Id, WorkspaceId, UserId, Status (Active/Pending), JoinedAt |
| WorkspaceInvitation | Id, WorkspaceId, InvitedBy, Email, Status, ExpiresAt (+7 days) |
| Team | Id, Name, Description, Color (hex), WorkspaceId, AdminId, CreatedBy |
| TeamMember | TeamId + UserId (composite PK), Role, JoinedAt |
| TeamInvitation | Id, TeamId, InvitedBy, Email, Role, Status, ExpiresAt (+7 days) |
| BoardStatus | Id, TeamId, Name, Description, Position |
| TaskItem | Id, TaskNumber, Title, Description, Priority, Label, StatusId, TeamId, AssigneeIds (Guid[]), Progress (0–100), DeletedAt (soft delete) |
| Comment | Id, TaskId, AuthorId, Body, ImageUrls (string[]), ImagePublicIds (string[]) |

---

## Enums

| Enum | Values |
|------|--------|
| Priority | High, Medium, Low |
| TeamRole | Admin, PM, TL, Developer |
| LabelType | Feature, Bug, Design, Docs, Infra, Refactor |
| InvitationStatus | Pending, Accepted, Declined, Expired |
| WorkspaceMemberStatus | Active, Pending |
| SprintStatus | Planning, Active, Completed |

All enums stored as **strings** in the DB (configured in FluentAPIConfigurations).

---

## NuGet Packages

| Package | Purpose |
|---------|---------|
| BCrypt.Net-Next 4.0.3 | Password hashing |
| Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0 | JWT bearer auth |
| Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0 | PostgreSQL provider |
| StackExchange.Redis 3.0.0 | Redis client |
| RabbitMQ.Client 7.1.2 | AMQP client — publishes events to `notifications.exchange` for the `Notification` microservice to consume |
| Swashbuckle.AspNetCore 6.6.2 | Swagger UI |
| OneOf 3.0.160 | Result type (available, not yet widely used) |
| Microsoft.EntityFrameworkCore.Tools 8.0.0 | EF Core CLI |

---

## Configuration

Secrets live in `appsettings.Development.json` (gitignored in production).  
`appsettings.json` holds only the schema with empty values.

**Required config keys:**
```
ConnectionStrings:DefaultConnection   → PostgreSQL connection string
ConnectionStrings:RedisConnection     → StackExchange.Redis format (not rediss:// URI)
ConnectionStrings:RabbitMqConnection  → AMQP URI, e.g. amqp://guest:guest@localhost:5672
RabbitMq:ExchangeName                 → Shared topic exchange name (notifications.exchange)
JwtSettings:Key                       → Min 32-char signing key
JwtSettings:Issuer
JwtSettings:Audience
JwtSettings:ExpiryMinutes             → Access token TTL (default 60)
```

**Redis connection string format** (StackExchange.Redis, not URI):
```
host:port,password=xxx,ssl=True,abortConnect=False
```

---

## Running Locally

```bash
dotnet run               # starts on http/https (see launchSettings.json)
dotnet ef migrations add <Name>
dotnet ef database update
```

Swagger UI available at `/swagger` in Development.

---

## APP Progress Tracking

`APP Progress/` folder contains one `.md` file per controller documenting completed endpoints with route, method, auth, request body, and response shape. Update these as endpoints are finished. When Postman MCP is connected, these files are used to push requests directly into Postman collections.

---

## Conventions

- **Never** expose `Model` classes directly — always map to a DTO before returning
- **Never** register a concrete service class without its interface in DI
- JWT: `iss`, `aud`, `exp` are set by the `JwtSecurityToken` constructor — don't add as manual claims
- `iat` must be a Unix timestamp: `DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()`
- TaskItems are hard-deleted (`TaskRepository.DeleteAsync` removes the row; `Comments` cascade-delete with it). The `DeletedAt` field still exists on the model but is no longer used by the delete flow.
- Avatar initials are auto-computed from first + last name characters
- Cloudinary `AvatarPublicId` is stored for cleanup on avatar replace/delete
