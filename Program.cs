using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Data;
using TaskFlowBackend.Middleware;
using TaskFlowBackend.Repository;
using TaskFlowBackend.Repository.Interfaces;
using TaskFlowBackend.Services;
using TaskFlowBackend.Services.Interfaces;
using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using TaskFlowBackend.Repository.Archive.Interfaces;
using TaskFlowBackend.Repository.Archive;
using TaskFlowBackend.Services.Archive.Interfaces;
using TaskFlowBackend.Services.Archive;

// Render's containers hit inotify limits when appsettings.json watches are enabled;
// disabling reload-on-change avoids the FileSystemWatcher crash at startup.
Environment.SetEnvironmentVariable("DOTNET_hostBuilder__reloadConfigOnChange", "false");

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ArchiveDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ArchiveConnection")));

// Redis Configuration
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    string connectionString = builder.Configuration.GetConnectionString("RedisConnection") ?? "";
    return ConnectionMultiplexer.Connect(connectionString);
});

// RabbitMQ Configuration
builder.Services.AddSingleton<IConnection>(sp =>
{
    string connectionString = builder.Configuration.GetConnectionString("RabbitMqConnection") ?? "amqp://guest:guest@localhost:5672";
    var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// Supabase Storage (avatar uploads)
builder.Services.AddHttpClient("SupabaseStorage", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["SupabaseStorage:Url"] ?? "";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("apikey", config["SupabaseStorage:ServiceRoleKey"]);
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config["SupabaseStorage:ServiceRoleKey"]);
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var response = ApiResponse<object>.Failure(
                    message: "Unauthorized. Token is missing or invalid.",
                    code: 401,
                    requestId: context.HttpContext.TraceIdentifier
                );
                await context.Response.WriteAsJsonAsync(response,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                var response = ApiResponse<object>.Failure(
                    message: "Forbidden. You do not have permission to access this resource.",
                    code: 403,
                    requestId: context.HttpContext.TraceIdentifier
                );
                await context.Response.WriteAsJsonAsync(response,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
        };
    });

// ========== Services ==========

// ==== DB Service ====
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// ==== Repository Services ====
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceMemberRepository, WorkspaceMemberRepository>();
builder.Services.AddScoped<IWorkspaceInvitationRepository, WorkspaceInvitationRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
builder.Services.AddScoped<ITeamInvitationRepository, TeamInvitationRepository>();
builder.Services.AddScoped<IBoardStatusRepository, BoardStatusRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IMigrateTasksRepository, MigrateTasksRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// ==== APP Services ====
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IBoardStatusService, BoardStatusService>();
builder.Services.AddSingleton<IEventPublisherService, EventPublisherService>();
builder.Services.AddScoped<IAvatarStorageService, AvatarStorageService>();
builder.Services.AddScoped<ITaskMigrationService, TaskMigrationService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<ITaskExportService, TaskExportService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();


builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err => new ApiError
                {
                    Field = e.Key,
                    Code = "ValidationError",
                    Message = err.ErrorMessage
                }))
                .ToList();

            var response = ApiResponse<object>.Failure(
                message: "Validation failed.",
                code: 422,
                errors: errors,
                requestId: context.HttpContext.TraceIdentifier
            );

            return new UnprocessableEntityObjectResult(response);
        };
    });
builder.Services.AddEndpointsApiExplorer();

// Swagger with Bearer token support
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TransactionMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
