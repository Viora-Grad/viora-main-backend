namespace Viora.Application.LegalPapers.GetLegalPaperFile;

/// <summary>
/// Carries an open read stream for the legal paper's file together with the metadata
/// needed to serve it. The caller is responsible for disposing <see cref="Content"/>
/// (the framework's FileStreamResult does this automatically).
/// </summary>
public sealed record LegalPaperFileResponse(Stream Content, string ContentType, string FileName);
