using Microsoft.AspNetCore.Diagnostics;

namespace WFDS.Server.Core;

public sealed class WebExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = 500 ;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            error = exception.Message
        }, cancellationToken);
        return true;
    }
}