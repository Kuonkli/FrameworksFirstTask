// Middlewares/TimingAndLogMiddleware.cs
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FrameworksFirstTask.Services;

namespace FrameworksFirstTask.Middlewares;

public sealed class TimingAndLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimingAndLogMiddleware> _logger;

    public TimingAndLogMiddleware(RequestDelegate next, ILogger<TimingAndLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestId = RequestId.GetOrCreate(context);
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            _logger.LogInformation(
                "[{RequestId}] {Method} {Path} → {StatusCode} | {TimeMs} ms | {Size} bytes",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                context.Response.ContentLength ?? 0
            );
        }
    }
}