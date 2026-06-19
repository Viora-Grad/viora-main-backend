using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Api.Controllers.Oganizations;

// TODO adjust these fields to be extracted from HTTP context after auth is done and mark them as NotMapped

public record SuspendOrganizationRequest(
    Guid? SuspendedById,
    string Reason,
    string Notes);

