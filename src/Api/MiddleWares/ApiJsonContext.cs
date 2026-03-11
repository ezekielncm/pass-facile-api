using System.Text.Json.Serialization;

namespace Api.MiddleWares
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(GlobalExceptionHandlerMiddleware.ErrorResponse))]
    [JsonSerializable(typeof(GlobalExceptionHandlerMiddleware.ValidationErrorResponse))]
    internal partial class ApiJsonContext : JsonSerializerContext
    {
    }
}