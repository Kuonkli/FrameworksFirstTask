using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using FrameworksFirstTask.Models;
using FrameworksFirstTask.Errors;
using FrameworksFirstTask.Middlewares;
using FrameworksFirstTask.Services;

var builder = WebApplication.CreateBuilder(args);

// Настройка JSON
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNamingPolicy = null; // сохраняем имена как в C#
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); // для enum
});

// Регистрация сервисов
builder.Services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();

var app = builder.Build();

// Конвейер обработки (порядок важен!)
app.UseMiddleware<RequestIdMiddleware>();      // 1. Устанавливаем ID запроса
app.UseMiddleware<ErrorHandlingMiddleware>();  // 2. Ловим все ошибки
app.UseMiddleware<TimingAndLogMiddleware>();   // 3. Логируем время

// GET /api/todos - список всех задач с фильтрацией
app.MapGet("/api/todos", async (
    ITodoRepository repo,
    bool? completed = null,
    PriorityLevel? priority = null,
    string? search = null) =>
{
    // Имитация асинхронности
    await Task.Delay(10);
    
    var items = repo.GetAll(completed, priority, search);
    return Results.Ok(items);
});

// GET /api/todos/{id} - получение задачи по ID
app.MapGet("/api/todos/{id:guid}", async (
    Guid id,
    ITodoRepository repo) =>
{
    await Task.Delay(5);
    
    var item = repo.GetById(id);
    if (item is null)
        throw new NotFoundException($"Задача с ID {id} не найдена");

    return Results.Ok(item);
});

// POST /api/todos - создание новой задачи
app.MapPost("/api/todos", async (
    HttpContext context,
    CreateTodoRequest request,
    ITodoRepository repo) =>
{
    // Валидация входных данных
    ValidationService.ValidateCreateRequest(request);

    var created = repo.Create(
        request.Title,
        request.Description ?? string.Empty,
        request.Priority,
        request.DueDate
    );

    var location = $"/api/todos/{created.Id}";
    context.Response.Headers.Location = location;

    return Results.Created(location, created);
});

// PUT /api/todos/{id} - обновление задачи
app.MapPut("/api/todos/{id:guid}", async (
    Guid id,
    UpdateTodoRequest request,
    ITodoRepository repo) =>
{
    // Валидация входных данных
    ValidationService.ValidateUpdateRequest(id, request);

    var updated = repo.Update(
        id,
        request.Title,
        request.Description ?? string.Empty,
        request.Priority,
        request.IsCompleted,
        request.DueDate
    );

    return Results.Ok(updated);
});

// DELETE /api/todos/{id} - удаление задачи
app.MapDelete("/api/todos/{id:guid}", (
    Guid id,
    ITodoRepository repo) =>
{
    var deleted = repo.Delete(id);
    
    if (!deleted)
        throw new NotFoundException($"Задача с ID {id} не найдена");

    return Results.NoContent();
});

// PATCH /api/todos/{id}/toggle - переключение статуса выполнения
app.MapPatch("/api/todos/{id:guid}/toggle", (
    Guid id,
    ITodoRepository repo) =>
{
    var item = repo.GetById(id);
    if (item is null)
        throw new NotFoundException($"Задача с ID {id} не найдена");

    var updated = repo.Update(
        id,
        item.Title,
        item.Description,
        item.Priority,
        !item.IsCompleted,
        item.DueDate
    );

    return Results.Ok(updated);
});

// GET /api/todos/stats - статистика по задачам
app.MapGet("/api/todos/stats", (ITodoRepository repo) =>
{
    var items = repo.GetAll();
    var stats = repo.GetStatistics();
    
    return Results.Ok(new
    {
        TotalCount = items.Count,
        CompletedCount = items.Count(x => x.IsCompleted),
        PendingCount = items.Count(x => !x.IsCompleted),
        CompletionRate = items.Any() 
            ? Math.Round((double)items.Count(x => x.IsCompleted) / items.Count() * 100, 2)
            : 0,
        ByPriority = stats,
        OverdueCount = items.Count(x => !x.IsCompleted && x.DueDate < DateTime.UtcNow)
    });
});

app.Run();

// Нужен для тестов
public partial class Program { }