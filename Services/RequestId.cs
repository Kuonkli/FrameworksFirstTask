// Services/RequestId.cs
using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace FrameworksFirstTask.Services;

/// <summary>
/// Генерация и хранение идентификатора запроса
/// </summary>
public static class RequestId
{
    private const string ItemKey = "request_id";
    private const string HeaderName = "X-Request-Id";
    private static readonly Regex Allowed = new("^[a-zA-Z0-9\\-]{1,64}$", RegexOptions.Compiled);

    public static string GetOrCreate(HttpContext context)
    {
        if (context.Items.TryGetValue(ItemKey, out var existing) && existing is string s && !string.IsNullOrEmpty(s))
            return s;

        // Пробуем взять из заголовка
        var candidate = context.Request.Headers[HeaderName].FirstOrDefault();
        var requestId = !string.IsNullOrWhiteSpace(candidate) && Allowed.IsMatch(candidate!)
            ? candidate!
            : GenerateRequestId();

        context.Items[ItemKey] = requestId;
        context.Response.Headers[HeaderName] = requestId;

        return requestId;
    }

    private static string GenerateRequestId()
    {
        // Формат: timestamp-рандом (читаемо и уникально)
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..8];
        return $"{timestamp}-{random}";
    }
}