using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Server.Dtos;

namespace Server.Middleware
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
            var traceId = httpContext.TraceIdentifier;
            ApiError errorResponse;

            switch (exception)
            {
                case ArgumentException argEx:
                    errorResponse = ApiError.BadRequest(argEx.Message, traceId);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case InvalidOperationException invOpEx:
                    errorResponse = ApiError.BadRequest(invOpEx.Message, traceId);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case KeyNotFoundException notFoundEx:
                    errorResponse = ApiError.NotFound(notFoundEx.Message, traceId);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case UnauthorizedAccessException:
                    errorResponse = ApiError.BadRequest("Unauthorized access", traceId);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                default:
                    _logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}", traceId);
                    errorResponse = ApiError.InternalServerError(
                        "An unexpected error occurred. Please try again later.",
                        traceId);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            httpContext.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await httpContext.Response.WriteAsJsonAsync(errorResponse, jsonOptions, cancellationToken);

            return true;
        }
    }
}

