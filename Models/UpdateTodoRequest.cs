using System;

namespace FrameworksFirstTask.Models;

/// <summary>
/// Запрос на обновление задачи
/// </summary>
public record UpdateTodoRequest(
    string Title,
    string Description,
    PriorityLevel Priority,
    bool IsCompleted,
    DateTime? DueDate = null);