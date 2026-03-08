// Models/TodoItem.cs
using System;

namespace FrameworksFirstTask.Models;

/// <summary>
/// Сущность задачи из Todo-листа
/// </summary>
public class TodoItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public PriorityLevel Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Приоритет задачи
/// </summary>
public enum PriorityLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}