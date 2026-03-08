// Services/ValidationService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using FrameworksFirstTask.Models;
using FrameworksFirstTask.Errors;

namespace FrameworksFirstTask.Services;

public static class ValidationService
{
    public static void ValidateCreateRequest(CreateTodoRequest request)
    {
        var errors = new List<string>();

        // Правило 1: Заголовок не может быть пустым
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add("Заголовок задачи не может быть пустым");
        }
        else if (request.Title.Length > 100)
        {
            errors.Add("Заголовок задачи не может превышать 100 символов");
        }

        // Правило 2: Описание не может быть слишком длинным (но может быть пустым)
        if (request.Description?.Length > 500)
        {
            errors.Add("Описание задачи не может превышать 500 символов");
        }

        // Правило 3: Дата выполнения не может быть в прошлом
        if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.UtcNow.Date)
        {
            errors.Add("Дата выполнения не может быть в прошлом");
        }

        // Правило 4: Приоритет должен быть в допустимом диапазоне
        if (!Enum.IsDefined(typeof(PriorityLevel), request.Priority))
        {
            errors.Add("Указан недопустимый приоритет");
        }

        if (errors.Any())
        {
            throw new ValidationException(string.Join("; ", errors));
        }
    }

    public static void ValidateUpdateRequest(Guid id, UpdateTodoRequest request)
    {
        var errors = new List<string>();

        // Правило 1: Заголовок не может быть пустым
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add("Заголовок задачи не может быть пустым");
        }
        else if (request.Title.Length > 100)
        {
            errors.Add("Заголовок задачи не может превышать 100 символов");
        }

        // Правило 2: Описание не может быть слишком длинным
        if (request.Description?.Length > 500)
        {
            errors.Add("Описание задачи не может превышать 500 символов");
        }

        // Правило 3: Дата выполнения не может быть в прошлом
        if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.UtcNow.Date)
        {
            errors.Add("Дата выполнения не может быть в прошлом");
        }

        // Правило 4: Приоритет должен быть в допустимом диапазоне
        if (!Enum.IsDefined(typeof(PriorityLevel), request.Priority))
        {
            errors.Add("Указан недопустимый приоритет");
        }

        if (errors.Any())
        {
            throw new ValidationException(string.Join("; ", errors));
        }
    }
}