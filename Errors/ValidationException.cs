// Errors/ValidationException.cs
namespace FrameworksFirstTask.Errors;

public sealed class ValidationException : DomainException
{
    public ValidationException(string message)
        : base("validation_error", message, 400)
    {
    }
}