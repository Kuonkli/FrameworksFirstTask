// Services/ITodoRepository.cs
using System;
using System.Collections.Generic;
using FrameworksFirstTask.Models;

namespace FrameworksFirstTask.Services;

public interface ITodoRepository
{
    IReadOnlyCollection<TodoItem> GetAll(bool? completed = null, PriorityLevel? priority = null, string? search = null);
    TodoItem? GetById(Guid id);
    TodoItem Create(string title, string description, PriorityLevel priority, DateTime? dueDate);
    TodoItem Update(Guid id, string title, string description, PriorityLevel priority, bool isCompleted, DateTime? dueDate);
    bool Delete(Guid id);
    bool Exists(Guid id);
    Dictionary<PriorityLevel, int> GetStatistics();
}