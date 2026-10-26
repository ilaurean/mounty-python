using Commons.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Commons.Services;

public static class ExceptionExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = MediaTypeNames.Application.Json;

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var logger = appError.ApplicationServices.GetService<ILogger>();

                    var exception = contextFeature.Error;
                    var defaultResponseBody = JsonSerializer.Serialize(new
                    {
                        Message = "An unexpected error occurred while processing the request."
                    });

                    switch (exception)
                    {
                        case DbUpdateConcurrencyException:
                            context.Response.StatusCode = StatusCodes.Status409Conflict;
                            break;
                        case DbUpdateException:
                            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                            break;
                        case InvalidOperationException:
                            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                            break;
                        case NotFoundException:
                            context.Response.StatusCode = StatusCodes.Status404NotFound;
                            break;
                        case UnauthorizedAccessException:
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            defaultResponseBody = JsonSerializer.Serialize(new
                            {
                                Message = exception.Message
                            });
                            break;
                        case BusinessException:
                            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                            break;
                        case ValidationException:
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            defaultResponseBody = JsonSerializer.Serialize(new
                            {
                                Message = "Validation failed.",
                                Errors = exception is ValidationException validationException
                                    ? validationException.Errors
                                    : null
                            });
                            break;
                        default:
                            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                            break;
                    }
                    await context.Response.WriteAsync(defaultResponseBody, Encoding.UTF8);
                    logger?.Error(exception, "An unhandled exception occurred while processing the request.");
                }
            });
        });
    }
}
