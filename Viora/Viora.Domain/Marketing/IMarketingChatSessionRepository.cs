namespace Viora.Domain.Marketing;

public interface IMarketingChatSessionRepository
{
    void Add(MarketingChatSession session);

    Task<MarketingChatSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Loads the session with its message collection (ordered by creation) for detail views and finalize.
    Task<MarketingChatSession?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MarketingChatSession>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
