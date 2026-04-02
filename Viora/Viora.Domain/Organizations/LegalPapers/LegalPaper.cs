using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.LegalPapers.Internals;

namespace Viora.Domain.Organizations.LegalPapers;

public sealed class LegalPaper : Entity
{
    public Guid AttachemntId { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public OfficalName Name { get; private set; } = default!;
    public AcceptanceStatus Status { get; private set; } = default!;
    public LegalPaperType Type { get; private set; }
    public DateTime SubmissionDateUtc { get; private set; }
    public DateTime? ExpiryDateUtc { get; private set; }

    private LegalPaper() { } // for Ef

    private LegalPaper(Guid attachemntId, OfficalName name, AcceptanceStatus status, LegalPaperType type, DateTime submissionDate)
    {
        AttachemntId = attachemntId;
        Name = name;
        Status = status;
        Type = type;
        SubmissionDateUtc = submissionDate;
    }

    public static Result<LegalPaper> Create(Guid attachemntId, string name, AcceptanceStatus status, LegalPaperType type, DateTime submissionDateUtc)
    {
        LegalPaper legalPaper = new(attachemntId, new(name), status, type, submissionDateUtc);

        return Result.Success(legalPaper);
    }

    public Result MarkExpired(DateTime ExpiryTimeUtc)
    {
        if (ExpiryDateUtc != null)
            return Result.Failure(LegalPaperErrors.AlreadyExpired);

        ExpiryDateUtc = ExpiryTimeUtc;

        return Result.Success();
    }
}
