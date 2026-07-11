using System.Text.Json;
using TaskFlowBackend.Helpers.API;
using TaskFlowBackend.Helpers.CustomException;

namespace TaskFlowBackend.Middleware
{
    // Middleware/ExceptionMiddleware.cs
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            IWebHostEnvironment env,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            // Generate RequestId
            var requestId = context.TraceIdentifier ?? Guid.NewGuid().ToString();

            // Dev message only shown outside production
            var devMessage = _env.IsProduction()
                ? ""
                : ex.InnerException?.Message ?? ex.Message;

            ApiResponse<object> response;

            switch (ex)
            {
                // Validation errors — 422
                case ValidationException validationEx:
                    context.Response.StatusCode = 422;
                    response = ApiResponse<object>.Failure(
                        message: "Validation failed.",
                        code: 422,
                        errors: validationEx.Errors,
                        devMessage: devMessage,
                        requestId: requestId
                    );
                    break;

                // Not found — 404
                case NotFoundException notFoundEx:
                    context.Response.StatusCode = 404;
                    response = ApiResponse<object>.Failure(
                        message: notFoundEx.Message,
                        code: 404,
                        devMessage: devMessage,
                        requestId: requestId
                    );
                    break;

                // Unauthorized — 401
                case UnauthorizedException unauthorizedEx:
                    context.Response.StatusCode = 401;
                    response = ApiResponse<object>.Failure(
                        message: unauthorizedEx.Message,
                        code: 401,
                        devMessage: devMessage,
                        requestId: requestId
                    );
                    break;

                // Forbidden — 403
                case ForbiddenException forbiddenEx:
                    context.Response.StatusCode = 403;
                    response = ApiResponse<object>.Failure(
                        message: forbiddenEx.Message,
                        code: 403,
                        devMessage: devMessage,
                        requestId: requestId
                    );
                    break;

                // Everything else — 500
                default:
                    context.Response.StatusCode = 500;
                    response = ApiResponse<object>.Failure(
                        message: "Something went wrong.",
                        code: 500,
                        devMessage: devMessage,
                        requestId: requestId
                    );
                    break;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // camelCase in JSON
            };

            await context.Response.WriteAsJsonAsync(response, options);
        }
    }
}