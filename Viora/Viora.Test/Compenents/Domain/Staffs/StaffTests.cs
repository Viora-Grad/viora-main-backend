using Moq;
using NetTopologySuite.Geometries;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Services;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;
using Viora.Domain.Users.Identity;

namespace Viora.Test.Compenents.Domain.Staffs;

[TestClass]
public sealed class StaffTests
{
    // ===== Create =====

    [TestMethod]
    public void Create_ValidInput_SetsOrganizationIdAndCreatedAtAndPendingStatus()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Staff staff = Staff.Create(orgId, createdAt);

        // Assert
        Assert.IsNotNull(staff);
        Assert.AreEqual(orgId, staff.OrganizationId);
        Assert.AreEqual(createdAt, staff.CreatedAt);
        Assert.AreEqual(StaffStatus.Pending, staff.StaffStatus);
    }

    [TestMethod]
    public void Create_WithoutId_GeneratesNewGuid()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Staff staff = Staff.Create(orgId, createdAt);

        // Assert
        Assert.AreNotEqual(Guid.Empty, staff.Id);
    }

    [TestMethod]
    public void Create_WithProvidedId_UsesSuppliedId()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;
        Guid suppliedId = Guid.NewGuid();

        // Act
        Staff staff = Staff.Create(orgId, createdAt, suppliedId);

        // Assert
        Assert.AreEqual(suppliedId, staff.Id);
    }

    // ===== AddRoles =====

    [TestMethod]
    public void AddRoles_NullRoles_ThrowsArgumentException()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => staff.AddRoles(null!));
    }

    [TestMethod]
    public void AddRoles_EmptyRoles_ThrowsArgumentException()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => staff.AddRoles(Enumerable.Empty<Role>()));
    }

    [TestMethod]
    public void AddRoles_ValidRoles_AddsRoles()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        Role role = new(1, "TestRole");

        // Act
        staff.AddRoles([role]);

        // Assert
        Assert.AreEqual(1, staff.Roles.Count);
        Assert.AreSame(role, staff.Roles.Single());
    }

    [TestMethod]
    public void AddRoles_DuplicateRoles_DoesNotAddDuplicates()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        Role role = new(1, "TestRole");

        // Act
        staff.AddRoles([role, role]);

        // Assert
        Assert.AreEqual(1, staff.Roles.Count);
    }

    // ===== AssignBranches =====

    [TestMethod]
    public void AssignBranches_NullBranches_ThrowsArgumentException()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => staff.AssignBranches(null!));
    }

    [TestMethod]
    public void AssignBranches_EmptyBranches_ThrowsArgumentException()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => staff.AssignBranches([]));
    }

    [TestMethod]
    public void AssignBranches_ValidBranches_AddsBranches()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        Branch branch = CreateTestBranch();

        // Act
        staff.AssignBranches([branch]);

        // Assert
        Assert.AreEqual(1, staff.Branches.Count);
        Assert.AreSame(branch, staff.Branches.Single());
    }

    [TestMethod]
    public void AssignBranches_DuplicateBranches_DoesNotAddDuplicates()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        Branch branch = CreateTestBranch();

        // Act
        staff.AssignBranches([branch]);
        staff.AssignBranches([branch]);

        // Assert
        Assert.AreEqual(1, staff.Branches.Count);
    }

    // ===== AssignServices =====

    [TestMethod]
    public void AssignServices_NullServices_ThrowsArgumentException()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => staff.AssignServices(null!));
    }

    [TestMethod]
    public void AssignServices_EmptyServices_ThrowsArgumentException()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => staff.AssignServices([]));
    }

    [TestMethod]
    public void AssignServices_ValidServices_AddsServices()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        Service service = CreateTestService();

        // Act
        staff.AssignServices([service]);

        // Assert
        Assert.AreEqual(1, staff.Services.Count);
        Assert.AreSame(service, staff.Services.Single());
    }

    [TestMethod]
    public void AssignServices_DuplicateServices_DoesNotAddDuplicates()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        Service service = CreateTestService();

        // Act
        staff.AssignServices([service]);
        staff.AssignServices([service]);

        // Assert
        Assert.AreEqual(1, staff.Services.Count);
    }

    // ===== SetStaffProperties =====

    [TestMethod]
    public void SetStaffProperties_ValidInput_AssignsAllProperties()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        FirstName firstName = "John";
        LastName lastName = "Doe";
        Username username = "john_doe";
        HashedPassword hashedPassword = "hash123";
        DateOnly dateOfBirth = new(1990, 1, 1);
        Gender gender = Gender.Male;
        PhoneNumber phoneNumber = "+1234567890";

        // Act
        staff.SetStaffProperties(firstName, lastName, username, hashedPassword, dateOfBirth, gender, phoneNumber);

        // Assert
        Assert.AreEqual(firstName, staff.FirstName);
        Assert.AreEqual(lastName, staff.LastName);
        Assert.AreEqual(username, staff.Username);
        Assert.AreEqual(hashedPassword, staff.HashedPassword);
        Assert.AreEqual(dateOfBirth, staff.DateOfBirth);
        Assert.AreEqual(gender, staff.Gender);
        Assert.AreEqual(phoneNumber, staff.PhoneNumber);
    }

    // ===== Activate =====

    [TestMethod]
    public void Activate_WhenAlreadyActive_ReturnsFailure()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        staff.SetStaffProperties("John", "Doe", "john_doe", "hash123", new DateOnly(1990, 1, 1), Gender.Male, "+1234567890");
        staff.AssignBranches([CreateTestBranch()]);
        staff.Activate();

        // Act
        Result result = staff.Activate();

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(StaffErrors.StaffAlreadyActive, result.Error);
    }

    [TestMethod]
    public void Activate_WhenMissingProperties_ReturnsFailure()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Act
        Result result = staff.Activate();

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(StaffErrors.InvalidStaffInstance, result.Error);
    }

    [TestMethod]
    public void Activate_WhenMissingBranches_ReturnsFailure()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        staff.SetStaffProperties("John", "Doe", "john_doe", "hash123", new DateOnly(1990, 1, 1), Gender.Male, "+1234567890");

        // Act
        Result result = staff.Activate();

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(StaffErrors.InvalidStaffInstance, result.Error);
    }

    [TestMethod]
    public void Activate_WhenValidInstance_ReturnsSuccessAndSetsStatusActive()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        staff.SetStaffProperties("John", "Doe", "john_doe", "hash123", new DateOnly(1990, 1, 1), Gender.Male, "+1234567890");
        staff.AssignBranches([CreateTestBranch()]);

        // Act
        Result result = staff.Activate();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(StaffStatus.Active, staff.StaffStatus);
    }

    // ===== Suspend =====

    [TestMethod]
    public void Suspend_FirstCall_ReturnsSuccessAndSetsStatusSuspended()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Act
        Result result = staff.Suspend();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(StaffStatus.Suspended, staff.StaffStatus);
    }

    [TestMethod]
    public void Suspend_WhenAlreadySuspended_ReturnsFailure()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        staff.Suspend();

        // Act
        Result result = staff.Suspend();

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(StaffErrors.StaffAlreadySuspended, result.Error);
    }

    // ===== Delete =====

    [TestMethod]
    public void Delete_SetsDeletedAtAndIsDeletedTrue()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        DateTime deletedAt = DateTime.UtcNow;

        // Act
        staff.Delete(deletedAt);

        // Assert
        Assert.AreEqual(deletedAt, staff.DeletedAt);
        Assert.IsTrue(staff.IsDeleted);
    }

    // ===== RemoveRoles =====

    [TestMethod]
    public void RemoveRoles_NullRoles_ThrowsArgumentException()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => staff.RemoveRoles(null!));
    }

    [TestMethod]
    public void RemoveRoles_EmptyRoles_ThrowsArgumentException()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => staff.RemoveRoles([]));
    }

    [TestMethod]
    public void RemoveRoles_ExistingRoles_RemovesOnlyMatchingRoles()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        Role role1 = new(1, "Role1");
        Role role2 = new(2, "Role2");
        Role role3 = new(3, "Role3");
        staff.AddRoles([role1, role2, role3]);

        // Act
        staff.RemoveRoles([role1, role3]);

        // Assert
        Assert.AreEqual(1, staff.Roles.Count);
        Assert.AreSame(role2, staff.Roles.Single());
    }

    [TestMethod]
    public void RemoveRoles_NonExistentRoles_DoesNothing()
    {
        // Arrange
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        Role role1 = new(1, "Role1");
        Role role2 = new(2, "Role2");
        staff.AddRoles([role1]);

        // Act
        staff.RemoveRoles([role2]);

        // Assert
        Assert.AreEqual(1, staff.Roles.Count);
        Assert.AreSame(role1, staff.Roles.Single());
    }

    // ===== SeedActiveStaff =====

    [TestMethod]
    public void SeedActiveStaff_PopulatesAllFields()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();
        DateTime createdAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateOnly dateOfBirth = new(1990, 5, 15);
        Gender gender = Gender.Male;
        PhoneNumber phoneNumber = "+1234567890";

        // Act
        Staff staff = Staff.SeedActiveStaff(id, orgId, "John", "Doe", createdAt, dateOfBirth, gender, phoneNumber);

        // Assert
        Assert.AreEqual(id, staff.Id);
        Assert.AreEqual(orgId, staff.OrganizationId);
        Assert.AreEqual(createdAt, staff.CreatedAt);
        Assert.AreEqual(StaffStatus.Pending, staff.StaffStatus);
        Assert.AreEqual("John", (string)staff.FirstName!);
        Assert.AreEqual("Doe", (string)staff.LastName!);
        Assert.AreEqual(dateOfBirth, staff.DateOfBirth);
        Assert.AreEqual(gender, staff.Gender);
        Assert.AreEqual(phoneNumber, staff.PhoneNumber);
        Assert.IsNull(staff.Username);
        Assert.IsNull(staff.HashedPassword);
    }

    [TestMethod]
    public void SeedActiveStaff_NullDateOfBirth_FallsBackToDefault()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();
        DateTime createdAt = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        Gender gender = Gender.Female;
        PhoneNumber phoneNumber = "+9876543210";

        // Act
        Staff staff = Staff.SeedActiveStaff(id, orgId, "Jane", "Smith", createdAt, null, gender, phoneNumber);

        // Assert
        Assert.AreEqual(new DateOnly(2000, 1, 1), staff.DateOfBirth);
    }

    // ===== UpdateRoles =====

    [TestMethod]
    public void UpdateRoles_NullRoles_ThrowsArgumentException()
    {
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        Assert.Throws<ArgumentException>(() => staff.UpdateRoles(null!));
    }

    [TestMethod]
    public void UpdateRoles_EmptyRoles_ThrowsArgumentException()
    {
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        Assert.Throws<ArgumentException>(() => staff.UpdateRoles(Enumerable.Empty<Role>()));
    }

    [TestMethod]
    public void UpdateRoles_ValidRoles_ReplacesExistingRoles()
    {
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        Role role1 = new(1, "Role1");
        Role role2 = new(2, "Role2");
        staff.AddRoles([role1]);

        staff.UpdateRoles([role2]);

        Assert.AreEqual(1, staff.Roles.Count);
        Assert.AreSame(role2, staff.Roles.Single());
    }

    // ===== UpdateServices =====

    [TestMethod]
    public void UpdateServices_NullServices_ThrowsArgumentException()
    {
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        Assert.Throws<ArgumentException>(() => staff.UpdateServices(null!));
    }

    [TestMethod]
    public void UpdateServices_EmptyServices_ThrowsArgumentException()
    {
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);

        Assert.Throws<ArgumentException>(() => staff.UpdateServices([]));
    }

    [TestMethod]
    public void UpdateServices_ValidServices_ReplacesExistingServices()
    {
        Staff staff = Staff.Create(Guid.NewGuid(), DateTime.UtcNow);
        Service service1 = CreateTestService();
        Service service2 = CreateTestService();
        staff.AssignServices([service1]);

        staff.UpdateServices([service2]);

        Assert.AreEqual(1, staff.Services.Count);
        Assert.AreSame(service2, staff.Services.Single());
    }

    // ===== Helpers =====

    private static Branch CreateTestBranch()
    {
        Address address = new(1, "Main St", "City", "State", Guid.NewGuid(), 12345);
        Point location = new GeometryFactory().CreatePoint(new Coordinate(0, 0));
        return Branch.Create(Guid.NewGuid(), address, location, "branch@test.com", [ServiceType.Cardiology], DateTime.UtcNow).Value;
    }

    private static Service CreateTestService()
    {
        Mock<IServiceSettings> mockSettings = new();
        mockSettings.SetupGet(s => s.SlotSizeInMinutes).Returns(30);
        mockSettings.SetupGet(s => s.MinimumDurationInMinutes).Returns(30);
        mockSettings.SetupGet(s => s.MaximumDurationInMinutes).Returns(120);
        mockSettings.SetupGet(s => s.MaxGallerySize).Returns(10);
        return Service.Create(Guid.NewGuid(), "Test", "Desc", 30, ServiceType.Cardiology, new Money(100m, Currency.Usd), mockSettings.Object).Value;
    }
}
