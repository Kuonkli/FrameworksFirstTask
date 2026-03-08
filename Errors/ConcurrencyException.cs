// Errors/ConcurrencyException.cs
namespace FrameworksFirstTask.Errors;

/// <summary>
/// Исключение для защиты от конфликтов при параллельных запросах
/// </summary>
public sealed class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message)
        : base("concurrency_error", message, 409)
    {
    }
}