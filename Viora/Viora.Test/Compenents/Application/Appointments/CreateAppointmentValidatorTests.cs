using FluentValidation.TestHelper;
using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Appointments.CreateAppointment;

namespace Viora.Test.Compenents.Application.Appointments;

[TestClass]
public sealed class CreateAppointmentValidatorTests
{
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly CreateAppointmentValidator _validator;
    private readonly DateTime _future = new(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc);

    public CreateAppointmentValidatorTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc));
        _validator = new CreateAppointmentValidator(_clockMock.Object);
    }

    [TestMethod]
    public void ValidCommand_PassesAllRules()
    {
        var command = new CreateAppointmentCommand(
            Guid.NewGuid(), Guid.NewGuid(), null, _future,
            "Cash", null, "Customer", "Web");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void EmptyServiceId_HasError()
    {
        var command = new CreateAppointmentCommand(
            Guid.Empty, Guid.NewGuid(), null, _future,
            "Cash", null, "Customer", "Web");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ServiceId);
    }

    [TestMethod]
    public void EmptyStaffId_HasError()
    {
        var command = new CreateAppointmentCommand(
            Guid.NewGuid(), Guid.Empty, null, _future,
            "Cash", null, "Customer", "Web");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.StaffId);
    }

    [TestMethod]
    public void PastReservationDate_HasError()
    {
        var past = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var command = new CreateAppointmentCommand(
            Guid.NewGuid(), Guid.NewGuid(), null, past,
            "Cash", null, "Customer", "Web");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ReservationDate);
    }

    [TestMethod]
    public void InvalidPaymentMethod_HasError()
    {
        var command = new CreateAppointmentCommand(
            Guid.NewGuid(), Guid.NewGuid(), null, _future,
            "Bitcoin", null, "Customer", "Web");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.PaymentMethod);
    }

    [TestMethod]
    public void InvalidCreatedBy_HasError()
    {
        var command = new CreateAppointmentCommand(
            Guid.NewGuid(), Guid.NewGuid(), null, _future,
            "Cash", null, "InvalidRole", "Web");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CreatedBy);
    }

    [TestMethod]
    public void InvalidPlatform_HasError()
    {
        var command = new CreateAppointmentCommand(
            Guid.NewGuid(), Guid.NewGuid(), null, _future,
            "Cash", null, "Customer", "Telepathy");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.RequestPlatform);
    }
}
