namespace Viora.Application.Abstractions.Exceptions;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
    public ConcurrencyException(string message) : base(message) { }
}
