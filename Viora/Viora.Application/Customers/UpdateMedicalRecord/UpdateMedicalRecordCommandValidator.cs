using FluentValidation;

namespace Viora.Application.Customers.UpdateMedicalRecord;

internal class UpdateMedicalRecordCommandValidator : AbstractValidator<UpdateMedicalRecordCommand>
{
    public UpdateMedicalRecordCommandValidator()
    {
        RuleFor(x => x.Systolic).InclusiveBetween(1, 300).When(x => x.Systolic.HasValue).WithMessage("Systolic must be between 1 and 300");
        RuleFor(x => x.Diastolic).InclusiveBetween(1, 200).When(x => x.Diastolic.HasValue).WithMessage("Diastolic must be between 1 and 200");
        RuleFor(x => x.Weight).InclusiveBetween(1, 300).When(x => x.Weight.HasValue).WithMessage("Weight must be between 1 and 300");
        RuleFor(x => x.HeartRate).InclusiveBetween(1, 200).When(x => x.HeartRate.HasValue).WithMessage("Heart rate must be between 1 and 200");
        RuleFor(x => x.BloodGlucose).InclusiveBetween(1, 250).When(x => x.BloodGlucose.HasValue).WithMessage("Blood glucose must be between 1 and 250");

        RuleFor(x => x).Must(x => x.Systolic.HasValue == x.Diastolic.HasValue).WithMessage("Both Systolic and Diastolic must be provided together or not at all");
    }
}
