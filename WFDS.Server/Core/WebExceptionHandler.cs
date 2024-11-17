using Microsoft.AspNetCore.Diagnostics;

namespace WFDS.Server.Core;

public class WebExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception ex, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = 500 ;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            error = ex.Message
        }, cancellationToken);
        return true;
    }
}