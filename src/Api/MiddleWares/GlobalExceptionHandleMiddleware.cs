using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.MiddleWares
{
    public sealed class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next)
            //ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            //_logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                //_logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

                await HandleExceptionAsync(context, exception);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var tuple = exception switch
            {
                //NotFoundException notFound => (
                //    HttpStatusCode.NotFound,
                //    (object)new ErrorResponse("NotFound", notFound.Message)
                //),

                //AppValidationException validation => (
                //    HttpStatusCode.BadRequest,
                //    (object)new ValidationErrorResponse(validation.Errors)
                //),

                //ConflictException conflict => (
                //    HttpStatusCode.Conflict,
                //    (object)new ErrorResponse("Conflict", conflict.Message)
                //),

                //ForbiddenAccessException forbidden => (
                //    HttpStatusCode.Forbidden,
                //    (object)new ErrorResponse("Forbidden", forbidden.Message)
                //),

                _ => (
                    HttpStatusCode.InternalServerError,
                    (object)new ErrorResponse("InternalServerError", "An internal server error occurred.")
                )
            };

            var statusCode = tuple.Item1;
            var response = tuple.Item2;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(
               response,
               ApiJsonContext.Default.ErrorResponse
                );

            await context.Response.WriteAsync(json);
        }

        public sealed record ErrorResponse(string Code, string Message);
        public sealed record ValidationErrorResponse(IDictionary<string, string[]> Errors)
        {
            public string Code => "ValidationError";
            public string Message => "One or more validation errors occurred.";
        }
    }
}
