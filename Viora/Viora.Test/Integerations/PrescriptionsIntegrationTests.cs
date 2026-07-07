using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Viora.Domain.Prescriptions;
using Viora.Infrastructure;

namespace Viora.Test.Integerations;

[TestClass]
public sealed class PrescriptionsIntegrationTests
{
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ApplicationDbContext _dbContext;
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public PrescriptionsIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);
    }

    [TestCleanup]
    public void Cleanup() => _dbContext.Dispose();

    [TestMethod]
    public async Task CreatePrescription_WithItems_PersistsAllEntities()
    {
        var appointmentId = Guid.NewGuid();
        var prescriptionResult = Prescription.Create(appointmentId, FixedNow);
        Assert.IsTrue(prescriptionResult.IsSuccess);
        var prescription = prescriptionResult.Value;

        var item1Result = PrescriptionItem.Create(prescription.Id, "Amoxicillin", "Take after meals", "500mg", 3, 7);
        var item2Result = PrescriptionItem.Create(prescription.Id, "Ibuprofen", "Take with food", "200mg", 2, 5);
        Assert.IsTrue(item1Result.IsSuccess);
        Assert.IsTrue(item2Result.IsSuccess);

        prescription.AddItems(new[] { item1Result.Value, item2Result.Value });

        _dbContext.Add(prescription);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<Prescription>()
            .FirstOrDefault(e => e.Entity.Id == prescription.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(appointmentId, tracked.Entity.AppointmentId);
    }

    [TestMethod]
    public async Task CreatePrescriptionTemplate_PersistsTemplateWithMargins()
    {
        var orgId = Guid.NewGuid();
        var templateResult = PrescriptionTemplate.Create(orgId, "General Prescription", Guid.NewGuid(), 2.0, 2.0, 2.0, 2.0);
        Assert.IsTrue(templateResult.IsSuccess);
        var template = templateResult.Value;

        _dbContext.Add(template);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<PrescriptionTemplate>()
            .FirstOrDefault(e => e.Entity.Id == template.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(orgId, tracked.Entity.OrganizationId);
        Assert.AreEqual(2.0, tracked.Entity.TopMargin);
        Assert.AreEqual(2.0, tracked.Entity.RightMargin);
        Assert.AreEqual(2.0, tracked.Entity.LeftMargin);
        Assert.AreEqual(2.0, tracked.Entity.BottomMargin);
    }

    [TestMethod]
    public async Task CreatePrescription_ThenAddItems_VerifiesItemCount()
    {
        var appointmentId = Guid.NewGuid();
        var prescriptionResult = Prescription.Create(appointmentId, FixedNow);
        Assert.IsTrue(prescriptionResult.IsSuccess);
        var prescription = prescriptionResult.Value;

        _dbContext.Add(prescription);
        await _dbContext.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var itemResult = PrescriptionItem.Create(prescription.Id, $"Medication{i}", null, "100mg", 1, 3);
            Assert.IsTrue(itemResult.IsSuccess);
            _dbContext.Add(itemResult.Value);
        }
        await _dbContext.SaveChangesAsync();

        var trackedItems = _dbContext.ChangeTracker.Entries<global::Viora.Domain.Prescriptions.PrescriptionItem>()
            .Where(e => e.Entity.PrescriptionId == prescription.Id)
            .ToList();
        Assert.AreEqual(5, trackedItems.Count);
    }

    [TestMethod]
    public async Task CreatePrescriptionTemplate_WithMediaId_PersistsTemplate()
    {
        var orgId = Guid.NewGuid();
        var mediaId = Guid.NewGuid();
        var templateResult = PrescriptionTemplate.Create(orgId, "Custom Template", mediaId, 1.5, 1.5, 1.5, 1.5);
        Assert.IsTrue(templateResult.IsSuccess);
        var template = templateResult.Value;

        _dbContext.Add(template);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<PrescriptionTemplate>()
            .FirstOrDefault(e => e.Entity.Id == template.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(mediaId, tracked.Entity.TemplateMediaId);
    }

    [TestMethod]
    public async Task DeletePrescription_RemovesEntity()
    {
        var appointmentId = Guid.NewGuid();
        var prescriptionResult = Prescription.Create(appointmentId, FixedNow);
        Assert.IsTrue(prescriptionResult.IsSuccess);
        var prescription = prescriptionResult.Value;

        var itemResult = PrescriptionItem.Create(prescription.Id, "Paracetamol", null, "500mg", 2, 3);
        Assert.IsTrue(itemResult.IsSuccess);
        prescription.AddItems(new[] { itemResult.Value });

        _dbContext.Add(prescription);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();
        var prescriptionToRemove = await _dbContext.Set<Prescription>()
            .FirstOrDefaultAsync(p => p.Id == prescription.Id);
        Assert.IsNotNull(prescriptionToRemove);

        _dbContext.Set<Prescription>().Remove(prescriptionToRemove);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Set<Prescription>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == prescription.Id);
        Assert.IsNull(retrieved);
    }
}
