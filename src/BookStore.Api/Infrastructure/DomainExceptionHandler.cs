using Microsoft.AspNetCore.Diagnostics;

namespace BookStore.Api.Infrastructure;

/// <summary>
/// Turns domain exceptions into RFC 9457 Problem Details (Constitution II). Anything else falls through to the
/// default handler, which returns a generic 500 without leaking internals.
/// </summary>
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<DomainExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Request conflicts with current state"),
            DomainValidationException => (StatusCodes.Status400BadRequest, "Request is invalid"),
            _ => (0, string.Empty),
        };

        if (status == 0)
        {
            return false;
        }

        logger.LogInformation("Domain rule returned {Status}: {Message}", status, exception.Message);

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
            },
        });
    }
}
