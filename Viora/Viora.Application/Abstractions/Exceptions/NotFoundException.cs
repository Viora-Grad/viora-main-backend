namespace Viora.Application.Abstractions.Exceptions;

internal class NotFoundException : Exception
{
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    public NotFoundException(string message) : base(message) { }
}
