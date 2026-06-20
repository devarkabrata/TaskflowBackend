using TaskFlowBackend.Helpers.API;

namespace TaskFlowBackend.Helpers.CustomException
{
    // Helpers/AppExceptions.cs
    public class ValidationException : Exception
    {
        public List<ApiError> Errors { get; }

        public ValidationException(string message = "Validation failed.", List<ApiError>? errors = null, int code = 400)
            : base(message)
        {
            base.Data["Code"] = code;
            Errors = errors ?? new List<ApiError>();
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message = "Not found.", int code = 404) : base(message)
        {
            base.Data["Code"] = code;
        }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Unauthorized.", int code = 401) : base(message)
        {
            base.Data["Code"] = code;
        }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Forbidden.", int code = 403) : base(message)
        {
            base.Data["Code"] = code;
        }
    }
}