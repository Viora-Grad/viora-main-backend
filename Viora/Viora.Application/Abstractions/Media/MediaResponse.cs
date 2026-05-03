namespace Viora.Application.Abstractions.Media;

public record MediaResponse(Stream Content, string ContentType, string FileName);