namespace Viora.Application.Branches.GetBranchGalleryImage;

/// <summary>
/// Carries an open read stream for a branch gallery image together with the metadata needed
/// to serve it. The caller is responsible for disposing <see cref="Content"/> (the
/// framework's FileStreamResult does this automatically).
/// </summary>
public sealed record BranchGalleryImageResponse(Stream Content, string ContentType, string FileName);
