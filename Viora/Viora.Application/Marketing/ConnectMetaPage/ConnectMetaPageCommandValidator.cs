using FluentValidation;

namespace Viora.Application.Marketing.ConnectMetaPage;

internal sealed class ConnectMetaPageCommandValidator : AbstractValidator<ConnectMetaPageCommand>
{
    public ConnectMetaPageCommandValidator()
    {
        RuleFor(x => x.AuthCode).NotEmpty().WithMessage("The Facebook authorization token is required.");
        RuleFor(x => x.PageId).NotEmpty().WithMessage("Facebook Page id is required.").MaximumLength(100);
    }
}
