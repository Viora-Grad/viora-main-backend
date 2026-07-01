using FluentValidation;
using Viora.Application.Abstractions.Clock;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs.Internal;

namespace Viora.Application.Staffs.UpdateStaffInfo;

internal class UpdateStaffInfoCommandValidator : AbstractValidator<UpdateStaffInfoCommand>
{
    private readonly IDateTimeProvider _clock;
    public UpdateStaffInfoCommandValidator(IDateTimeProvider clock)
    {
        _clock = clock;

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("StaffId is required.");
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("FirstName cannot exceed 50 characters.");
        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("LastName cannot exceed 50 characters.");
        RuleFor(x => x.Username)
            .MaximumLength(20).WithMessage("Username cannot exceed 20 characters.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(_clock.UtcNow)).WithMessage("DateOfBirth must be in the past.");

        RuleFor(x => x.Gender)
            .Must(g => g is null || Enum.TryParse<Gender>(g, true, out _)).WithMessage("Invalid gender value.");

        RuleFor(x => x.PhoneNumber)
            .Must(p => p is null || !string.IsNullOrEmpty(new PhoneNumber(p))).WithMessage("Invalid phone number format.");

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character (e.g., !, @, #, $, etc.).");


    }
}
