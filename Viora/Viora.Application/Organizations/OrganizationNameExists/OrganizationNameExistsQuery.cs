using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Organizations.OrganizationNameExists;

public sealed record OrganizationNameExistsQuery(string Name) : IQuery<bool>;