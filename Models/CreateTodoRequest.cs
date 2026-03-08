using System;

namespace FrameworksFirstTask.Models;

/// <summary>
/// Запрос на создание задачи
/// </summary>
public record CreateTodoRequest(
    string Title,
    string Description,
    PriorityLevel Priority,
    DateTime? DueDate = null);