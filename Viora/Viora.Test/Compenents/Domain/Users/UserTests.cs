using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Test.Compenents.Domain.Users;

[TestClass]
public sealed class UserTests
{
    private static readonly PersonalInfo PersonalInfo = new(
        "John", "Doe", new DateOnly(1990, 1, 1), Gender.Male);

    private static readonly Email Email = new("john@example.com");

    // ===== Create =====

    [TestMethod]
    public void Create_ValidInput_SetsAllFieldsAndRegisteredRole()
    {
        DateTime utcNow = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

        User user = User.Create(PersonalInfo, Email, utcNow);

        Assert.IsNotNull(user);
        Assert.AreNotEqual(Guid.Empty, user.Id);
        Assert.AreEqual(Email, user.Email);
        Assert.AreEqual(utcNow, user.CreatedAt);
        Assert.AreEqual(AccountStatus.Active, user.Status);
        Assert.IsFalse(user.IsEmailVerified);
        Assert.IsNull(user.LastLoginAt);
        Assert.AreEqual(1, user.Roles.Count);
        Assert.AreSame(Role.Registered, user.Roles.Single());
    }

    [TestMethod]
    public void Create_DifferentCalls_GenerateDifferentIds()
    {
        DateTime utcNow = DateTime.UtcNow;

        User user1 = User.Create(PersonalInfo, Email, utcNow);
        User user2 = User.Create(PersonalInfo, Email, utcNow);

        Assert.AreNotEqual(user1.Id, user2.Id);
    }

    // ===== LinkIdentity =====

    [TestMethod]
    public void LinkIdentity_ValidIdentity_AddsToCollection()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        AuthIdentity identity = AuthIdentity.Create("google", Guid.NewGuid(), "google-id-123", DateTime.UtcNow);

        Result result = user.LinkIdentity(identity);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, user.Identities.Count);
        Assert.AreSame(identity, user.Identities.Single());
    }

    [TestMethod]
    public void LinkIdentity_NullIdentity_ReturnsFailure()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        Result result = user.LinkIdentity(null!);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.EmptyField, result.Error);
        Assert.AreEqual(0, user.Identities.Count);
    }

    [TestMethod]
    public void LinkIdentity_DuplicateProviderAndKey_ReturnsFailure()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        AuthIdentity identity = AuthIdentity.Create("google", Guid.NewGuid(), "google-id-123", DateTime.UtcNow);
        user.LinkIdentity(identity);

        AuthIdentity duplicate = AuthIdentity.Create("google", Guid.NewGuid(), "google-id-123", DateTime.UtcNow);
        Result result = user.LinkIdentity(duplicate);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.IdentityLinked, result.Error);
        Assert.AreEqual(1, user.Identities.Count);
    }

    [TestMethod]
    public void LinkIdentity_DifferentProviders_BothAllowed()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        AuthIdentity google = AuthIdentity.Create("google", Guid.NewGuid(), "google-id", DateTime.UtcNow);
        AuthIdentity facebook = AuthIdentity.Create("facebook", Guid.NewGuid(), "fb-id", DateTime.UtcNow);

        user.LinkIdentity(google);
        user.LinkIdentity(facebook);

        Assert.AreEqual(2, user.Identities.Count);
    }

    // ===== PromoteToOwner =====

    [TestMethod]
    public void PromoteToOwner_NotYetOwner_AddsRoleAndReturnsSuccess()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        Result result = user.PromoteToOwner(Role.Owner);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(user.Roles.Any(r => r.Id == Role.Owner.Id));
    }

    [TestMethod]
    public void PromoteToOwner_WhenAlreadyOwner_ReturnsFailure()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        user.PromoteToOwner(Role.Owner);

        Result result = user.PromoteToOwner(Role.Owner);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.AlreadyOwner, result.Error);
    }

    [TestMethod]
    public void PromoteToOwner_KeepsExistingRoles()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        user.PromoteToOwner(Role.Owner);

        Assert.AreEqual(2, user.Roles.Count);
        Assert.IsTrue(user.Roles.Any(r => r.Id == Role.Registered.Id));
        Assert.IsTrue(user.Roles.Any(r => r.Id == Role.Owner.Id));
    }

    // ===== BecomeCustomer =====

    [TestMethod]
    public void BecomeCustomer_NotYetCustomer_AddsRoleAndReturnsSuccess()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        Result result = user.BecomeCustomer(Role.Customer);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(user.Roles.Any(r => r.Id == Role.Customer.Id));
    }

    [TestMethod]
    public void BecomeCustomer_WhenAlreadyCustomer_ReturnsFailure()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        user.BecomeCustomer(Role.Customer);

        Result result = user.BecomeCustomer(Role.Customer);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.AlreadyCustomer, result.Error);
    }

    // ===== AddRole =====

    [TestMethod]
    public void AddRole_ValidRole_AddsToCollection()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        Result result = user.AddRole(Role.Admin);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(user.Roles.Any(r => r.Id == Role.Admin.Id));
    }

    [TestMethod]
    public void AddRole_DuplicateRole_ReturnsFailure()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        user.AddRole(Role.Admin);
        Result result = user.AddRole(Role.Admin);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.RoleAlreadyAssigned, result.Error);
        Assert.AreEqual(2, user.Roles.Count); // Registered + Admin (no duplicate)
    }

    // ===== Status Mutations =====

    [TestMethod]
    public void Activate_WhenDeactivated_SetsStatusActive()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        user.Deactivate();

        user.Activate();

        Assert.AreEqual(AccountStatus.Active, user.Status);
    }

    [TestMethod]
    public void Deactivate_WhenActive_SetsStatusDeactivated()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        user.Deactivate();

        Assert.AreEqual(AccountStatus.Deactivated, user.Status);
    }

    [TestMethod]
    public void MarkAsDeleted_SetsStatusDeleted()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        user.MarkAsDeleted();

        Assert.AreEqual(AccountStatus.Deleted, user.Status);
    }

    [TestMethod]
    public void VerifyEmail_SetsIsEmailVerifiedTrue()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        user.VerifyEmail();

        Assert.IsTrue(user.IsEmailVerified);
    }

    [TestMethod]
    public void RecordLogin_SetsLastLoginAt()
    {
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        DateTime loginTime = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

        user.RecordLogin(loginTime);

        Assert.AreEqual(loginTime, user.LastLoginAt);
    }
}
