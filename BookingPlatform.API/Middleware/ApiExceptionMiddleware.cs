using FluentValidation;
using System.Net;

namespace BookingPlatform.API.Middleware;

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ApiExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteResponse(context, HttpStatusCode.BadRequest, "Validacion fallida.", ex.Errors.Select(error => error.ErrorMessage));
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteResponse(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteResponse(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception)
        {
            await WriteResponse(context, HttpStatusCode.InternalServerError, "Ocurrio un error inesperado.");
        }
    }

    private static Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message, IEnumerable<string>? errors = null)
    {
        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = new ErrorResponse(message, errors?.ToArray());

        return context.Response.WriteAsJsonAsync(payload);
    }

    private sealed record ErrorResponse(string Message, string[]? Errors = null);
}