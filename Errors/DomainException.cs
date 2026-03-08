// Errors/DomainException.cs
using System;

namespace FrameworksFirstTask.Errors;

public abstract class DomainException : Exception
{
    protected DomainException(string code, string message, int statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public string Code { get; }
    public int StatusCode { get; }
}