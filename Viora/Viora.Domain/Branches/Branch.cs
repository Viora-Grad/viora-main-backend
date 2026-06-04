using NetTopologySuite.Geometries;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches.Internals;
using Viora.Domain.Shared.Enums;
using Viora.Domain.Shared.Internal;

namespace Viora.Domain.Branches;

public sealed class Branch : Entity
{
    public Guid OrganizationId { get; private set; }

    public IReadOnlyCollection<ServiceType> ServicesProvided => _services.AsReadOnly();
    private readonly List<ServiceType> _services = [];

    public Address Address { get; private set; } = default!;
    public Point Location { get; private set; } = default!;

    public BranchStatus Status { get; private set; }
    public Email ContactEmail { get; private set; } = default!;

    public IReadOnlyCollection<PhoneNumber> PhoneNumbers => _phoneNumbers.AsReadOnly();
    private readonly List<PhoneNumber> _phoneNumbers = [];

    public IReadOnlyCollection<BusinessHour> BusinessHours => _businessHours.AsReadOnly();
    private readonly List<BusinessHour> _businessHours = [];

    public TimeZoneId TimeZone { get; set; } = "UTC";

    private Branch() { }    // for EfCore

    //fail safe if time could not be found to fall back to UTC
    private TimeZoneInfo ResolveTimeZone() =>
        TimeZoneInfo.TryFindSystemTimeZoneById(TimeZone, out var tz) ? tz : TimeZoneInfo.Utc;

    public Result SetBusinessHours(DayOfWeek day, TimeSpan openTime, TimeSpan closeTime)
    {
        var newHoursResult = BusinessHour.Create(day, openTime, closeTime);
        if (newHoursResult.IsFailure)
            return Result.Failure(newHoursResult.Error);

        _businessHours.RemoveAll(bh => bh.Day == day);
        _businessHours.Add(newHoursResult.Value);
        _businessHours.Sort((a, b) => a.Day.CompareTo(b.Day));

        return Result.Success();
    }

    public bool IsCurrentlyOpen(DateTime utcNow)
    {
        var timeZone = ResolveTimeZone();
        DateTime branchLocalTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
        TimeSpan timeOfDay = branchLocalTime.TimeOfDay;
        DayOfWeek dayOfWeek = branchLocalTime.DayOfWeek;

        return _businessHours.Any(bh =>
            bh.Day == dayOfWeek &&
            timeOfDay >= bh.OpenTime &&
            timeOfDay <= bh.CloseTime);
    }
}
