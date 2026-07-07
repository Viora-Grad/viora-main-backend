using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.RealTimeScheduling.Internals;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;
using Viora.Infrastructure;

namespace Viora.Test.Integerations;

[TestClass]
public sealed class RealTimeSchedulingIntegrationTests
{
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ApplicationDbContext _dbContext;
    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public RealTimeSchedulingIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);
    }

    [TestCleanup]
    public void Cleanup() => _dbContext.Dispose();

    [TestMethod]
    public async Task CreateSchedule_WithShifts_PersistsScheduleAndShifts()
    {
        var branchId = Guid.NewGuid();
        var staff = Staff.Create(OrgId, FixedNow);
        staff.SetStaffProperties("Ahmed", "Ali", "ahmed_a", "hashed_pw",
            new DateOnly(1990, 3, 10), Gender.Male, "+201009876543");
        staff.Activate();
        _dbContext.Add(staff);
        await _dbContext.SaveChangesAsync();

        var schedule = Schedule.Create(branchId, DayOfWeek.Monday);
        _dbContext.Add(schedule);
        await _dbContext.SaveChangesAsync();

        var shift1 = Shift.Create(schedule.Id, new TimeOnly(9, 0), new TimeOnly(12, 0), staff.Id);
        var shift2 = Shift.Create(schedule.Id, new TimeOnly(14, 0), new TimeOnly(17, 0), staff.Id);
        _dbContext.AddRange(shift1, shift2);
        await _dbContext.SaveChangesAsync();

        var retrievedSchedule = _dbContext.ChangeTracker.Entries<Schedule>()
            .FirstOrDefault(e => e.Entity.Id == schedule.Id);
        Assert.IsNotNull(retrievedSchedule);
        Assert.AreEqual(branchId, retrievedSchedule.Entity.BranchId);
        Assert.AreEqual(DayOfWeek.Monday, retrievedSchedule.Entity.DayOfWeek);

        var trackedShifts = _dbContext.ChangeTracker.Entries<Shift>()
            .Where(e => e.Entity.ScheduleId == schedule.Id)
            .ToList();
        Assert.AreEqual(2, trackedShifts.Count);
        Assert.AreEqual(staff.Id, trackedShifts[0].Entity.StaffId);
    }

    [TestMethod]
    public async Task CancelShift_PersistsCancellationRecord()
    {
        var staff = Staff.Create(OrgId, FixedNow);
        staff.SetStaffProperties("Sara", "Hassan", "sara_h", "hashed_pw",
            new DateOnly(1992, 7, 20), Gender.Female, "+201005551234");
        staff.Activate();
        _dbContext.Add(staff);
        await _dbContext.SaveChangesAsync();

        var schedule = Schedule.Create(Guid.NewGuid(), DayOfWeek.Wednesday);
        _dbContext.Add(schedule);
        await _dbContext.SaveChangesAsync();

        var shift = Shift.Create(schedule.Id, new TimeOnly(10, 0), new TimeOnly(14, 0), staff.Id);
        _dbContext.Add(shift);
        await _dbContext.SaveChangesAsync();

        var cancellation = ScheduleCancellations.Create(shift.Id, FixedNow.AddDays(3), "Staff unavailable");
        _dbContext.Add(cancellation);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<ScheduleCancellations>()
            .FirstOrDefault(e => e.Entity.Id == cancellation.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(shift.Id, tracked.Entity.ShiftId);
        Assert.AreEqual(FixedNow.AddDays(3), tracked.Entity.CancellationDate);
        Assert.AreEqual("Staff unavailable", tracked.Entity.Reason);
    }

    [TestMethod]
    public async Task DelayAppointment_PersistsDelayRecord()
    {
        var appointmentId = Guid.NewGuid();
        var delay = ScheduleDelay.Create(appointmentId, new TimeOnly(0, 30), "Traffic delay", FixedNow.AddHours(2), InitiatorType.Client);
        _dbContext.Add(delay);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<ScheduleDelay>()
            .FirstOrDefault(e => e.Entity.Id == delay.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(appointmentId, tracked.Entity.AppointmentId);
        Assert.AreEqual(new TimeOnly(0, 30), tracked.Entity.DelayDuration);
        Assert.AreEqual("Traffic delay", tracked.Entity.Reason);
        Assert.AreEqual(InitiatorType.Client, tracked.Entity.Initiator);
    }

    [TestMethod]
    public async Task CreateSchedule_DifferentDays_PersistsEachDay()
    {
        var branchId = Guid.NewGuid();
        var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        foreach (var day in days)
        {
            var schedule = Schedule.Create(branchId, day);
            _dbContext.Add(schedule);
        }
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<Schedule>()
            .Where(e => e.Entity.BranchId == branchId)
            .ToList();
        Assert.AreEqual(5, tracked.Count);
    }

    [TestMethod]
    public async Task CreateMultipleShifts_ForSameSchedule_PersistsAllShifts()
    {
        var staff = Staff.Create(OrgId, FixedNow);
        staff.Activate();
        _dbContext.Add(staff);
        await _dbContext.SaveChangesAsync();

        var schedule = Schedule.Create(Guid.NewGuid(), DayOfWeek.Saturday);
        _dbContext.Add(schedule);
        await _dbContext.SaveChangesAsync();

        var shifts = new[]
        {
            Shift.Create(schedule.Id, new TimeOnly(8, 0), new TimeOnly(10, 0), staff.Id),
            Shift.Create(schedule.Id, new TimeOnly(10, 0), new TimeOnly(12, 0), staff.Id),
            Shift.Create(schedule.Id, new TimeOnly(14, 0), new TimeOnly(16, 0), staff.Id),
            Shift.Create(schedule.Id, new TimeOnly(16, 0), new TimeOnly(18, 0), staff.Id)
        };
        _dbContext.AddRange(shifts);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<Shift>()
            .Where(e => e.Entity.ScheduleId == schedule.Id)
            .ToList();
        Assert.AreEqual(4, tracked.Count);
    }
}
