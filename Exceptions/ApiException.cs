using System;
using System.Net;

namespace EXE02_Backend_RE_CAFE.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public ApiException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : ApiException
    {
        public NotFoundException(string message) 
            : base(message, HttpStatusCode.NotFound)
        {
        }
    }

    public class BadRequestException : ApiException
    {
        public BadRequestException(string message) 
            : base(message, HttpStatusCode.BadRequest)
        {
        }
    }

    public class ConflictException : ApiException
    {
        public ConflictException(string message)
            : base(message, HttpStatusCode.Conflict)
        {
        }
    }

    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message) 
            : base(message, HttpStatusCode.Unauthorized)
        {
        }
    }
}
