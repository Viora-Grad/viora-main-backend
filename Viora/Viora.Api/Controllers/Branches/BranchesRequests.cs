using Viora.Domain.Branches.Internals;

namespace Viora.Api.Controllers.Branches;

public record AddBranchRequest(
    Guid OrganizationId,
    int AddressNumber,
    string AddressStreet,
    string AddressCity,
    string AddressState,
    Guid AddressCountryId,
    int AddressPostalCode,
    double Latitude,
    double Longitude,
    string ContactEmail,
    ICollection<string> ServicesProvided,
    string TimeZoneId);

public record UpdatePhoneNumbersRequest(ICollection<string> PhoneNumbers);

public record UpdateBranchStatusRequest(BranchStatus Status);

public record UpdateScheduleRequest(IEnumerable<BusinessHourRequest> Schedule);

public record BusinessHourRequest(DayOfWeek Day, TimeSpan OpenTime, TimeSpan CloseTime);
