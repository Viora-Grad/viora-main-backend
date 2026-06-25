namespace Viora.Application.Abstractions.Media;

/// <summary>
/// defines an object that stores the content as a conversion to base 64 
/// </summary>
/// <param name="Id"></param>
/// <param name="Content"></param>
/// <param name="ContentType"></param>
/// <param name="FileName"></param>
/// <param name="CreatedAt"></param>
public record MediaResponseContent(Guid Id, string Content, string ContentType, string FileName, DateTime CreatedAt);


/// <summary>
/// minimal response with id to be used on the endpoint for media
/// </summary>
/// <param name="Id"></param>
/// <param name="ContentType"></param>
/// <param name="FileName"></param>
/// <param name="CreatedAt"></param>
public record MediaResponse(Guid Id, string ContentType, string FileName, DateTime CreatedAt);

/// <summary>
/// represents the stream object
/// </summary>
/// <param name="Content"></param>
/// <param name="ContentType"></param>
/// <param name="FileName"></param>
public record MediaResponseStream(Stream Content, string ContentType, string FileName);