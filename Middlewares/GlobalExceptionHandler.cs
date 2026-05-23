using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;

namespace EXE02_Backend_RE_CAFE.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred.";

            if (exception is ApiException apiException)
            {
                statusCode = apiException.StatusCode;
                message = apiException.Message;
            }

            var routeAction = httpContext.GetRouteData()?.Values["action"]?.ToString();
            var actionName = string.IsNullOrWhiteSpace(routeAction) ? "UnhandledException" : routeAction;

            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = "application/json";

            var errorResponse = ApiResponseBuilder.Error<object>(
                message: message,
                action: actionName,
                statusCode: (int)statusCode,
                path: httpContext.Request.Path.Value ?? string.Empty,
                traceId: httpContext.TraceIdentifier);

            var responseJson = JsonSerializer.Serialize(errorResponse);
            await httpContext.Response.WriteAsync(responseJson, cancellationToken);

            return true;
        }
    }
}
