// Services/InMemoryTodoRepository.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FrameworksFirstTask.Models;
using FrameworksFirstTask.Errors;

namespace FrameworksFirstTask.Services;

/// <summary>
/// Хранилище задач в памяти с защитой от параллельных запросов
/// </summary>
public sealed class InMemoryTodoRepository : ITodoRepository
{
    private readonly ConcurrentDictionary<Guid, TodoItem> _items = new();
    private readonly object _lockObject = new();

    public IReadOnlyCollection<TodoItem> GetAll(bool? completed = null, PriorityLevel? priority = null, string? search = null)
    {
        var query = _items.Values.AsEnumerable();

        // Фильтрация по статусу выполнения
        if (completed.HasValue)
        {
            query = query.Where(x => x.IsCompleted == completed.Value);
        }

        // Фильтрация по приоритету
        if (priority.HasValue)
        {
            query = query.Where(x => x.Priority == priority.Value);
        }

        // Поиск по тексту (в заголовке или описании)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            query = query.Where(x => 
                x.Title.ToLowerInvariant().Contains(searchLower) || 
                x.Description.ToLowerInvariant().Contains(searchLower));
        }

        // Сортировка: сначала невыполненные, потом по приоритету (высокий сначала), потом по дате создания
        return query
            .OrderBy(x => x.IsCompleted)
            .ThenByDescending(x => x.Priority)
            .ThenByDescending(x => x.DueDate ?? DateTime.MaxValue)
            .ThenByDescending(x => x.CreatedAt)
            .ToArray();
    }

    public TodoItem? GetById(Guid id)
    {
        _items.TryGetValue(id, out var item);
        return item;
    }

    public TodoItem Create(string title, string description, PriorityLevel priority, DateTime? dueDate)
    {
        lock (_lockObject) // Защита от конфликтов при параллельном создании
        {
            // Проверка на дубликат (нельзя создать две задачи с одинаковым названием)
            if (_items.Values.Any(x => x.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ValidationException($"Задача с названием '{title}' уже существует");
            }

            var id = Guid.NewGuid();
            var item = new TodoItem
            {
                Id = id,
                Title = title.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Priority = priority,
                DueDate = dueDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = null
            };

            _items[id] = item;
            return item;
        }
    }

    public TodoItem Update(Guid id, string title, string description, PriorityLevel priority, bool isCompleted, DateTime? dueDate)
    {
        lock (_lockObject)
        {
            if (!_items.TryGetValue(id, out var existing))
            {
                throw new NotFoundException($"Задача с ID {id} не найдена");
            }

            // Проверка на дубликат названия (если название меняется)
            if (!existing.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
            {
                if (_items.Values.Any(x => x.Id != id && x.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ValidationException($"Задача с названием '{title}' уже существует");
                }
            }

            // Обновляем поля
            existing.Title = title.Trim();
            existing.Description = description?.Trim() ?? string.Empty;
            existing.Priority = priority;
            existing.DueDate = dueDate;

            // Если статус выполнения изменился
            if (existing.IsCompleted != isCompleted)
            {
                existing.IsCompleted = isCompleted;
                existing.CompletedAt = isCompleted ? DateTime.UtcNow : null;
            }

            return existing;
        }
    }

    public bool Delete(Guid id)
    {
        return _items.TryRemove(id, out _);
    }

    public bool Exists(Guid id) => _items.ContainsKey(id);

    public Dictionary<PriorityLevel, int> GetStatistics()
    {
        return _items.Values
            .GroupBy(x => x.Priority)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );
    }
}