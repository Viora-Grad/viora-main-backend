using Viora.Domain.Abstractions;

namespace Viora.Domain.Organizations.OrganizationDetails;

public static class OrganizationErrors
{
    public static readonly Error OrganizationMustBeActiveToHide = new("Organizations.OrganizationMustBeActiveToHide", "The organization must be active to be hidden", ErrorCategory.Conflict);
    public static readonly Error OrganizationMustBeActiveToUpdateLogo = new("Organizations.OrganizationMustBeActiveToUpdateLogo", "The organization must be active to update its logo", ErrorCategory.Conflict);
    public static readonly Error LogoMustBeAnImage = new("Organizations.LogoMustBeAnImage", "The uploaded media must be an image", ErrorCategory.Validation);
    public static readonly Error RatingOutOfBound = new("Organizations.RatingOutOfBound", "The rating must be between 0 and 10", ErrorCategory.Validation);
    public static readonly Error SubDomainTaken = new("Organizations.SubDomainTaken", "The subdomain is already in use by another organization", ErrorCategory.Conflict);
    public static readonly Error InvalidSubDomain = new("Organizations.InvalidSubDomain", "The subdomain must not be empty or contain spaces", ErrorCategory.Validation);
    public static readonly Error NoServicesProvided = new("Organizations.NoServicesProvided", "At least one service must be provided", ErrorCategory.Validation);
}
