// Middlewares/ErrorHandlingMiddleware.cs
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FrameworksFirstTask.Models;
using FrameworksFirstTask.Errors;
using FrameworksFirstTask.Services;

namespace FrameworksFirstTask.Middlewares;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestId = RequestId.GetOrCreate(context);

        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error: {Code} - {Message} (RequestId: {RequestId})", 
                ex.Code, ex.Message, requestId);
            
            await WriteErrorResponse(context, ex.StatusCode, ex.Code, ex.Message, requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception (RequestId: {RequestId})", requestId);
            
            await WriteErrorResponse(context, 500, "internal_error", 
                "Внутренняя ошибка сервера", requestId);
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context, 
        int statusCode, 
        string code, 
        string message, 
        string requestId)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new ErrorResponse(code, message, requestId, DateTime.UtcNow);
        var json = JsonSerializer.Serialize(response);
        
        await context.Response.WriteAsync(json);
    }
}