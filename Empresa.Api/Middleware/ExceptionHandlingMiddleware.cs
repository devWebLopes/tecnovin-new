using System.Net;
using System.Text.Json;

namespace Empresa.Api.Middleware;

/// <summary>
/// Middleware global para captura de exceções não tratadas
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Recurso não encontrado: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await WriteErrorResponse(context, "Not Found", ex.Message);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Violação de regra de negócio: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            await WriteErrorResponse(context, "Business Error", ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteErrorResponse(context, "Validation Error", ex.Message);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflito: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await WriteErrorResponse(context, "Conflict", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acesso não autorizado: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await WriteErrorResponse(context, "Forbidden", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno não tratado ao processar {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteErrorResponse(context, "Internal Server Error",
                "Ocorreu um erro interno. Tente novamente mais tarde.");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, string error, string message)
    {
        context.Response.ContentType = "application/json";
        var response = new ErrorResponse(error, message, context.TraceIdentifier);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Modelo padronizado para respostas de erro
/// </summary>
public record ErrorResponse(string Error, string Message, string? TraceId = null);

// ─── Exceções Customizadas ─────────────────────────────────────

/// <summary>
/// Recurso não encontrado (404)
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string resource, object id)
        : base($"{resource} com ID '{id}' não encontrado(a)") { }

    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Violação de regra de negócio (422)
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

/// <summary>
/// Conflito de dados (409) — duplicidade de registros
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>
/// Erro de validação de dados (400)
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
