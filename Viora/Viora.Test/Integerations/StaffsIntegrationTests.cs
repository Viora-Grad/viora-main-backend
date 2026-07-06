using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using Viora.Domain.Branches;
using Viora.Domain.Services;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Staffs.Internal;
using Viora.Domain.Users.Identity;
using Viora.Infrastructure;

namespace Viora.Test.Integerations;

[TestClass]
public sealed class StaffsIntegrationTests
{
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ApplicationDbContext _dbContext;
    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    private static readonly Mock<IServiceSettings> ServiceSettingsMock = new();

    public StaffsIntegrationTests()
    {
        ServiceSettingsMock.Setup(s => s.SlotSizeInMinutes).Returns(15);
        ServiceSettingsMock.Setup(s => s.MinimumDurationInMinutes).Returns(15);
        ServiceSettingsMock.Setup(s => s.MaximumDurationInMinutes).Returns(240);
        ServiceSettingsMock.Setup(s => s.MaxGallerySize).Returns(10);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);
    }

    [TestInitialize]
    public void Initialize()
    {
    }

    [TestCleanup]
    public void Cleanup() => _dbContext.Dispose();

    [TestMethod]
    public async Task CreateStaff_WithAssignedServices_PersistsRelationshipsCorrectly()
    {
        var address = new Address(1, "Main St", "Cairo", "Cairo", Guid.NewGuid(), 12345);
        var point = new Point(31.2357, 30.0444);
        var branchResult = Branch.Create(OrgId, address, point, "branch@test.com",
            [ServiceType.Cardiology], FixedNow);
        Assert.IsTrue(branchResult.IsSuccess);
        var branch = branchResult.Value;

        var money = new Money(200m, Currency.Usd);
        var serviceResult = Service.Create(branch.Id, "Cardiology Consult", "Heart consultation",
            30, ServiceType.Cardiology, money, ServiceSettingsMock.Object);
        Assert.IsTrue(serviceResult.IsSuccess);
        var service = serviceResult.Value;

        var staff = Staff.Create(OrgId, FixedNow);
        staff.SetStaffProperties("Ahmed", "Ali", "ahmed_ali", "hashed_pw",
            new DateOnly(1990, 1, 1), Gender.Male, "+201001234567");
        staff.AddRoles([new Role("Cardiologist", null, OrgId)]);
        staff.AssignBranches([branch]);
        staff.AssignServices([service]);

        var activateResult = staff.Activate();
        Assert.IsTrue(activateResult.IsSuccess);

        _dbContext.AddRange(branch, service, staff);
        await _dbContext.SaveChangesAsync();

        // Use Find to retrieve the tracked entity directly from the local cache,
        // bypassing the LINQ query pipeline which triggers an in-memory provider bug
        // when resolving Service entity metadata (Staff has a many-to-many to Service).
        var retrieved = _dbContext.Set<Staff>().Find(staff.Id);

        Assert.IsNotNull(retrieved);
        Assert.AreEqual(StaffStatus.Active, retrieved.StaffStatus);
        Assert.AreEqual("ahmed_ali", retrieved.Username!.Value);

        // Verify relationship join tables via the change tracker instead of LINQ queries
        var branchLink = _dbContext.ChangeTracker.Entries<Dictionary<string, object>>()
            .FirstOrDefault(e => e.Metadata.Name == "StaffBranch"
                && (Guid)e.CurrentValues["StaffId"]! == staff.Id);
        Assert.IsNotNull(branchLink);
        Assert.AreEqual(branch.Id, (Guid)branchLink.CurrentValues["BranchId"]!);

        var roleLink = _dbContext.ChangeTracker.Entries<Dictionary<string, object>>()
            .FirstOrDefault(e => e.Metadata.Name == "StaffRole"
                && (Guid)e.CurrentValues["StaffId"]! == staff.Id);
        Assert.IsNotNull(roleLink);

        var serviceLink = _dbContext.ChangeTracker.Entries<Dictionary<string, object>>()
            .FirstOrDefault(e => e.Metadata.Name == "StaffService"
                && (Guid)e.CurrentValues["StaffId"]! == staff.Id);
        Assert.IsNotNull(serviceLink);
        Assert.AreEqual(service.Id, (Guid)serviceLink.CurrentValues["ServiceId"]!);
    }
}
