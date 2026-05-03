namespace Viora.Domain.Notifications;

public interface INotificationRepository
{
    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(Notification notification);
}
