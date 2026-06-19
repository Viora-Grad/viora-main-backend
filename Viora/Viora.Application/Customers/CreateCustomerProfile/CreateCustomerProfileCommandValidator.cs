using FluentValidation;
using Viora.Domain.Shared.Internal;

namespace Viora.Application.Customers.CreateCustomerProfile;

internal class CreateCustomerProfileCommandValidator : AbstractValidator<CreateCustomerProfileCommand>
{
    public CreateCustomerProfileCommandValidator()
    {
        RuleFor(command => command.PhoneNumbers)
            .NotEmpty()
            .WithMessage("at least one phone number is required.");

        RuleForEach(command => command.PhoneNumbers)
            .Must(phone => !string.IsNullOrWhiteSpace(new PhoneNumber(phone).Value))
            .WithMessage("each phone number must be in E.164 format.")
            .When(command => command.PhoneNumbers != null && command.PhoneNumbers.Any());

        RuleForEach(command => command.Emails)
            .EmailAddress()
            .WithMessage("each email must be a valid email address.")
            .When(command => command.Emails != null && command.Emails.Any());


    }
}
