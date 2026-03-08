// Models/ErrorResponse.cs
using System;

namespace FrameworksFirstTask.Models;

/// <summary>
/// Единый формат ошибки для клиентов
/// </summary>
public record ErrorResponse(
    string Code,
    string Message,
    string RequestId,
    DateTime Timestamp);