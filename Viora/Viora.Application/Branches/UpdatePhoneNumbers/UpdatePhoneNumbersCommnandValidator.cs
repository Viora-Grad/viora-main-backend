using FluentValidation;

namespace Viora.Application.Branches.UpdatePhoneNumbers;

internal class UpdatePhoneNumbersCommnandValidator : AbstractValidator<UpdatePhoneNumbersCommand>
{
    public UpdatePhoneNumbersCommnandValidator()
    {
        RuleFor(x => x.PhoneNumbers)
            .NotEmpty();
    }
}
