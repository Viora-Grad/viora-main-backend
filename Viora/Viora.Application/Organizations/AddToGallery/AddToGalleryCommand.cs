using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Plans.Features;

namespace Viora.Application.Organizations.AddToGallery;

public sealed record AddToGalleryCommand(Guid OrganizationId, IReadOnlyList<MediaRequest> Medias) : ILimitedFeatureCommand
{
    public Guid LimitedFeatureId { get; init; } = LimitedFeature.StorageBytes.Id;
    public long DeltaChange { get; init; } = Medias.Sum(x => x.SizeBytes);
}