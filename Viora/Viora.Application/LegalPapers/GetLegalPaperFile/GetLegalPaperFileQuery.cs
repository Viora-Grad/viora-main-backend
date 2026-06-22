using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.LegalPapers.GetLegalPaperFile;

/// <summary>
/// Streams the file behind a legal paper. Access is keyed by the legal paper (a domain
/// resource with known access rules) rather than the raw media id, so the underlying
/// media is never exposed through a generic by-id endpoint.
/// </summary>
/// <param name="LegalPaperId">The legal paper whose attachment is requested.</param>
/// <param name="RequesterId">The currently authenticated user.</param>
/// <param name="IsPrivileged">True when the caller may read any application's papers (e.g. an admin reviewer).</param>
public sealed record GetLegalPaperFileQuery(Guid LegalPaperId, Guid RequesterId, bool IsPrivileged)
    : IQuery<LegalPaperFileResponse>;
