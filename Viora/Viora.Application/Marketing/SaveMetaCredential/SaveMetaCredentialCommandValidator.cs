using FluentValidation;

namespace Viora.Application.Marketing.SaveMetaCredential;

internal sealed class SaveMetaCredentialCommandValidator : AbstractValidator<SaveMetaCredentialCommand>
{
    public SaveMetaCredentialCommandValidator()
    {
        RuleFor(x => x.PageId).NotEmpty().WithMessage("Facebook Page id is required.").MaximumLength(100);
        RuleFor(x => x.AccessToken).NotEmpty().WithMessage("Facebook Page access token is required.");
    }
}
