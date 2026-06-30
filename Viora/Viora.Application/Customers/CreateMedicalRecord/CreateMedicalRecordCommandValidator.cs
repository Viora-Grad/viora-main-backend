using FluentValidation;

namespace Viora.Application.Customers.CreateMedicalRecord;

internal class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
{
    public CreateMedicalRecordCommandValidator()
    {
        RuleFor(x => x.Systolic).NotEmpty().GreaterThan(0).InclusiveBetween(1, 300).WithMessage("Systolic must be between 1 and 300");
        RuleFor(x => x.Diastolic).NotEmpty().GreaterThan(0).InclusiveBetween(1, 200).WithMessage("Diastolic must be between 1 and 200");
        RuleFor(x => x.Weight).NotEmpty().GreaterThan(0).InclusiveBetween(1, 300).WithMessage("Weight must be between 1 and 300");
        RuleFor(x => x.HeartRate).NotEmpty().GreaterThan(0).InclusiveBetween(1, 200).WithMessage("Heart rate must be between 1 and 200");
        RuleFor(x => x.BloodGlucose).NotEmpty().GreaterThan(0).InclusiveBetween(1, 250).WithMessage("Blood glucose must be between 1 and 250");
    }
}
