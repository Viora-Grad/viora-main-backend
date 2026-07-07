using Moq;
using System.Text.Json;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Forms.SubmitFormAnswer;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Branches;
using Viora.Domain.Forms;
using Viora.Domain.Services;
using Viora.Domain.Shared;

namespace Viora.Test.Compenents.Application.Forms;

/// <summary>
/// Unit tests for the SubmitFormAnswerCommandHandler covering successful submission, duplicate submission, and not-found scenarios.
/// </summary>
[TestClass]
public sealed class SubmitFormAnswerCommandHandlerTests
{
    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IFormRepository> _formRepoMock = new();
    private readonly Mock<IFormSubmissionRepository> _formSubmissionRepoMock = new();
    private readonly Mock<IServiceRepository> _serviceRepoMock = new();
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly SubmitFormAnswerCommandHandler _handler;

    public SubmitFormAnswerCommandHandlerTests()
    {
        _handler = new SubmitFormAnswerCommandHandler(
            _appointmentRepoMock.Object,
            _unitOfWorkMock.Object,
            _formRepoMock.Object,
            _formSubmissionRepoMock.Object,
            _serviceRepoMock.Object,
            _branchRepoMock.Object,
            _dateTimeProviderMock.Object);
    }

    // ===== Handle =====

    [TestMethod]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        Guid appointmentId = Guid.NewGuid();
        Guid formId = Guid.NewGuid();
        var submission = JsonDocument.Parse("{}");

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new SubmitFormAnswerCommand(appointmentId, formId, submission), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_FormNotFound_ThrowsNotFoundException()
    {
        Guid appointmentId = Guid.NewGuid();
        Guid formId = Guid.NewGuid();
        var submission = JsonDocument.Parse("{}");

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestAppointment(appointmentId));
        _formRepoMock.Setup(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Form?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new SubmitFormAnswerCommand(appointmentId, formId, submission), CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_DuplicateSubmission_ReturnsFailure()
    {
        Guid appointmentId = Guid.NewGuid();
        Guid formId = Guid.NewGuid();
        var submission = JsonDocument.Parse("{}");
        var form = CreateTestForm(formId);
        var existingSubmission = CreateTestFormSubmission(appointmentId, formId);

        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestAppointment(appointmentId));
        _formRepoMock.Setup(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(form);
        _serviceRepoMock.Setup(r => r.GetByIdAsync(form.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestService(form.ServiceId));
        _branchRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestBranch());
        _formSubmissionRepoMock.Setup(r => r.GetByAppointmentIdAsync(appointmentId, formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubmission);

        var result = await _handler.Handle(
            new SubmitFormAnswerCommand(appointmentId, formId, submission), CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FormSubmissionError.AlreadySubmit.Name, result.Error.Name);
    }

    [TestMethod]
    public async Task Handle_ValidSubmission_CreatesSubmission()
    {
        Guid appointmentId = Guid.NewGuid();
        Guid formId = Guid.NewGuid();
        var submission = JsonDocument.Parse("{\"questions\":[]}");
        var form = CreateTestForm(formId);

        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
        _appointmentRepoMock.Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestAppointment(appointmentId));
        _formRepoMock.Setup(r => r.GetByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(form);
        _serviceRepoMock.Setup(r => r.GetByIdAsync(form.ServiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestService(form.ServiceId));
        _branchRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestBranch());
        _formSubmissionRepoMock.Setup(r => r.GetByAppointmentIdAsync(appointmentId, formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FormSubmission?)null);
        _unitOfWorkMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new SubmitFormAnswerCommand(appointmentId, formId, submission), CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _formSubmissionRepoMock.Verify(r => r.Add(It.IsAny<FormSubmission>()), Times.Once);
    }

    // ===== Helpers =====

    private static Appointment CreateTestAppointment(Guid id)
    {
        return Appointment.Book(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            DateTime.UtcNow.AddDays(1), 30,
            Viora.Domain.Appointments.Internal.PaymentMethod.Cash,
            null,
            Viora.Domain.Appointments.Internal.Creator.Customer,
            Viora.Domain.Appointments.Internal.Platform.Web,
            30, DateTime.UtcNow);
    }

    private static Form CreateTestForm(Guid formId)
    {
        return Form.Create(Guid.NewGuid(), null, "TestForm", JsonDocument.Parse("{}")).Value;
    }

    private static FormSubmission CreateTestFormSubmission(Guid appointmentId, Guid formId)
    {
        return FormSubmission.Create(appointmentId, formId, JsonDocument.Parse("{}"), DateTime.UtcNow).Value;
    }

    private static Service CreateTestService(Guid serviceId)
    {
        var result = Service.Create(
            Guid.NewGuid(), "TestService", "Description", 30,
            ServiceType.InternalMedicine,
            new Money(100m, Currency.Usd), new TestServiceSettings());
        return result.Value;
    }

    private static Branch CreateTestBranch()
    {
        var result = Branch.Create(
            Guid.NewGuid(),
            new Viora.Domain.Shared.Internal.Address(1, "123 St", "City", "State", Guid.NewGuid(), 12345),
            new NetTopologySuite.Geometries.Point(0, 0) { SRID = 4326 },
            "test@example.com",
            new List<ServiceType> { ServiceType.InternalMedicine },
            DateTime.UtcNow);
        return result.Value;
    }

    private sealed class TestServiceSettings : IServiceSettings
    {
        public int SlotSizeInMinutes { get; set; } = 15;
        public int MinimumDurationInMinutes { get; set; } = 15;
        public int MaximumDurationInMinutes { get; set; } = 480;
        public int MaxGallerySize { get; set; } = 10;
    }
}
