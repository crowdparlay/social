using System.Net.Mime;
using System.Text.Json;
using CrowdParlay.Social.Application.Exceptions;
using ApplicationException = CrowdParlay.Social.Domain.Exceptions.ApplicationException;

namespace CrowdParlay.Social.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "{ExceptionMessage}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        var statusCode = GetStatusCode(exception);

        var response = new
        {
            Title = GetTitle(exception),
            Status = statusCode,
            Detail = exception.Message,
            Errors = GetErrors(exception)
        };

        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private int GetStatusCode(Exception exception) => exception switch
    {
        ValidationException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    private string GetTitle(Exception exception) => exception switch
    {
        ApplicationException a => a.Title,
        _ => "Server error"
    };

    private IReadOnlyDictionary<string, string[]> GetErrors(Exception exception) => exception switch
    {
        ValidationException validationException => validationException.ErrorsDictionary,
        _ => new Dictionary<string, string[]>()
    };
}