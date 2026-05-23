using System;

namespace EXE02_Backend_RE_CAFE.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string Path { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
    }

    public static class ApiResponseBuilder
    {
        public static ApiResponse<T> Success<T>(
            string message,
            string action,
            T? data,
            int statusCode,
            string path,
            string traceId)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Action = action,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Path = path,
                TraceId = traceId
            };
        }

        public static ApiResponse<T> Error<T>(
            string message,
            string action,
            int statusCode,
            string path,
            string traceId,
            T? data = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Action = action,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Path = path,
                TraceId = traceId
            };
        }
    }
}
