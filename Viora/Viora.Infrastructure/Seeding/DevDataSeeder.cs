using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Medias;
using Viora.Domain.Orders;
using Viora.Domain.Organizations.LegalPapers;
using Viora.Domain.Organizations.LegalPapers.Internals;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.OnBoardings.Internals;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Plans;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Subscriptions;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;
using Viora.Domain.Users.Owners;
using Viora.Infrastructure.Authentication;
using BranchEmail = Viora.Domain.Shared.Internal.Email;

namespace Viora.Infrastructure.Seeding;

public interface IDevDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Seeds coherent, loginable "persona" object graphs for local development so the UI/QA can
/// exercise entities across their different states. This is DEV-ONLY scenario data and is kept
/// separate from <see cref="DatabaseSeeder"/> (which seeds canonical reference data).
///
/// Design notes:
/// - Idempotent: each persona is gated on its login identity, so re-running is a no-op.
/// - Domain events are suppressed during seeding (see <see cref="SaveSuppressingDomainEventsAsync"/>)
///   so creating aggregates does not fire emails / scheduling side effects for fake data.
/// - Real placeholder blobs are written through <see cref="IStorageService"/> so the media
///   download endpoints actually serve a file.
/// </summary>
internal sealed class DevDataSeeder(
    ApplicationDbContext db,
    IHasher hasher,
    IDateTimeProvider clock,
    IStorageService storage,
    IStorageSettings storageSettings,
    IOnboardingSettings onboardingSettings,
    ILogger<DevDataSeeder> logger) : IDevDataSeeder
{
    // Seeded reference ids (see PlanData / CountriesData).
    private static readonly Guid StarterPlanId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid EgyptCountryId = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000003");

    private const string DefaultPassword = "Dev123!Pass";

    // A minimal but valid 1x1 transparent PNG and a tiny valid PDF, used as placeholder blobs.
    private static readonly byte[] PlaceholderPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+M8AAAMBAQDJ/pLvAAAAAElFTkSuQmCC");

    private static readonly byte[] PlaceholderPdf = System.Text.Encoding.ASCII.GetBytes(
        "%PDF-1.1\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n" +
        "2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n" +
        "3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 200 200]>>endobj\n" +
        "trailer<</Root 1 0 R>>\n%%EOF");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Track the seeded Role singletons as Unchanged so the many-to-many join inserts
        // reference the existing rows instead of trying to re-insert the roles.
        db.Attach(Role.Registered);
        db.Attach(Role.Owner);

        await SeedActiveOwnerAsync(cancellationToken);
        await SeedPendingOwnerAsync(cancellationToken);
    }

    /// <summary>
    /// "Active" owner: approved application, active organization + subscription, a branch with a
    /// gallery image, and four legal papers (one accepted). Exercises both media download endpoints.
    /// Login: active.owner@viora.dev / Dev123!Pass
    /// </summary>
    private async Task SeedActiveOwnerAsync(CancellationToken cancellationToken)
    {
        const string email = "active.owner@viora.dev";
        if (await PersonaExistsAsync(email, cancellationToken))
        {
            logger.LogInformation("DevData: active owner persona ({Email}) already present, skipping.", email);
            return;
        }

        var now = clock.UtcNow;
        var ownerId = await SeedLoginableOwnerAsync(email, "Aya", "Hassan", cancellationToken);

        // Application -> approved.
        var application = Unwrap(BuildApplication(ownerId, "Nile Care Clinic", now), "application");
        Check(application.MarkAccepted(now), "mark application accepted");
        db.Set<OrganizationApplication>().Add(application);
        await SaveSuppressingDomainEventsAsync(cancellationToken);

        // Organization (active) + subscription (active).
        var organization = Unwrap(Organization.Create(
            ownerId, EgyptCountryId, "Nile Care Clinic",
            "A multi-specialty clinic in Cairo.", "General and specialized outpatient care.",
            new List<ServiceType> { ServiceType.Cardiology, ServiceType.Dermatology },
            now, ReferralSource.Website, "billing@nilecare.dev", "support@nilecare.dev"), "organization");
        db.Set<Organization>().Add(organization);

        var subscription = Unwrap(Subscription.Create(StarterPlanId, organization.Id, now, now.AddMonths(1)), "subscription");
        db.Set<Subscription>().Add(subscription);

        var plan = await db.Set<Plan>().FindAsync([StarterPlanId], cancellationToken)
            ?? throw new InvalidOperationException("DevData: Starter plan not seeded; run reference seeder first.");
        var order = Unwrap(SubscriptionOrder.CreateNewSubscriptionOrder(organization.Id, plan, now), "subscription order");
        db.Set<SubscriptionOrder>().Add(order);

        // Branch with a gallery image (placeholder PNG) -> branch gallery file endpoint.
        var branch = Unwrap(Branch.Create(
            organization.Id,
            new Address(10, "Tahrir Street", "Cairo", "Cairo", EgyptCountryId, 11511),
            new Point(31.2357, 30.0444) { SRID = 4326 },
            new BranchEmail("branch@nilecare.dev"),
            new List<ServiceType> { ServiceType.Cardiology },
            now), "branch");

        var galleryImage = await SeedMediaAsync(
            "clinic-front.png", $"branches/{branch.Id}/gallery/{Guid.NewGuid()}.png",
            "image/png", PlaceholderPng, organization.Id, cancellationToken);
        Check(branch.AddToGallery(galleryImage, maxImageInBranch: 50), "add image to gallery");
        db.Set<Branch>().Add(branch);

        // Legal papers (placeholder PDFs) -> legal paper file endpoint. First one accepted.
        var papers = new[]
        {
            LegalPaperType.ArticleOfAssociation,
            LegalPaperType.CommercialRegistration,
            LegalPaperType.RegisteredAddressProof,
            LegalPaperType.TaxCard,
        };
        var first = true;
        foreach (var type in papers)
        {
            var media = await SeedMediaAsync(
                $"{type}.pdf", $"legal-papers/{application.Id}/{Guid.NewGuid()}.pdf",
                "application/pdf", PlaceholderPdf, organizationId: null, cancellationToken);

            var paper = Unwrap(LegalPaper.Create(
                media.Id, application.Id, $"{type} document",
                AcceptanceStatus.UnderReview, type, now, now.AddYears(1)), $"legal paper {type}");

            if (first)
            {
                Check(paper.Accept(now, ownerId), "accept legal paper");
                first = false;
            }

            db.Set<LegalPaper>().Add(paper);
        }

        await SaveSuppressingDomainEventsAsync(cancellationToken);
        logger.LogInformation("DevData: seeded active owner persona ({Email}).", email);
    }

    /// <summary>
    /// "Pending" owner: a submitted application still awaiting review, with under-review legal papers.
    /// Login: pending.owner@viora.dev / Dev123!Pass
    /// </summary>
    private async Task SeedPendingOwnerAsync(CancellationToken cancellationToken)
    {
        const string email = "pending.owner@viora.dev";
        if (await PersonaExistsAsync(email, cancellationToken))
        {
            logger.LogInformation("DevData: pending owner persona ({Email}) already present, skipping.", email);
            return;
        }

        var now = clock.UtcNow;
        var ownerId = await SeedLoginableOwnerAsync(email, "Omar", "Khaled", cancellationToken);

        var application = Unwrap(BuildApplication(ownerId, "Cairo Dental Center", now), "application");
        db.Set<OrganizationApplication>().Add(application);
        await SaveSuppressingDomainEventsAsync(cancellationToken);

        var media = await SeedMediaAsync(
            "CommercialRegistration.pdf", $"legal-papers/{application.Id}/{Guid.NewGuid()}.pdf",
            "application/pdf", PlaceholderPdf, organizationId: null, cancellationToken);
        var paper = Unwrap(LegalPaper.Create(
            media.Id, application.Id, "Commercial registration document",
            AcceptanceStatus.UnderReview, LegalPaperType.CommercialRegistration, now, now.AddYears(1)),
            "legal paper");
        db.Set<LegalPaper>().Add(paper);

        await SaveSuppressingDomainEventsAsync(cancellationToken);
        logger.LogInformation("DevData: seeded pending owner persona ({Email}).", email);
    }

    private Result<OrganizationApplication> BuildApplication(Guid ownerId, string proposedName, DateTime now) =>
        OrganizationApplication.Create(
            ownerId, EgyptCountryId,
            new Name(proposedName),
            new Letter("We are a healthcare provider seeking to onboard with Viora."),
            new About($"{proposedName} provides outpatient care."),
            new List<ServiceType> { ServiceType.Cardiology, ServiceType.Dermatology },
            new ServiceDescription("General and specialized outpatient care."),
            ReferralSource.Website,
            new BillingEmail("billing@" + Slug(proposedName) + ".dev"),
            new SupportEmail("support@" + Slug(proposedName) + ".dev"),
            now, onboardingSettings);

    /// <summary>Creates a User with local credentials + Owner role, then the Owner aggregate. Returns the shared id.</summary>
    private async Task<Guid> SeedLoginableOwnerAsync(string email, string firstName, string lastName, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;

        var user = User.Create(
            new PersonalInfo(firstName, lastName, new DateOnly(1990, 1, 1), Gender.Male),
            new Domain.Users.Internal.Email(email), now);

        db.Set<LocalCredential>().Add(new LocalCredential(user.Id, hasher.Hash(DefaultPassword)));

        var identity = AuthIdentity.Create("local", user.Id, user.Email.Value, now);
        user.LinkIdentity(identity);
        db.Set<AuthIdentity>().Add(identity);

        user.PromoteToOwner(Role.Owner);
        db.Set<User>().Add(user);
        await SaveSuppressingDomainEventsAsync(cancellationToken);

        db.Set<Owner>().Add(Owner.Create(user.Id, EgyptCountryId, user.PersonalInfo, now));
        await SaveSuppressingDomainEventsAsync(cancellationToken);

        return user.Id;
    }

    /// <summary>Writes a placeholder blob to storage and registers a tracked <see cref="MediaFile"/> for it.</summary>
    private async Task<MediaFile> SeedMediaAsync(
        string name, string key, string mimeType, byte[] content, Guid? organizationId, CancellationToken cancellationToken)
    {
        await storage.SaveFileAsync(new MemoryStream(content), key, cancellationToken);
        var media = Unwrap(MediaFile.Create(
            name, content.LongLength, key, mimeType, clock.UtcNow, storageSettings.MaxFileSizeBytes, organizationId),
            $"media {name}");
        db.Set<MediaFile>().Add(media);
        return media;
    }

    private Task<bool> PersonaExistsAsync(string email, CancellationToken cancellationToken) =>
        db.Set<AuthIdentity>().AnyAsync(
            i => i.Provider == "local" && i.ProviderKey == email.ToLowerInvariant().Trim(),
            cancellationToken);

    private async Task SaveSuppressingDomainEventsAsync(CancellationToken cancellationToken)
    {
        foreach (var entry in db.ChangeTracker.Entries<Entity>())
            entry.Entity.ClearDomainEvents();

        await db.SaveChangesAsync(cancellationToken);
    }

    private static T Unwrap<T>(Result<T> result, string what) =>
        result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException($"DevData: failed creating {what}: {result.Error.Name} - {result.Error.Description}");

    private static void Check(Result result, string what)
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"DevData: failed during {what}: {result.Error.Name} - {result.Error.Description}");
    }

    private static string Slug(string value) =>
        new string(value.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c)).ToArray());
}
