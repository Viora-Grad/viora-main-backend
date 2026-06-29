namespace Viora.Domain.Forms;

public interface IFormSubmissionRepository
{
    public Task<FormSubmission?> GetByAppointmentIdAsync(Guid AppointmentId, Guid FormId, CancellationToken cancellationToken);
    public void Add(FormSubmission formSubmission);
    public Task<FormSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
