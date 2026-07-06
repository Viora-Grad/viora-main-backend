using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Viora.Domain.Notifications;
using Viora.Domain.Notifications.Internal;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;
using Viora.Infrastructure;

namespace Viora.Test.Integerations;

[TestClass]
public sealed class NotificationsIntegrationTests
{
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ApplicationDbContext _dbContext;
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public NotificationsIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);
    }

    [TestCleanup]
    public void Cleanup() => _dbContext.Dispose();

    [TestMethod]
    public async Task CreateAndProcessNotification_PersistsAndIsRetrievable()
    {
        var user = User.Create(
            new PersonalInfo("John", "Doe", new DateOnly(1990, 1, 1), Gender.Male),
            new Email("john@example.com"),
            FixedNow);

        _dbContext.Attach(Role.Registered);
        _dbContext.Add(user);
        await _dbContext.SaveChangesAsync();

        var notification = Notification.Create(
            user.Id,
            new Title("Welcome to Viora"),
            new Body("Your account has been created successfully."),
            FixedNow);

        _dbContext.Add(notification);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == notification.Id);

        Assert.IsNotNull(retrieved);
        Assert.AreEqual(user.Id, retrieved.RecipientId);
        Assert.AreEqual("Welcome to Viora", retrieved.Title.Value);
        Assert.AreEqual("Your account has been created successfully.", retrieved.Body.Value);
        Assert.AreEqual(FixedNow, retrieved.SentAt);
        Assert.IsFalse(retrieved.IsRead);

        retrieved.MarkAsRead();
        var updatedEntry = _dbContext.Entry(retrieved);
        updatedEntry.State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();

        var readNotification = await _dbContext.Set<Notification>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notification.Id);

        Assert.IsNotNull(readNotification);
        Assert.IsTrue(readNotification.IsRead);
    }
}
