using System.Net;
using System.Net.Mime;
using CrowdParlay.Social.Application.Exceptions;
using Grpc.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Services;

/// <summary>
/// Handles exceptions thrown in the ASP.NET Core pipeline and writes the details to the HTTP response in accordance with RFC 7807.
/// Gets called by the default ASP.NET Core exception handling middleware.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "{ExceptionMessage}", exception.Message);

        var problemDetails = exception switch
        {
            ValidationException e => ConvertToProblemDetails(e),
            FluentValidation.ValidationException e => ConvertToProblemDetails(e),
            NotFoundException => GetNotFoundProblemDetails(),
            ForbiddenException => GetForbiddenProblemDetails(),
            RpcException => GetUnavailableDependencyProblemDetails(),
            _ => GetDefaultProblemDetails()
        };

        context.Response.ContentType = MediaTypeNames.Application.ProblemJson;
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails, GlobalSerializerOptions.SnakeCase, cancellationToken);

        return true;
    }

    private static ProblemDetails GetDefaultProblemDetails() => new()
    {
        Status = (int)HttpStatusCode.InternalServerError,
        Detail = "Something went wrong. Try again later.",
    };

    private static ValidationProblemDetails ConvertToProblemDetails(ValidationException exception) => new()
    {
        Status = (int)HttpStatusCode.BadRequest,
        Detail = "The specified data is invalid.",
        Errors = exception.Errors.ToDictionary(
            x => x.Key,
            x => x.Value.ToArray())
    };

    private static ValidationProblemDetails ConvertToProblemDetails(FluentValidation.ValidationException exception) => new()
    {
        Status = (int)HttpStatusCode.BadRequest,
        Detail = "The specified data is invalid.",
        Errors = exception.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                propertyFailureGroup => propertyFailureGroup.Key,
                propertyFailureGroup => propertyFailureGroup
                    .Select(failure => failure.ErrorMessage)
                    .ToArray())
    };

    private static ProblemDetails GetNotFoundProblemDetails() => new()
    {
        Status = (int)HttpStatusCode.NotFound,
        Detail = "The requested resource doesn't exist."
    };

    private static ProblemDetails GetForbiddenProblemDetails() => new()
    {
        Status = (int)HttpStatusCode.Forbidden,
        Detail = "You have no permission for this action."
    };

    private static ProblemDetails GetUnavailableDependencyProblemDetails() => new()
    {
        Status = (int)HttpStatusCode.ServiceUnavailable,
        Detail = "Failed to handle the request due to an unavailable dependency. Try again later."
    };
}