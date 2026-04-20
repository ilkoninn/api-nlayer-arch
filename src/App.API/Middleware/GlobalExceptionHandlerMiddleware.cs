namespace App.API.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            logger.LogError("Unhandled exception for {Method} {Path} - {ErrorMessage}", context.Request.Method, context.Request.Path, message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var code = StatusCodes.Status500InternalServerError;

		switch (ex)
		{
			case UnauthorizedAccessException:
				code = StatusCodes.Status401Unauthorized;
				break;
			case KeyNotFoundException:
				code = StatusCodes.Status404NotFound;
				break;
			case ArgumentException:
				code = StatusCodes.Status400BadRequest;
				break;
			case BusinessConflictException:
				code = StatusCodes.Status409Conflict;
				break;
			case InvalidOperationException when IsLoginPost(context):
				code = StatusCodes.Status403Forbidden;
				break;
			case InvalidOperationException:
				code = StatusCodes.Status400BadRequest;
				break;
		}

        var errorResponse = new
        {
            status = code,
            message = ex.Message,
            inner = ex.InnerException?.Message,
            inner2 = ex.InnerException?.InnerException?.InnerException?.Message,
        };

        var result = JsonConvert.SerializeObject(errorResponse);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = code;

        return context.Response.WriteAsync(result);
    }

    private static bool IsLoginPost(HttpContext context) =>
        HttpMethods.IsPost(context.Request.Method)
        && context.Request.Path.StartsWithSegments("/api/auth/login");
}

