using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.LegalPapers.Internals;

namespace Viora.Domain.Organizations.LegalPapers;

public sealed class LegalPaper : Entity
{
    public Guid AttachmentId { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public OfficalName Name { get; private set; } = default!;
    public AcceptanceStatus Status { get; private set; } = default!;
    public LegalPaperType Type { get; private set; }
    public DateTime SubmissionDateUtc { get; private set; }
    public DateTime ExpiryDateUtc { get; private set; }

    private LegalPaper() { } // for Ef

    private LegalPaper(Guid id, Guid attachmentId, OfficalName name, AcceptanceStatus status, LegalPaperType type, DateTime submissionDate, DateTime expiryDateUtc) : base(id)
    {
        AttachmentId = attachmentId;
        Name = name;
        Status = status;
        Type = type;
        SubmissionDateUtc = submissionDate;
        ExpiryDateUtc = expiryDateUtc;
    }

    public static Result<LegalPaper> Create(Guid attachemntId, string name, AcceptanceStatus status, LegalPaperType type, DateTime submissionDateUtc, ILegalPapersSettings legalPaperSettings)
    {
        LegalPaper legalPaper = new(
            Guid.NewGuid(),
            attachemntId,
            new(name),
            status,
            type,
            submissionDateUtc,
            submissionDateUtc + legalPaperSettings.LegalPaperExpiry);

        return Result.Success(legalPaper);
    }

    public Result MarkExpired(DateTime ExpiryTimeUtc, DateTime currentDateTime)
    {
        if (Status == AcceptanceStatus.Expired)
            return Result.Failure(LegalPaperErrors.AlreadyExpired);

        if (currentDateTime > ExpiryDateUtc)
        {
            Status = AcceptanceStatus.Expired;
            return Result.Failure(LegalPaperErrors.AlreadyExpired);
        }

        ExpiryDateUtc = ExpiryTimeUtc;

        return Result.Success();
    }
}
