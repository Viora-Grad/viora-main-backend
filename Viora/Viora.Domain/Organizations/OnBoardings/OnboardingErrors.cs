using Viora.Domain.Abstractions;

namespace Viora.Domain.Organizations.OnBoardings;

public static class OnboardingErrors
{
    public static Error AlreadyExpired => new("Onboarding.AlreadyExpired", "The onboarding application has already expired.", ErrorCategory.Forbidden);
    public static Error StatusNotPending => new("Onboarding.StatusNotPending", "Only pending applications can be marked as completed.", ErrorCategory.Conflict);
    public static Error OwnerHasOrganizationRequest => new("Onboarding.OwnerHasOrganizationRequest", "The owner already has a pending organization application.", ErrorCategory.Conflict);
    public static Error OwnerHasOrganization => new("Onboarding.OwnerHasOrganization", "The owner already has an organization.", ErrorCategory.Conflict);
    public static Error IsInCoolDownPeriod(DateTime NextAttemptDate) =>
        new("Onboarding.IsInCoolDownPeriod",
        $"The latest application is still in the cool-down period.\n Please try again on ${NextAttemptDate}.", ErrorCategory.Conflict);
    public static Error NameAlreadyTaken => new("Onboarding.NameAlreadyTaken", "The proposed organization name is already taken by another organization or pending application.", ErrorCategory.Conflict);
}
