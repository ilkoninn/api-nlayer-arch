using Newtonsoft.Json;

namespace App.API.Middlewares;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static Task HandleException(HttpContext context, Exception ex)
    {
        var code = StatusCodes.Status500InternalServerError;

        switch (ex)
        {
            case UnauthorizedAccessException:
                code = StatusCodes.Status401Unauthorized;
                break;
        }

        var errorResponse = new
        {
            status = code,
            message = ex.Message,
            inner = ex.InnerException?.Message,
            inner2 = ex.InnerException?.InnerException?.Message,
        };

        var result = JsonConvert.SerializeObject(errorResponse);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = code;

        return context.Response.WriteAsync(result);
    }
}

