using FluentValidation;
using Viora.Application.Abstractions.Clock;

namespace Viora.Application.LegalPapers.AddLegalPaper;

internal class AddLegalPaperCommandValidator : AbstractValidator<AddLegalPaperCommand>
{
    private static readonly string[] AllowedContentTypes =
        ["application/pdf", "image/png", "image/jpeg", "image/webp"];

    public AddLegalPaperCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.ExpiryDateUtc)
            .Must(date => date > dateTimeProvider.UtcNow)
            .WithMessage("Expiry date must be in the future.");

        RuleFor(x => x.OfficalName)
            .NotEmpty()
            .WithMessage("Name must have value.")
            .MaximumLength(50)
            .WithMessage("Max size for name is 50.");

        RuleFor(x => x.MediaContent)
            .NotNull()
            .WithMessage("Media content is required.");

        RuleFor(x => x.MediaContent.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Legal paper must be one of: {string.Join(", ", AllowedContentTypes)}.")
            .When(x => x.MediaContent is not null);
    }
}
