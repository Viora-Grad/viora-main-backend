using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Events;
using Viora.Domain.Appointments.Internal;

namespace Viora.Test.Compenents.Domain.Appointments;

[TestClass]
public sealed class AppointmentTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid ServiceId = Guid.NewGuid();
    private static readonly Guid StaffId = Guid.NewGuid();
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly DateTime BaseReservation = new(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);

    // ===== Book =====

    [TestMethod]
    public void Book_ValidInput_SetsAllFields()
    {
        DateTime createdAt = new(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Wallet, null,
            Creator.Customer, Platform.Web, 30, createdAt);

        Assert.IsNotNull(appointment);
        Assert.AreEqual(CustomerId, appointment.CustomerId);
        Assert.AreEqual(ServiceId, appointment.ServiceId);
        Assert.AreEqual(StaffId, appointment.StaffId);
        Assert.AreEqual(BranchId, appointment.BranchId);
        Assert.AreEqual(BaseReservation, appointment.ReservationDate);
        Assert.AreEqual(1, appointment.AppointmentQueueNumber);
        Assert.AreEqual(PaymentMethod.Wallet, appointment.PayMethod);
        Assert.AreEqual(CustomerStatus.NotArrived, appointment.Status);
        Assert.AreEqual(Creator.Customer, appointment.CreatedBy);
        Assert.AreEqual(Platform.Web, appointment.RequestPlatform);
        Assert.AreEqual(30, appointment.EstimatedDurationMinutes);
        Assert.AreEqual(createdAt, appointment.CreatedAt);
        Assert.IsFalse(appointment.IsCheckedIn);
    }

    [TestMethod]
    public void Book_WithNullCustomerId_AllowsAnonymousBooking()
    {
        DateTime createdAt = DateTime.UtcNow;

        Appointment appointment = Appointment.Book(
            null, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Staff, Platform.Mobile, 30, createdAt);

        Assert.IsNull(appointment.CustomerId);
    }

    [TestMethod]
    public void Book_RaisesAppointmentBookedEvent()
    {
        DateTime createdAt = DateTime.UtcNow;

        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Online, null,
            Creator.Customer, Platform.Web, 30, createdAt);

        Assert.AreEqual(1, appointment.DomainEvents.Count);
        Assert.IsInstanceOfType(appointment.DomainEvents.Single(), typeof(AppointmentBookedEvent));
    }

    [TestMethod]
    public void Book_CustomerStatusOverride_AppliesProvidedStatus()
    {
        DateTime createdAt = DateTime.UtcNow;

        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash,
            CustomerStatus.Waiting, Creator.Customer, Platform.Web, 30, createdAt);

        Assert.AreEqual(CustomerStatus.Waiting, appointment.Status);
    }

    // ===== CheckIn =====

    [TestMethod]
    public void CheckIn_CustomerWithinWindow_ReturnsSuccessAndUpdatesStatus()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime checkInTime = BaseReservation.AddMinutes(-15);

        Result result = appointment.CheckIn(checkInTime, Creator.Customer);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.InProgress, appointment.Status);
        Assert.IsTrue(appointment.IsCheckedIn);
        Assert.AreEqual(checkInTime, appointment.LastUpdatedAt);
    }

    [TestMethod]
    public void CheckIn_CustomerTooEarly_ReturnsFailure()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime checkInTime = BaseReservation.AddMinutes(-31);

        Result result = appointment.CheckIn(checkInTime, Creator.Customer);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CheckInNotWithinAcceptableWindow, result.Error);
        Assert.IsFalse(appointment.IsCheckedIn);
    }

    [TestMethod]
    public void CheckIn_CustomerAlreadyCheckedIn_ReturnsFailure()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);

        Result result = appointment.CheckIn(BaseReservation.AddMinutes(-10), Creator.Customer);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CheckInProhibited, result.Error);
    }

    [TestMethod]
    public void CheckIn_ByStaff_BypassesWindowCheck()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime checkInTime = BaseReservation.AddMinutes(-60);

        Result result = appointment.CheckIn(checkInTime, Creator.Staff);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.InProgress, appointment.Status);
        Assert.IsTrue(appointment.IsCheckedIn);
    }

    // ===== Complete =====

    [TestMethod]
    public void Complete_ValidInProgress_ReturnsSuccess()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);
        DateTime completeTime = BaseReservation.AddMinutes(25);

        Result result = appointment.Complete(completeTime);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.Completed, appointment.Status);
        Assert.AreEqual(completeTime, appointment.LastUpdatedAt);
    }

    [TestMethod]
    public void Complete_WhenAlreadyCompleted_ReturnsFailure()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);
        appointment.Complete(BaseReservation.AddMinutes(25));

        Result result = appointment.Complete(BaseReservation.AddMinutes(30));

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CompleteProhibited, result.Error);
    }

    [TestMethod]
    public void Complete_TooEarly_ReturnsFailure()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);
        DateTime tooEarly = BaseReservation.AddHours(-2);

        Result result = appointment.Complete(tooEarly);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CompleteProhibited, result.Error);
    }

    [TestMethod]
    public void Complete_RaisesAppointmentCompletedEvent()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);
        appointment.ClearDomainEvents();
        DateTime completeTime = BaseReservation.AddMinutes(25);

        appointment.Complete(completeTime);

        Assert.AreEqual(1, appointment.DomainEvents.Count);
        Assert.IsInstanceOfType(appointment.DomainEvents.Single(), typeof(AppointmentCompletedEvent));
    }

    // ===== Cancel =====

    [TestMethod]
    public void Cancel_ByCustomerOutsideWindow_ReturnsSuccess()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime cancelTime = BaseReservation.AddHours(-3);

        Result result = appointment.Cancel(cancelTime, BranchId, Creator.Customer);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.Canceled, appointment.Status);
        Assert.AreEqual(cancelTime, appointment.LastUpdatedAt);
    }

    [TestMethod]
    public void Cancel_ByCustomerInsideTwoHourWindow_ReturnsFailure()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime cancelTime = BaseReservation.AddHours(-1);

        Result result = appointment.Cancel(cancelTime, BranchId, Creator.Customer);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CancellationProhibited, result.Error);
    }

    [TestMethod]
    public void Cancel_ByStaff_AlwaysAllowed()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime cancelTime = BaseReservation.AddMinutes(-30);

        Result result = appointment.Cancel(cancelTime, BranchId, Creator.Staff);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.Canceled, appointment.Status);
    }

    [TestMethod]
    public void Cancel_WhenCompleted_ReturnsFailure()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);
        appointment.Complete(BaseReservation.AddMinutes(25));

        Result result = appointment.Cancel(DateTime.UtcNow, BranchId, Creator.Staff);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CancellationProhibited, result.Error);
    }

    // ===== NoShow =====

    [TestMethod]
    public void NoShow_NotArrivedAfterReservation_ReturnsSuccess()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime noShowTime = BaseReservation.AddHours(1);

        Result result = appointment.NoShow(noShowTime);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.NoShow, appointment.Status);
        Assert.AreEqual(noShowTime, appointment.LastUpdatedAt);
    }

    [TestMethod]
    public void NoShow_WhenAlreadyCheckedIn_ReturnsFailure()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);

        Result result = appointment.NoShow(BaseReservation.AddHours(1));

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.NoShowProhibited, result.Error);
    }

    [TestMethod]
    public void NoShow_TimeBeforeReservation_ReturnsFailure()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);

        Result result = appointment.NoShow(BaseReservation.AddHours(-1));

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.NoShowTimeInvalid, result.Error);
    }

    [TestMethod]
    public void NoShow_RaisesAppointmentNoShowEvent()
    {
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.ClearDomainEvents();

        appointment.NoShow(BaseReservation.AddHours(1));

        Assert.AreEqual(1, appointment.DomainEvents.Count);
        Assert.IsInstanceOfType(appointment.DomainEvents.Single(), typeof(AppointmentNoShowEvent));
    }
}
