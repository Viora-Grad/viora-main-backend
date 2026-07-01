using Viora.Domain.Abstractions;

namespace Viora.Domain.Forms;

public class FormSubmissionMedia : Entity
{
    public Guid FormSubmissionId { get; private set; }
    public Guid MediaId { get; private set; }

    protected FormSubmissionMedia() { }

    private FormSubmissionMedia(Guid id, Guid formSubmissionId, Guid mediaId) : base(id)
    {
        FormSubmissionId = formSubmissionId;
        MediaId = mediaId;
    }


    public static Result<FormSubmissionMedia> Create(Guid formSubmissionId, Guid mediaId)
    {
        var formSubmissionMedia = new FormSubmissionMedia(Guid.NewGuid(), formSubmissionId, mediaId);
        return Result.Success(formSubmissionMedia);
    }
}
