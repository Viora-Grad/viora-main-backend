using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Branches.GetBranchGalleryImage;

/// <summary>
/// Streams a single gallery image for a branch. The image is addressed through its owning
/// branch and the handler verifies the media actually belongs to that branch's gallery, so
/// the endpoint can never be used to fetch arbitrary media by id.
/// </summary>
/// <param name="BranchId">The branch that owns the gallery.</param>
/// <param name="MediaId">The image within that branch's gallery.</param>
public sealed record GetBranchGalleryImageQuery(Guid BranchId, Guid MediaId)
    : IQuery<MediaResponseStream>;
