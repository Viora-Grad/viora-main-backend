using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.UpdateLogo;

public record UpdateLogoCommand(
    Guid OrganizationId,
    Stream FileStream,
    string FileName,
    string MimeType,
    long SizeInBytes) : ICommand;