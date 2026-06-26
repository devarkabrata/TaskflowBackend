namespace TaskFlowBackend.Helpers.API
{
    public class ApiResponse<T>
    {
        public bool Status { get; set; }
        public int Code { get; set; }
        public T? Result { get; set; }
        public string Message { get; set; } = "";
        public List<ApiError> Errors { get; set; } = new();
        public string DevMessage { get; set; } = "";
        public string RequestId { get; set; } = "";
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
        public string Source {get; set;} = "Dotnet Application";

        public static ApiResponse<T> Success(T result, string message = "", int code = 200, string requestId = "")
        {
            return new ApiResponse<T>
            {
                Status = true,
                Code = code,
                Result = result,
                Message = message,
                Errors = new List<ApiError>(),
                DevMessage = "",
                RequestId = requestId,
                Timestamp = DateTime.UtcNow.ToString("o"),
                Source = "Dotnet 8.0.0 web api"
            };
        }

        // Failure
        public static ApiResponse<T> Failure(string message, int code = 400, List<ApiError>? errors = null, string devMessage = "", string requestId = "")
        {
            return new ApiResponse<T>
            {
                Status = false,
                Code = code,
                Result = default,
                Message = message,
                Errors = errors ?? new List<ApiError>(),
                DevMessage = devMessage,
                RequestId = requestId,
                Timestamp = DateTime.UtcNow.ToString("o"),
                Source = "Dotnet 8.0.0 web api"
            };
        }
    }

    // Single error shape
    public class ApiError
    {
        public string Field { get; set; } = "";
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
    }
}