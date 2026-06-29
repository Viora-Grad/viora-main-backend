namespace Viora.Domain.Forms;

public interface IFormRepository
{
    public void Add(Form form);
    public Task<Form?> GetServiceFormAsync(Guid serviceId, CancellationToken cancellationToken);
    public Task<Form?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public void Remove(Guid formId);
}

