using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.LegalPapers.Internals;

namespace Viora.Domain.Organizations.LegalPapers;

public sealed class LegalPaper : Entity
{
    public Guid ApplicationId { get; private set; }
    public Guid AttachmentId { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public OfficalName Name { get; private set; } = default!;
    public AcceptanceStatus Status { get; private set; } = default!;
    public LegalPaperType Type { get; private set; }
    public DateTime SubmissionDateUtc { get; private set; }
    public DateTime ExpiryDateUtc { get; private set; }

    private LegalPaper() { } // for Ef

    private LegalPaper(
        Guid id,
        Guid applicationId,
        Guid attachmentId,
        OfficalName name,
        AcceptanceStatus status,
        LegalPaperType type,
        DateTime submissionDate,
        DateTime expiryDateUtc) : base(id)
    {
        ApplicationId = applicationId;
        AttachmentId = attachmentId;
        Name = name;
        Status = status;
        Type = type;
        SubmissionDateUtc = submissionDate;
        ExpiryDateUtc = expiryDateUtc;
    }

    public static Result<LegalPaper> Create(
        Guid attachemntId,
        Guid applicationId,
        string name,
        AcceptanceStatus status,
        LegalPaperType type,
        DateTime submissionDateUtc,
        DateTime ExpiryUtc)
    {
        LegalPaper legalPaper = new(
            Guid.NewGuid(),
            applicationId,
            attachemntId,
            new(name),
            status,
            type,
            submissionDateUtc,
            ExpiryUtc);

        return Result.Success(legalPaper);
    }

    public void MarkExpired()
    {
        Status = AcceptanceStatus.Expired;
    }

    public Result Accept(DateTime currentDateTime, Guid adminId)
    {
        var passesExpiryCheckResult = PassesExpiryCheck(currentDateTime);
        if (passesExpiryCheckResult.IsFailure)
            return Result.Failure(passesExpiryCheckResult.Error);

        Status = AcceptanceStatus.Accepted;
        ApprovedById = adminId;

        return Result.Success();
    }

    public Result Deny(DateTime currentDateTime, Guid adminId)
    {
        var passesExpiryCheckResult = PassesExpiryCheck(currentDateTime);
        if (passesExpiryCheckResult.IsFailure)
            return Result.Failure(passesExpiryCheckResult.Error);

        Status = AcceptanceStatus.Denied;
        ApprovedById = adminId;

        return Result.Success();

    }

    private Result PassesExpiryCheck(DateTime currentDateTime)
    {
        if (Status != AcceptanceStatus.UnderReview)
            return Result.Failure(LegalPaperErrors.PaperStatusNotUnderReview);

        if (ExpiryDateUtc < currentDateTime)
        {
            Status = AcceptanceStatus.Expired;
            return Result.Failure(LegalPaperErrors.AlreadyExpired);
        }
        return Result.Success();
    }
}
