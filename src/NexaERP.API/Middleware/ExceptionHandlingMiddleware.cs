using System.Net;
using System.Text.Json;
using NexaERP.Domain.Exceptions;

namespace NexaERP.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title, errors) = ex switch
        {
            NotFoundException nfe =>
                (HttpStatusCode.NotFound, nfe.Message, (IDictionary<string, string[]>?)null),

            BusinessException be =>
                (HttpStatusCode.BadRequest, be.Message, null),

            Domain.Exceptions.ValidationException ve =>
                (HttpStatusCode.UnprocessableEntity, "One or more validation errors occurred.", (IDictionary<string, string[]>?)ve.Errors),

            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var body = new Dictionary<string, object> { ["title"] = title };
        if (errors is not null) body["errors"] = errors;

        return context.Response.WriteAsync(JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
