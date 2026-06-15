using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Branches.GetBranchGallery;

public sealed record GetBranchGalleryQuery(Guid BranchId) : IQuery<List<MediaResponse>>;