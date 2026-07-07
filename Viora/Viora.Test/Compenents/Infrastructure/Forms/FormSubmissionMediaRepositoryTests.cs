using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Forms;

namespace Viora.Test.Compenents.Infrastructure.Forms;

/// <summary>
/// Unit tests for the FormSubmissionMediaRepository against an InMemory database.
/// </summary>
[TestClass]
public sealed class FormSubmissionMediaRepositoryTests : InfrastructureTestBase
{
    private readonly FormSubmissionMediaRepository _repository;

    public FormSubmissionMediaRepositoryTests()
    {
        _repository = new FormSubmissionMediaRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_FormSubmissionMedia_PersistsToDatabase()
    {
        var media = CreateTestFormSubmissionMedia();

        _repository.Add(media);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(media.Id != Guid.Empty);
    }

    // ===== GetByFormSubmissionIdAsync =====

    [TestMethod]
    public async Task GetByFormSubmissionIdAsync_MediaExists_ReturnsMediaList()
    {
        var submissionId = Guid.NewGuid();
        var media1 = global::Viora.Domain.Forms.FormSubmissionMedia.Create(submissionId, Guid.NewGuid()).Value;
        var media2 = global::Viora.Domain.Forms.FormSubmissionMedia.Create(submissionId, Guid.NewGuid()).Value;

        DbContext.Set<global::Viora.Domain.Forms.FormSubmissionMedia>().AddRange(media1, media2);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByFormSubmissionIdAsync(submissionId, CancellationToken.None);

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetByFormSubmissionIdAsync_NoMedia_ReturnsEmpty()
    {
        var result = await _repository.GetByFormSubmissionIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetByFormSubmissionIdAsync_DifferentSubmission_ReturnsEmpty()
    {
        var submissionId = Guid.NewGuid();
        var media = global::Viora.Domain.Forms.FormSubmissionMedia.Create(submissionId, Guid.NewGuid()).Value;

        DbContext.Set<global::Viora.Domain.Forms.FormSubmissionMedia>().Add(media);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByFormSubmissionIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.AreEqual(0, result.Count);
    }

    // ===== Helpers =====

    private static global::Viora.Domain.Forms.FormSubmissionMedia CreateTestFormSubmissionMedia()
    {
        return global::Viora.Domain.Forms.FormSubmissionMedia.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
    }
}
