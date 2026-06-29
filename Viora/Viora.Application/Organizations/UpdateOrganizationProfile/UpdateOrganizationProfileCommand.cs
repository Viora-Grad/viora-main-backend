using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.UpdateOrganizationProfile;

public sealed record UpdateOrganizationProfileCommand(
    Guid OrganizationId,
    string SubDomain,
    string SupportEmail,
    string BillingEmail,
    string ServiceDescription,
    IReadOnlyList<string> ServicesProvided,
    string About) : ICommand;
