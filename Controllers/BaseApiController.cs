using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected ApiResponse<T> SuccessResponse<T>(string message, string action, T? data, int statusCode)
        {
            return ApiResponseBuilder.Success(
                message: message,
                action: action,
                data: data,
                statusCode: statusCode,
                path: HttpContext.Request.Path.Value ?? string.Empty,
                traceId: HttpContext.TraceIdentifier);
        }

        protected ApiResponse<T> ErrorResponse<T>(string message, string action, int statusCode, T? data = default)
        {
            return ApiResponseBuilder.Error(
                message: message,
                action: action,
                statusCode: statusCode,
                path: HttpContext.Request.Path.Value ?? string.Empty,
                traceId: HttpContext.TraceIdentifier,
                data: data);
        }
    }
}
