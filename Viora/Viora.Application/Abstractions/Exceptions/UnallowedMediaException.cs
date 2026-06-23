namespace Viora.Application.Abstractions.Exceptions;

public sealed class UnallowedMediaException : ArgumentException
{
    public UnallowedMediaException(string message) : base(message) { }
    public UnallowedMediaException(string message, Exception innerException) : base(message, innerException) { }
    public UnallowedMediaException(string message, string paramName) : base(message, paramName) { }

}
