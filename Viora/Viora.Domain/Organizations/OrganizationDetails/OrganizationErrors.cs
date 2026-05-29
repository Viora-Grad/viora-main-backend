using Viora.Domain.Abstractions;

namespace Viora.Domain.Organizations.OrganizationDetails;

public static class OrganizationErrors
{
    public static readonly Error OrganizationMustBeActiveToHide = new("Organizations.OrganizationMustBeActiveToHide", "The organization must be active to be hidden", ErrorCategory.Conflict);
    public static readonly Error OrganizationMustBeActiveToUpdateLogo = new("Organizations.OrganizationMustBeActiveToUpdateLogo", "The organization must be active to update its logo", ErrorCategory.Conflict);
    public static readonly Error LogoMustBeAnImage = new("Organizations.LogoMustBeAnImage", "The uploaded media must be an image", ErrorCategory.Validation);
}
