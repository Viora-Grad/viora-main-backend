using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Shared.Enums;

namespace Viora.Application.Organizations.RequestOnboard;

public record RequestOnboardCommand(
    Guid OwnerId,
    Guid CountryId,
    string ProposedName,
    string About,
    string ServiceDescription,
    string Letter,
    ICollection<ServiceType> ServiceTypes,
    ReferralSource ReferralSource,
    string BillingEmail,
    string SupportEmail) : ICommand<Guid>;