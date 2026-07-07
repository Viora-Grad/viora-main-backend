using Viora.Domain.Abstractions;
using Viora.Domain.Forms;

namespace Viora.Test.Compenents.Domain.Forms;

/// <summary>
/// Unit tests for the FormSubmissionMedia entity covering the Create factory method.
/// </summary>
[TestClass]
public sealed class FormSubmissionMediaTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a FormSubmissionMedia with correct properties.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsMediaWithCorrectProperties()
    {
        // Arrange
        Guid formSubmissionId = Guid.NewGuid();
        Guid mediaId = Guid.NewGuid();

        // Act
        Result<FormSubmissionMedia> result = FormSubmissionMedia.Create(formSubmissionId, mediaId);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        FormSubmissionMedia media = result.Value;
        Assert.IsNotNull(media);
        Assert.AreNotEqual(Guid.Empty, media.Id);
        Assert.AreEqual(formSubmissionId, media.FormSubmissionId);
        Assert.AreEqual(mediaId, media.MediaId);
    }

    /// <summary>
    /// Verifies that Create generates a new unique Id for each media record.
    /// </summary>
    [TestMethod]
    public void Create_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Guid submissionId = Guid.NewGuid();

        // Act
        Result<FormSubmissionMedia> r1 = FormSubmissionMedia.Create(submissionId, Guid.NewGuid());
        Result<FormSubmissionMedia> r2 = FormSubmissionMedia.Create(submissionId, Guid.NewGuid());

        // Assert
        Assert.AreNotEqual(r1.Value.Id, r2.Value.Id);
    }

    /// <summary>
    /// Verifies that Create with specific GUIDs assigns them correctly.
    /// </summary>
    [TestMethod]
    public void Create_WithSpecificIds_SetsIdsCorrectly()
    {
        // Arrange
        Guid submissionId = new("11111111-1111-1111-1111-111111111111");
        Guid mediaId = new("22222222-2222-2222-2222-222222222222");

        // Act
        Result<FormSubmissionMedia> result = FormSubmissionMedia.Create(submissionId, mediaId);

        // Assert
        Assert.AreEqual(submissionId, result.Value.FormSubmissionId);
        Assert.AreEqual(mediaId, result.Value.MediaId);
    }

    /// <summary>
    /// Verifies that two FormSubmissionMedia instances with the same IDs are not the same reference.
    /// </summary>
    [TestMethod]
    public void Create_DifferentInstances_AreNotSameReference()
    {
        // Arrange
        Guid submissionId = Guid.NewGuid();
        Guid mediaId = Guid.NewGuid();

        // Act
        Result<FormSubmissionMedia> r1 = FormSubmissionMedia.Create(submissionId, mediaId);
        Result<FormSubmissionMedia> r2 = FormSubmissionMedia.Create(submissionId, mediaId);

        // Assert
        Assert.AreNotSame(r1.Value, r2.Value);
    }
}
