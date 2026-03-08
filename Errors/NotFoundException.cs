// Errors/NotFoundException.cs
namespace FrameworksFirstTask.Errors;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base("not_found", message, 404)
    {
    }
}