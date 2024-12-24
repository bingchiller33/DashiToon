using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DashiToon.Api.Web.Infrastructure;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

    public CustomExceptionHandler()
    {
        // Register known exception types and handlers.
        _exceptionHandlers = new Dictionary<Type, Func<HttpContext, Exception, Task>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
            { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
            { typeof(Exception), HandleUnhandledException },
            { typeof(ChapterNotFoundException), HandleConflictException },
            { typeof(DashiFanTierNotFoundException), HandleConflictException },
            { typeof(VolumeNotFoundException), HandleConflictException },
            { typeof(ChapterVersionNotFoundException), HandleConflictException },
            { typeof(PublishMoreThanOnceException), HandleConflictException },
            { typeof(UnpublishNonPublishedChapterException), HandleConflictException },
            { typeof(DeleteInUseVersionChapterException), HandleConflictException },
            { typeof(AlreadyCheckinException), HandleConflictException },
            { typeof(AlreadySubscribedException), HandleConflictException },
            { typeof(BalanceInsufficientException), HandleConflictException },
            { typeof(UpdateDashiFanCoolDownException), HandleConflictException },
            { typeof(AdvanceChapterCannotBeUnlockedException), HandleConflictException },
            { typeof(NotPublishedChapterCannotBeUnlockedException), HandleConflictException },
            { typeof(ChapterAlreadyUnlockedException), HandleConflictException },
            { typeof(AlreadyReviewedException), HandleConflictException },
            { typeof(AlreadyFollowedException), HandleConflictException },
            { typeof(OverWithdrawException), HandleConflictException },
            { typeof(ArgumentException), HandleArgumentException },
            { typeof(GoneException), HandleGoneException }
        };
    }

    private async Task HandleArgumentException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Detail = ex.Message
        });
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        Type exceptionType = exception.GetType();

        if (!_exceptionHandlers.TryGetValue(exceptionType, out Func<HttpContext, Exception, Task>? handler))
        {
            return false;
        }

        await handler.Invoke(httpContext, exception);
        return true;
    }

    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        ValidationException exception = (ValidationException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Detail = exception.Message,
            Errors = exception.Errors
        });
    }

    private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        NotFoundException exception = (NotFoundException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found.",
            Detail = exception.Message
        });
    }

    private async Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        });
    }

    private async Task HandleUnhandledException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Error",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        });
    }

    private async Task HandleForbiddenAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        });
    }

    private async Task HandleConflictException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Detail = ex.Message
        });
    }

    private async Task HandleGoneException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status410Gone;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status410Gone,
            Title = "Gone",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Detail = "Phúc Fuck off bitch"
        });
    }
}
