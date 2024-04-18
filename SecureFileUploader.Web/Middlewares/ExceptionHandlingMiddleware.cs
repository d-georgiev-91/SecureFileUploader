using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SecureFileUploader.Services.Exceptions;

namespace SecureFileUploader.Web.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    private const string DefaultContentTypeForException = "application/json";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = DefaultContentTypeForException;

        ProblemDetails? problemDetails;

        if (exception is NotFoundException)
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "NotFound",
                Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.4",
                Detail = exception.Message
            };
        }
        else if (exception is InvalidCredentialException)
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Type = "https://www.rfc-editor.org/rfc/rfc7231#section-3.1",
                Detail = exception.Message
            };
        }
        else if (exception is UserAlreadyRegisteredException)
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "BadRequest",
                Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1",
                Detail = exception.Message
            };
        }
        else
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing the request.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Detail = "Error has occured, if you need assistance please contact our team."
            };
        }

        context.Response.StatusCode = problemDetails.Status!.Value;
        return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
