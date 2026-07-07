using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using Viora.Domain.Forms;
using Viora.Infrastructure;

namespace Viora.Test.Integerations;

[TestClass]
public sealed class FormsIntegrationTests
{
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ApplicationDbContext _dbContext;
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public FormsIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, _publisherMock.Object);
    }

    [TestCleanup]
    public void Cleanup() => _dbContext.Dispose();

    [TestMethod]
    public async Task CreateForm_WithFields_PersistsForm()
    {
        var serviceId = Guid.NewGuid();
        var fields = JsonDocument.Parse("""{"patient_name": "text", "symptoms": "textarea"}""");
        var formResult = Form.Create(serviceId, null, "Patient Registration", fields);
        Assert.IsTrue(formResult.IsSuccess);
        var form = formResult.Value;

        _dbContext.Add(form);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<Form>()
            .FirstOrDefault(e => e.Entity.Id == form.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(serviceId, tracked.Entity.ServiceId);
    }

    [TestMethod]
    public async Task CreateFormSubmission_PersistsSubmissionData()
    {
        var serviceId = Guid.NewGuid();
        var fields = JsonDocument.Parse("""{"patient_name": "text"}""");
        var formResult = Form.Create(serviceId, null, "Patient Intake", fields);
        Assert.IsTrue(formResult.IsSuccess);
        var form = formResult.Value;
        _dbContext.Add(form);
        await _dbContext.SaveChangesAsync();

        var submissionData = JsonDocument.Parse("""{"patient_name": "John Doe", "age": "30"}""");
        var submissionResult = FormSubmission.Create(Guid.NewGuid(), form.Id, submissionData, FixedNow);
        Assert.IsTrue(submissionResult.IsSuccess);
        var submission = submissionResult.Value;

        _dbContext.Add(submission);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Set<FormSubmission>()
            .AsNoTracking()
            .FirstOrDefaultAsync(fs => fs.Id == submission.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(form.Id, retrieved.FormId);
        Assert.AreEqual(FixedNow, retrieved.CreatedAt);
    }

    [TestMethod]
    public async Task CreateFormSubmissionMedia_PersistsMediaLink()
    {
        var serviceId = Guid.NewGuid();
        var fields = JsonDocument.Parse("""{"xray": "file"}""");
        var formResult = Form.Create(serviceId, null, "X-Ray Upload", fields);
        Assert.IsTrue(formResult.IsSuccess);
        var form = formResult.Value;
        _dbContext.Add(form);
        await _dbContext.SaveChangesAsync();

        var submissionResult = FormSubmission.Create(Guid.NewGuid(), form.Id, JsonDocument.Parse("""{"xray": "image1.jpg"}"""), FixedNow);
        Assert.IsTrue(submissionResult.IsSuccess);
        var submission = submissionResult.Value;
        _dbContext.Add(submission);
        await _dbContext.SaveChangesAsync();

        var mediaResult = FormSubmissionMedia.Create(submission.Id, Guid.NewGuid());
        Assert.IsTrue(mediaResult.IsSuccess);
        var media = mediaResult.Value;

        _dbContext.Add(media);
        await _dbContext.SaveChangesAsync();

        var retrieved = await _dbContext.Set<FormSubmissionMedia>()
            .AsNoTracking()
            .FirstOrDefaultAsync(fsm => fsm.Id == media.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(submission.Id, retrieved.FormSubmissionId);
    }

    [TestMethod]
    public async Task UpdateForm_Fields_PersistsUpdatedFields()
    {
        var serviceId = Guid.NewGuid();
        var fields = JsonDocument.Parse("""{"field1": "text"}""");
        var formResult = Form.Create(serviceId, null, "Dynamic Form", fields);
        Assert.IsTrue(formResult.IsSuccess);
        var form = formResult.Value;
        _dbContext.Add(form);
        await _dbContext.SaveChangesAsync();

        var updatedFields = JsonDocument.Parse("""{"field1": "text", "field2": "textarea", "field3": "checkbox"}""");
        form.Update(updatedFields);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<Form>()
            .FirstOrDefault(e => e.Entity.Id == form.Id);
        Assert.IsNotNull(tracked);
    }

    [TestMethod]
    public async Task CreateForm_WithStaffId_PersistsStaffAssociation()
    {
        var serviceId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var fields = JsonDocument.Parse("""{"assessment": "textarea"}""");
        var formResult = Form.Create(serviceId, staffId, "Staff Assessment", fields);
        Assert.IsTrue(formResult.IsSuccess);
        var form = formResult.Value;

        _dbContext.Add(form);
        await _dbContext.SaveChangesAsync();

        var tracked = _dbContext.ChangeTracker.Entries<Form>()
            .FirstOrDefault(e => e.Entity.Id == form.Id);
        Assert.IsNotNull(tracked);
        Assert.AreEqual(staffId, tracked.Entity.StaffId);
    }
}
