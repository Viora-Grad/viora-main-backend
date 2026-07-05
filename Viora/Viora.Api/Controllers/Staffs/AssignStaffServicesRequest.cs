namespace Viora.Api.Controllers.Staffs;

public sealed record AssignStaffServicesRequest(
    List<Guid> ServiceIds
);