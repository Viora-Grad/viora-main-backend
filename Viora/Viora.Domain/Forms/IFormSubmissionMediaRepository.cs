namespace Viora.Domain.Forms;

public interface IFormSubmissionMediaRepository
{
    public void Add(FormSubmissionMedia formSubmissionMedia);

    public Task<List<FormSubmissionMedia>> GetByFormSubmissionIdAsync(Guid formSubmissionId, CancellationToken cancellationToken);
}
