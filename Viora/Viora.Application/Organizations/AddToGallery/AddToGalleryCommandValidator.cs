using FluentValidation;

namespace Viora.Application.Organizations.AddToGallery;

internal sealed class AddToGalleryCommandValidator : AbstractValidator<AddToGalleryCommand>
{
    private const int MaxBatchSize = 20;

    public AddToGalleryCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization id cannot be empty.");

        RuleFor(x => x.Medias)
            .NotEmpty()
            .WithMessage("At least one media item is required.")
            .Must(m => m.Count <= MaxBatchSize)
            .WithMessage($"A maximum of {MaxBatchSize} media items can be uploaded per request.");
    }
}
