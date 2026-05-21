using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = statusCode.ToString(),
                Detail = message,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = "application/json";

            var responseJson = JsonSerializer.Serialize(problemDetails);
            await httpContext.Response.WriteAsync(responseJson, cancellationToken);

            return true;
        }
    }
}
