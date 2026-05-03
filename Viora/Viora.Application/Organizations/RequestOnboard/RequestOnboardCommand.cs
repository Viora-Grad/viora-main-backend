using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Organizations.Shared.Enums;

namespace Viora.Application.Organizations.RequestOnboard;

public record RequestOnboardCommand(
    Guid OwnerId,
    Guid CountryId,
    string ProposedName,
    string ServiceDescription,
    string Letter,
    ServiceType ServiceType,
    ReferralSource ReferralSource,
    string BillingEmail,
    string SupportEmail) : ICommand<Guid>;