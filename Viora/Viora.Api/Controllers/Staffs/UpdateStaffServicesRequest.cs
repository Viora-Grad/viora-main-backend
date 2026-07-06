namespace Viora.Api.Controllers.Staffs;

public sealed record UpdateStaffServicesRequest(
    List<Guid> ServiceIds
);