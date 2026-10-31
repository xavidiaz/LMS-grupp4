using Domain.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LMS.API.Extensions;

public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var problemDetailsFactory = app.Services.GetRequiredService<ProblemDetailsFactory>();
                    var exception = contextFeature.Error;

                    var (statusCode, title) = exception switch
                    {
                        InvalidActivityDateException ex => (StatusCodes.Status400BadRequest, ex.Message),
                        ActivityOverlappingException ex => (StatusCodes.Status409Conflict, ex.Message),
                        ModuleOverlappingException ex => (StatusCodes.Status409Conflict, ex.Message),
                        CourseNameRequiredException ex => (StatusCodes.Status400BadRequest, ex.Title),
                        CourseInvalidDatesException ex => (StatusCodes.Status400BadRequest, ex.Title),
                        InvalidFileException ex => (StatusCodes.Status400BadRequest, ex.Message),
                        InvalidModuleDateException ex => (StatusCodes.Status400BadRequest, ex.Message),
                        UserAlreadyExistsException ex => (StatusCodes.Status400BadRequest, ex.Title),
                        DeleteOwnAccountException ex => (StatusCodes.Status400BadRequest, ex.Title),
                        NotFoundException ex => (StatusCodes.Status404NotFound, ex.Title),
                        TokenValidationException ex => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                        DeleteCourseException ex => (StatusCodes.Status409Conflict, ex.Title),
                        InvalidSubmissionStatusException ex => (StatusCodes.Status400BadRequest, ex.Message),
                        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
                    };

                    var problemDetails = problemDetailsFactory.CreateProblemDetails(
                        context,
                        statusCode,
                        title,
                        detail: exception.Message,
                        instance: context.Request.Path);

                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsJsonAsync(problemDetails);
                }

            });
        });
    }
}
