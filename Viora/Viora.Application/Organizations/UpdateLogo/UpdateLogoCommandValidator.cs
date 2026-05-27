using FluentValidation;
using System.Collections.Frozen;

namespace Viora.Application.Organizations.UpdateLogo;

internal class UpdateLogoCommandValidator : AbstractValidator<UpdateLogoCommand>
{
    private static readonly FrozenSet<string> AllowedMimeTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    private const long MaxLogoSizeInBytes = 5 * 1024 * 1024;

    public UpdateLogoCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();

        RuleFor(x => x.FileName).NotEmpty();

        RuleFor(x => x.SizeInBytes)
            .GreaterThan(0).WithMessage("File must not be empty.")
            .LessThanOrEqualTo(MaxLogoSizeInBytes).WithMessage("Logo must not exceed 5 MB.");

        RuleFor(x => x.MimeType)
            .Must(m => AllowedMimeTypes.Contains(m))
            .WithMessage("Logo must be a JPEG, PNG, GIF, or WebP image.");
    }
}
