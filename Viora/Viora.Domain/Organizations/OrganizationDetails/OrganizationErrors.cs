using Viora.Domain.Abstractions;

namespace Viora.Domain.Organizations.OrganizationDetails;

public static class OrganizationErrors
{
    public static Error OrganizationMustBeActiveToHide = new("Organizations.OrganizationMustBeActiveToHide", "The organization must be active to be hidden", ErrorCategory.Conflict);
}
