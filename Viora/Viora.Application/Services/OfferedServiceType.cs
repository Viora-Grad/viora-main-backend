using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Services;
using Viora.Domain.Shared;

namespace Viora.Application.Services;

/// <summary>
/// Shared validation used by the add/update service handlers: a service's specialty must be a
/// recognized <see cref="ServiceType"/> AND one of the specialties the owning organization declares
/// it provides (<see cref="Organization.ServicesProvided"/>).
/// </summary>
internal static class OfferedServiceType
{
    public static Result<ServiceType> Resolve(string rawServiceType, Organization organization)
    {
        var serviceType = ServiceType.All
            .FirstOrDefault(type => type.Value.Equals(rawServiceType, StringComparison.OrdinalIgnoreCase));

        if (serviceType is null)
            return Result.Failure<ServiceType>(ServiceErrors.UnknownServiceType);

        if (!organization.ServicesProvided.Contains(serviceType))
            return Result.Failure<ServiceType>(ServiceErrors.ServiceTypeNotOfferedByOrganization);

        return Result.Success(serviceType);
    }
}
