// Middlewares/RequestIdMiddleware.cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FrameworksFirstTask.Services;

namespace FrameworksFirstTask.Middlewares;

public sealed class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        RequestId.GetOrCreate(context);
        await _next(context);
    }
}