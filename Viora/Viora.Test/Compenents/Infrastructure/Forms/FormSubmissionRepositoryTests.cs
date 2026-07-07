using System.Text.Json;
using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Forms;

namespace Viora.Test.Compenents.Infrastructure.Forms;

/// <summary>
/// Unit tests for the FormSubmissionRepository against an InMemory database.
/// FormSubmission has no ComplexProperty so queries work normally.
/// </summary>
[TestClass]
public sealed class FormSubmissionRepositoryTests : InfrastructureTestBase
{
    private readonly FormSubmissionRepository _repository;

    public FormSubmissionRepositoryTests()
    {
        _repository = new FormSubmissionRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_FormSubmission_PersistsToDatabase()
    {
        var submission = CreateTestFormSubmission();

        _repository.Add(submission);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(submission.Id != Guid.Empty);
    }

    // ===== GetByIdAsync =====

    [TestMethod]
    public async Task GetByIdAsync_SubmissionExists_ReturnsSubmission()
    {
        var submission = CreateTestFormSubmission();
        DbContext.Set<global::Viora.Domain.Forms.FormSubmission>().Add(submission);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(submission.Id, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(submission.Id, result.Id);
        Assert.AreEqual(submission.AppointmentId, result.AppointmentId);
        Assert.AreEqual(submission.FormId, result.FormId);
    }

    [TestMethod]
    public async Task GetByIdAsync_SubmissionNotFound_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.IsNull(result);
    }

    // ===== GetByAppointmentIdAsync =====

    [TestMethod]
    public async Task GetByAppointmentIdAsync_SubmissionExists_ReturnsSubmission()
    {
        var submission = CreateTestFormSubmission();
        DbContext.Set<global::Viora.Domain.Forms.FormSubmission>().Add(submission);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByAppointmentIdAsync(submission.AppointmentId, submission.FormId, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(submission.AppointmentId, result.AppointmentId);
        Assert.AreEqual(submission.FormId, result.FormId);
    }

    [TestMethod]
    public async Task GetByAppointmentIdAsync_SubmissionNotFound_ReturnsNull()
    {
        var result = await _repository.GetByAppointmentIdAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.IsNull(result);
    }

    // ===== Helpers =====

    private static global::Viora.Domain.Forms.FormSubmission CreateTestFormSubmission()
    {
        var submission = JsonDocument.Parse("""{"answers": [{"question": "q1", "answer": "a1"}]}""");
        return global::Viora.Domain.Forms.FormSubmission.Create(Guid.NewGuid(), Guid.NewGuid(), submission, DateTime.UtcNow).Value;
    }
}
