using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Branches;
using Viora.Domain.Inventory;
using Viora.Domain.InventoryMovements;
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
using Viora.Domain.RealTimeScheduling;
using Viora.Domain.Services;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Staffs;
using Viora.Domain.Subscriptions;
using Viora.Domain.Users.Customers;
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
    IServiceSettings serviceSettings,
    IStorageSettings storageSettings,
    IOnboardingSettings onboardingSettings,
    ILogger<DevDataSeeder> logger) : IDevDataSeeder
{
    // Seeded reference ids (see PlanData / CountriesData).
    private static readonly Guid StarterPlanId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid EgyptCountryId = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000003");

    // the seeded Organization Id is pinned to a fixed id so its operational data 

    private static readonly Guid OrganizationId = new("c8f5a2b4-9d3e-4e2a-8f7a-2c6f9b1d4e7a");
    // The dev branch is pinned to a fixed id so its operational data (services, staff, schedules)
    // can reference it deterministically across runs and machines.
    private static readonly Guid BranchId = Guid.Parse("9D88C6E0-7B53-434A-82AC-83F1AB9B5C19");

    // Fixed staff ids; shifts below reference these.
    private static readonly Guid[] StaffIds =
    [
        Guid.Parse("e7b4a1c9-2d68-4f35-8b0e-6c9d1f2a7e54"),
        Guid.Parse("f3c1a2d4-5e6b-4f7c-9a8d-1b2c3d4e5f6a"),
        Guid.Parse("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
        Guid.Parse("b1c2d3e4-5f6a-7b8c-9d0e-1f2a3b4c5d6e"),
        Guid.Parse("c1d2e3f4-5a6b-7c8d-9e0f-1a2b3c4d5e6f"),
    ];

    // Fixed schedule ids (Mon–Fri); shifts reference these, and Shift.ScheduleId is an enforced FK,
    // so the schedules must carry exactly these ids.
    private static readonly Guid[] ScheduleIds =
    [
        Guid.Parse("AEC9F604-DB9D-4273-95A5-88DED795AD5D"),
        Guid.Parse("02967DEF-7D97-42EC-ADC6-89AEA592A204"),
        Guid.Parse("A2202230-5353-4B6A-9D54-2754ABEF6867"),
        Guid.Parse("BA7AEF31-2F7C-4A56-BF73-9DC4C195D055"),
        Guid.Parse("AE77FCBF-048F-47CA-A358-88F6BD0B75BC"),
    ];

    // Pinned customer persona id (shared between User and Customer aggregates) so seeded
    // appointments can reference it deterministically. Login: customer@viora.dev / Dev123!Pass
    private static readonly Guid CustomerUserId = Guid.Parse("d4e5f6a7-0002-0000-0000-000000000001");

    // Pinned ids for the Alexandria dental persona (dental.owner@viora.dev).
    private static readonly Guid AlexDentalOrgId     = new("aed00001-0000-0000-0000-000000000001");
    private static readonly Guid AlexDentalBranch1Id  = new("aed00002-0000-0000-0000-000000000001");
    private static readonly Guid AlexDentalBranch2Id  = new("aed00003-0000-0000-0000-000000000001");

    // Pinned ids for the Giza physiotherapy persona (physio.owner@viora.dev).
    private static readonly Guid GizaPhysioOrgId    = new("f1c00001-0000-0000-0000-000000000002");
    private static readonly Guid GizaPhysioBranchId  = new("f1c00002-0000-0000-0000-000000000002");

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
        db.Attach(Role.Customer);

        await SeedActiveOwnerAsync(cancellationToken);
        await SeedPendingOwnerAsync(cancellationToken);
        await SeedAlexDentalAsync(cancellationToken);
        await SeedGizaPhysioAsync(cancellationToken);
        await SeedSharmEyeAsync(cancellationToken);
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
        SetEntityId(organization, OrganizationId); // pin so dev branch can reference it

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
        SetEntityId(branch, BranchId); // pin so dev services/staff/schedules can reference it

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

        // Operational data (services/staff/schedules/shifts) for the branch just created+committed.
        // Kept under this persona's gate so it can never run without its branch present.
        await SeedBranchOperationsAsync(cancellationToken);

        // Inventory (items + movement history) and appointments for the dev branch. Both depend on the
        // branch/services/staff committed above, so they run last under this persona's gate.
        await SeedInventoryAsync(ownerId, organization.Id, cancellationToken);
        await SeedAppointmentsAsync(cancellationToken);

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

    /// <summary>
    /// "Alex Dental" owner: approved application, active org + subscription, two branches in Alexandria
    /// (City Centre and Sidi Gaber) with dental services.
    /// Login: dental.owner@viora.dev / Dev123!Pass
    /// </summary>
    private async Task SeedAlexDentalAsync(CancellationToken cancellationToken)
    {
        const string email = "dental.owner@viora.dev";
        if (await PersonaExistsAsync(email, cancellationToken))
        {
            logger.LogInformation("DevData: dental owner persona ({Email}) already present, skipping.", email);
            return;
        }

        var now = clock.UtcNow;
        var ownerId = await SeedLoginableOwnerAsync(email, "Sara", "Mostafa", cancellationToken);

        var application = Unwrap(BuildApplication(
            ownerId, "Alex Dental Smile", now,
            new List<ServiceType> { ServiceType.DentistryAndOralHealth }), "application");
        Check(application.MarkAccepted(now), "mark application accepted");
        db.Set<OrganizationApplication>().Add(application);
        await SaveSuppressingDomainEventsAsync(cancellationToken);

        var organization = Unwrap(Organization.Create(
            ownerId, EgyptCountryId, "Alex Dental Smile",
            "A leading dental clinic network in Alexandria.",
            "Comprehensive oral health, restorations, and orthodontics.",
            new List<ServiceType> { ServiceType.DentistryAndOralHealth },
            now, ReferralSource.SocialMedia,
            "billing@alexdentalsmile.dev", "support@alexdentalsmile.dev"), "organization");
        SetEntityId(organization, AlexDentalOrgId);
        db.Set<Organization>().Add(organization);

        var subscription = Unwrap(Subscription.Create(StarterPlanId, organization.Id, now, now.AddMonths(1)), "subscription");
        db.Set<Subscription>().Add(subscription);

        var plan = await db.Set<Plan>().FindAsync([StarterPlanId], cancellationToken)
            ?? throw new InvalidOperationException("DevData: Starter plan not seeded; run reference seeder first.");
        db.Set<SubscriptionOrder>().Add(
            Unwrap(SubscriptionOrder.CreateNewSubscriptionOrder(organization.Id, plan, now), "subscription order"));

        // Branch 1 — Alexandria City Centre (El-Horreya Rd)
        var branch1 = Unwrap(Branch.Create(
            organization.Id,
            new Address(5, "El-Horreya Road", "Alexandria", "Alexandria", EgyptCountryId, 21500),
            new Point(29.9187, 31.2001) { SRID = 4326 },
            new BranchEmail("downtown@alexdentalsmile.dev"),
            new List<ServiceType> { ServiceType.DentistryAndOralHealth },
            now), "branch 1");
        SetEntityId(branch1, AlexDentalBranch1Id);
        db.Set<Branch>().Add(branch1);

        // Branch 2 — Sidi Gaber district
        var branch2 = Unwrap(Branch.Create(
            organization.Id,
            new Address(12, "Victor Emmanuel Square", "Alexandria", "Alexandria", EgyptCountryId, 21600),
            new Point(29.9547, 31.2069) { SRID = 4326 },
            new BranchEmail("sidigaber@alexdentalsmile.dev"),
            new List<ServiceType> { ServiceType.DentistryAndOralHealth },
            now), "branch 2");
        SetEntityId(branch2, AlexDentalBranch2Id);
        db.Set<Branch>().Add(branch2);

        await SaveSuppressingDomainEventsAsync(cancellationToken);

        db.Set<Service>().AddRange(
        [
            Unwrap(Service.Create(branch1.Id, "Dental Consultation", "Initial examination and treatment plan.", 30, ServiceType.DentistryAndOralHealth, new Money(150.00m, Currency.Egp), serviceSettings), "Dental Consultation"),
            Unwrap(Service.Create(branch1.Id, "Teeth Cleaning", "Professional scaling and polishing.", 40, ServiceType.DentistryAndOralHealth, new Money(250.00m, Currency.Egp), serviceSettings), "Teeth Cleaning"),
            Unwrap(Service.Create(branch1.Id, "Tooth Filling", "Composite resin restoration.", 60, ServiceType.DentistryAndOralHealth, new Money(350.00m, Currency.Egp), serviceSettings), "Tooth Filling"),
            Unwrap(Service.Create(branch1.Id, "Tooth Extraction", "Simple extraction under local anaesthesia.", 50, ServiceType.DentistryAndOralHealth, new Money(400.00m, Currency.Egp), serviceSettings), "Tooth Extraction"),
            Unwrap(Service.Create(branch2.Id, "Orthodontic Consultation", "Braces or aligner assessment.", 30, ServiceType.DentistryAndOralHealth, new Money(200.00m, Currency.Egp), serviceSettings), "Orthodontic Consultation"),
            Unwrap(Service.Create(branch2.Id, "Teeth Whitening", "Professional in-office whitening.", 60, ServiceType.DentistryAndOralHealth, new Money(800.00m, Currency.Egp), serviceSettings), "Teeth Whitening"),
            Unwrap(Service.Create(branch2.Id, "Dental X-Ray", "Full-mouth digital radiograph.", 20, ServiceType.DentistryAndOralHealth, new Money(180.00m, Currency.Egp), serviceSettings), "Dental X-Ray"),
        ]);

        await SaveSuppressingDomainEventsAsync(cancellationToken);
        logger.LogInformation("DevData: seeded dental owner persona ({Email}).", email);
    }

    /// <summary>
    /// "Giza Physio" owner: approved application, active org + subscription, one branch in Mohandessin
    /// specialising in orthopaedics, sports medicine, and neurology.
    /// Login: physio.owner@viora.dev / Dev123!Pass
    /// </summary>
    private async Task SeedGizaPhysioAsync(CancellationToken cancellationToken)
    {
        const string email = "physio.owner@viora.dev";
        if (await PersonaExistsAsync(email, cancellationToken))
        {
            logger.LogInformation("DevData: physio owner persona ({Email}) already present, skipping.", email);
            return;
        }

        var now = clock.UtcNow;
        var ownerId = await SeedLoginableOwnerAsync(email, "Karim", "Saber", cancellationToken);

        var application = Unwrap(BuildApplication(
            ownerId, "Giza Physio and Rehab", now,
            new List<ServiceType> { ServiceType.OrthopedicSurgery, ServiceType.SportsMedicine, ServiceType.Neurology }), "application");
        Check(application.MarkAccepted(now), "mark application accepted");
        db.Set<OrganizationApplication>().Add(application);
        await SaveSuppressingDomainEventsAsync(cancellationToken);

        var organization = Unwrap(Organization.Create(
            ownerId, EgyptCountryId, "Giza Physio & Rehab",
            "A specialised physiotherapy and rehabilitation centre in Mohandessin.",
            "Orthopaedic assessment, neurological rehab, and sports-injury recovery.",
            new List<ServiceType> { ServiceType.OrthopedicSurgery, ServiceType.SportsMedicine, ServiceType.Neurology },
            now, ReferralSource.GoogleSearch,
            "billing@gizaphysio.dev", "support@gizaphysio.dev"), "organization");
        SetEntityId(organization, GizaPhysioOrgId);
        db.Set<Organization>().Add(organization);

        var subscription = Unwrap(Subscription.Create(StarterPlanId, organization.Id, now, now.AddMonths(1)), "subscription");
        db.Set<Subscription>().Add(subscription);

        var plan = await db.Set<Plan>().FindAsync([StarterPlanId], cancellationToken)
            ?? throw new InvalidOperationException("DevData: Starter plan not seeded; run reference seeder first.");
        db.Set<SubscriptionOrder>().Add(
            Unwrap(SubscriptionOrder.CreateNewSubscriptionOrder(organization.Id, plan, now), "subscription order"));

        // Single branch in Mohandessin, Giza
        var branch = Unwrap(Branch.Create(
            organization.Id,
            new Address(20, "El-Sudan Street", "Giza", "Giza", EgyptCountryId, 12311),
            new Point(31.2000, 30.0600) { SRID = 4326 },
            new BranchEmail("mohandessin@gizaphysio.dev"),
            new List<ServiceType> { ServiceType.OrthopedicSurgery, ServiceType.SportsMedicine },
            now), "branch");
        SetEntityId(branch, GizaPhysioBranchId);
        db.Set<Branch>().Add(branch);

        await SaveSuppressingDomainEventsAsync(cancellationToken);

        db.Set<Service>().AddRange(
        [
            Unwrap(Service.Create(branch.Id, "Physiotherapy Session", "Manual therapy and tailored exercise programme.", 60, ServiceType.OrthopedicSurgery, new Money(300.00m, Currency.Egp), serviceSettings), "Physiotherapy Session"),
            Unwrap(Service.Create(branch.Id, "Sports Injury Assessment", "Biomechanical screening and treatment plan.", 40, ServiceType.SportsMedicine, new Money(350.00m, Currency.Egp), serviceSettings), "Sports Injury Assessment"),
            Unwrap(Service.Create(branch.Id, "Electrotherapy", "TENS/ultrasound pain-relief treatment.", 30, ServiceType.OrthopedicSurgery, new Money(200.00m, Currency.Egp), serviceSettings), "Electrotherapy"),
            Unwrap(Service.Create(branch.Id, "Post-Operative Rehab", "Structured recovery programme after surgery.", 90, ServiceType.OrthopedicSurgery, new Money(500.00m, Currency.Egp), serviceSettings), "Post-Operative Rehab"),
            Unwrap(Service.Create(branch.Id, "Neurological Rehabilitation", "Motor re-education and coordination training.", 60, ServiceType.Neurology, new Money(450.00m, Currency.Egp), serviceSettings), "Neurological Rehabilitation"),
        ]);

        await SaveSuppressingDomainEventsAsync(cancellationToken);
        logger.LogInformation("DevData: seeded physio owner persona ({Email}).", email);
    }

    /// <summary>
    /// "Sharm Eye" owner: submitted application still awaiting review — exercises the pending-with-papers
    /// state for an ophthalmology clinic. Login: eye.owner@viora.dev / Dev123!Pass
    /// </summary>
    private async Task SeedSharmEyeAsync(CancellationToken cancellationToken)
    {
        const string email = "eye.owner@viora.dev";
        if (await PersonaExistsAsync(email, cancellationToken))
        {
            logger.LogInformation("DevData: eye owner persona ({Email}) already present, skipping.", email);
            return;
        }

        var now = clock.UtcNow;
        var ownerId = await SeedLoginableOwnerAsync(email, "Nadia", "Farouk", cancellationToken);

        var application = Unwrap(BuildApplication(
            ownerId, "Sharm Eye Clinic", now,
            new List<ServiceType> { ServiceType.Ophthalmology }), "application");
        db.Set<OrganizationApplication>().Add(application);

        foreach (var type in new[] { LegalPaperType.CommercialRegistration, LegalPaperType.TaxCard })
        {
            var media = await SeedMediaAsync(
                $"{type}.pdf", $"legal-papers/{application.Id}/{Guid.NewGuid()}.pdf",
                "application/pdf", PlaceholderPdf, organizationId: null, cancellationToken);
            var paper = Unwrap(LegalPaper.Create(
                media.Id, application.Id, $"{type} document",
                AcceptanceStatus.UnderReview, type, now, now.AddYears(1)), $"legal paper {type}");
            db.Set<LegalPaper>().Add(paper);
        }

        await SaveSuppressingDomainEventsAsync(cancellationToken);
        logger.LogInformation("DevData: seeded eye owner persona ({Email}).", email);
    }

    private Result<OrganizationApplication> BuildApplication(
        Guid ownerId, string proposedName, DateTime now,
        List<ServiceType>? serviceTypes = null) =>
        OrganizationApplication.Create(
            ownerId, EgyptCountryId,
            new Name(proposedName),
            new Letter("We are a healthcare provider seeking to onboard with Viora."),
            new About($"{proposedName} provides outpatient care."),
            serviceTypes ?? new List<ServiceType> { ServiceType.Cardiology, ServiceType.Dermatology },
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

    /// <summary>
    /// Dev-only operational data for the pinned branch: services, staff (each assigned to the branch and
    /// presenting a couple of services), a weekly schedule, and a shift per staff per weekday. Moved here
    /// from the production reference seeder (it must never go live). Gated on services already existing so
    /// re-runs are a no-op.
    /// </summary>
    private async Task SeedBranchOperationsAsync(CancellationToken cancellationToken)
    {
        if (await db.Set<Service>().AnyAsync(cancellationToken))
        {
            logger.LogInformation("DevData: branch operations already seeded, skipping.");
            return;
        }

        // Services + staff for the dev branch (branch already committed by the active-owner persona).
        var services = BuildServices().ToList();
        db.Set<Service>().AddRange(services);

        // The branch these staff belong to; needed to seed the Staff<->Branch (StaffBranch) join.
        var branch = await db.Set<Branch>().FindAsync([BranchId], cancellationToken)
            ?? throw new InvalidOperationException("DevData: dev branch not found; the active-owner persona must run first.");

        for (var i = 0; i < StaffIds.Length; i++)
        {
            var staff = Staff.SeedActiveStaff(
                StaffIds[i],
                OrganizationId,
                "John",
                "Doe",
                clock.UtcNow,
                new DateOnly(1990, 1, 1),
                Domain.Staffs.Internal.Gender.Male,
                new PhoneNumber("+1234567890"));

            // Staff <-> Branch: every dev staff member works at the pinned branch.
            staff.AssignBranches([branch]);

            // Staff <-> Service: each member "presents" two of the branch's services (rotating pair).
            staff.AssignServices([services[i % services.Count], services[(i + 1) % services.Count]]);

            db.Set<Staff>().Add(staff);
        }

        // Weekly schedule (Mon–Fri), pinned to known ids so the shifts below resolve their FK.
        var days = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
        };
        for (var i = 0; i < days.Length; i++)
        {
            var schedule = Schedule.Create(BranchId, days[i]);
            SetEntityId(schedule, ScheduleIds[i]);
            db.Set<Schedule>().Add(schedule);
        }

        // Persist schedules first — Shift.ScheduleId is an enforced FK to Schedule.
        await SaveSuppressingDomainEventsAsync(cancellationToken);

        // A shift for every staff member on every weekday, so each staff has a full weekly schedule
        // (the schedule returned when querying a staff member's shifts).
        var shifts =
            from scheduleId in ScheduleIds
            from staffId in StaffIds
            select Shift.Create(scheduleId, new TimeOnly(9, 0), new TimeOnly(17, 0), staffId);
        db.Set<Shift>().AddRange(shifts);

        await SaveSuppressingDomainEventsAsync(cancellationToken);
        logger.LogInformation("DevData: seeded branch operations (services, staff, schedules, shifts).");
    }

    private IEnumerable<Service> BuildServices() =>
    [
        Unwrap(Service.Create(BranchId, "Haircut", "A basic haircut service.", 30, ServiceType.Cardiology, new Money(20.00m, Currency.Egp), serviceSettings), "service Haircut"),
        Unwrap(Service.Create(BranchId, "Hair Coloring", "A professional hair coloring service.", 90, ServiceType.Dermatology, new Money(100.00m, Currency.Egp), serviceSettings), "service Hair Coloring"),
        Unwrap(Service.Create(BranchId, "Manicure", "A complete manicure service.", 40, ServiceType.Otolaryngology, new Money(30.00m, Currency.Egp), serviceSettings), "service Manicure"),
        Unwrap(Service.Create(BranchId, "Pedicure", "A complete pedicure service.", 60, ServiceType.Endocrinology, new Money(40.00m, Currency.Egp), serviceSettings), "service Pedicure"),
        Unwrap(Service.Create(BranchId, "Facial", "A complete facial service.", 60, ServiceType.Endocrinology, new Money(50.00m, Currency.Egp), serviceSettings), "service Facial"),
        Unwrap(Service.Create(BranchId, "Massage", "A relaxing massage service.", 60, ServiceType.Dermatology, new Money(70.00m, Currency.Egp), serviceSettings), "service Massage"),
        Unwrap(Service.Create(BranchId, "Makeup Application", "A professional makeup application service.", 60, ServiceType.Cardiology, new Money(80.00m, Currency.Egp), serviceSettings), "service Makeup Application"),
        Unwrap(Service.Create(BranchId, "Waxing", "A complete waxing service.", 30, ServiceType.Cardiology, new Money(25.00m, Currency.Egp), serviceSettings), "service Waxing"),
    ];

    /// <summary>
    /// Dev-only inventory for the pinned branch: a handful of items (one with a placeholder image,
    /// one intentionally at/below its threshold) plus a realistic movement history per item.
    /// Gated on any inventory item already existing for the branch so re-runs are a no-op.
    /// </summary>
    private async Task SeedInventoryAsync(Guid performedByUserId, Guid organizationId, CancellationToken cancellationToken)
    {
        if (await db.Set<InventoryItem>().AnyAsync(item => item.BranchId == BranchId, cancellationToken))
        {
            logger.LogInformation("DevData: inventory already seeded, skipping.");
            return;
        }

        var now = clock.UtcNow;

        // (name, notes, quantity, minimumThreshold, withImage)
        var specs = new[]
        {
            ("Surgical Gloves (Box)", "Latex-free, medium.", 120, 30, true),
            ("Disposable Syringes 5ml", "Single-use, sterile.", 75, 25, false),
            ("Gauze Rolls", "Sterile cotton gauze.", 18, 20, false),   // below threshold -> exercises low-stock UI
            ("Alcohol Swabs (Pack)", "70% isopropyl.", 200, 50, false),
            ("Examination Couch Paper", "Roll, 60cm.", 40, 15, false),
        };

        foreach (var (name, notes, quantity, threshold, withImage) in specs)
        {
            Guid? imageId = null;
            if (withImage)
            {
                var image = await SeedMediaAsync(
                    "inventory-item.png", $"inventory/{BranchId}/{Guid.NewGuid()}.png",
                    "image/png", PlaceholderPng, organizationId, cancellationToken);
                imageId = image.Id;
            }

            var item = InventoryItem.Create(BranchId, name, notes, quantity, threshold, imageId);
            db.Set<InventoryItem>().Add(item);

            // A movement history that nets to the current quantity: an initial restock, then some consumption.
            var consumed = Math.Max(0, (quantity / 3));
            var initialRestock = quantity + consumed;

            db.Set<InventoryMovement>().Add(
                Unwrap(InventoryMovement.Restock(item.Id, performedByUserId, initialRestock, now.AddDays(-14)), $"restock {name}"));

            if (consumed > 0)
            {
                db.Set<InventoryMovement>().Add(
                    Unwrap(InventoryMovement.Consume(item.Id, performedByUserId, consumed, now.AddDays(-3)), $"consume {name}"));
            }
        }

        await SaveSuppressingDomainEventsAsync(cancellationToken);
        logger.LogInformation("DevData: seeded inventory items and movement history.");
    }

    /// <summary>
    /// Dev-only appointments for the pinned branch across a spread of statuses, dates, staff and payment
    /// methods. Seeds a loginable customer to satisfy the appointment -> customer FK. Gated on any
    /// appointment already existing for the branch so re-runs are a no-op.
    /// </summary>
    private async Task SeedAppointmentsAsync(CancellationToken cancellationToken)
    {
        if (await db.Set<Appointment>().AnyAsync(appointment => appointment.BranchId == BranchId, cancellationToken))
        {
            logger.LogInformation("DevData: appointments already seeded, skipping.");
            return;
        }

        var customerId = await SeedLoginableCustomerAsync("customer@viora.dev", "Mona", "Said", cancellationToken);

        var serviceIds = await db.Set<Service>()
            .Where(service => service.BranchId == BranchId)
            .Select(service => service.Id)
            .Take(4)
            .ToListAsync(cancellationToken);

        if (serviceIds.Count == 0)
        {
            logger.LogWarning("DevData: no services found for the dev branch; skipping appointment seeding.");
            return;
        }

        var now = clock.UtcNow;

        // (dayOffset, status, payMethod, platform, durationMinutes)
        var plan = new (int DayOffset, CustomerStatus Status, PaymentMethod PayMethod, Platform Platform, int Minutes)[]
        {
            (-7, CustomerStatus.Completed, PaymentMethod.Cash,   Platform.Web,    30),
            (-2, CustomerStatus.NoShow,    PaymentMethod.Online, Platform.Mobile, 45),
            ( 0, CustomerStatus.InProgress,PaymentMethod.Wallet, Platform.Web,    30),
            ( 1, CustomerStatus.NotArrived,PaymentMethod.Cash,   Platform.Mobile, 60),
            ( 3, CustomerStatus.Waiting,   PaymentMethod.Online, Platform.Web,    30),
            ( 5, CustomerStatus.NotArrived,PaymentMethod.Cash,   Platform.Mobile, 90),
        };

        for (var i = 0; i < plan.Length; i++)
        {
            var (dayOffset, status, payMethod, platform, minutes) = plan[i];
            var serviceId = serviceIds[i % serviceIds.Count];
            var staffId = StaffIds[i % StaffIds.Length];

            var appointment = Appointment.Book(
                customerId: customerId,
                serviceId: serviceId,
                staffId: staffId,
                branchId: BranchId,
                paymentId: null,
                reservationDate: now.AddDays(dayOffset),
                appointmentQueueNumber: i + 1,
                payMethod: payMethod,
                status: status,
                createdBy: Creator.Customer,
                requestPlatform: platform,
                estimatedDuration: TimeSpan.FromMinutes(minutes),
                createdAt: now.AddDays(dayOffset - 1));

            db.Set<Appointment>().Add(appointment);
        }

        await SaveSuppressingDomainEventsAsync(cancellationToken);
        logger.LogInformation("DevData: seeded appointments for the dev branch.");
    }

    /// <summary>Creates a User with local credentials + Customer role, then the Customer aggregate (shared id).</summary>
    private async Task<Guid> SeedLoginableCustomerAsync(string email, string firstName, string lastName, CancellationToken cancellationToken)
    {
        if (await PersonaExistsAsync(email, cancellationToken))
        {
            var existing = await db.Set<AuthIdentity>()
                .Where(identity => identity.Provider == "local" && identity.ProviderKey == email.ToLowerInvariant().Trim())
                .Select(identity => identity.UserId)
                .FirstAsync(cancellationToken);
            return existing;
        }

        var now = clock.UtcNow;

        var personalInfo = new PersonalInfo(firstName, lastName, new DateOnly(1995, 6, 15), Gender.Female);
        var user = User.Create(personalInfo, new Domain.Users.Internal.Email(email), now);
        SetEntityId(user, CustomerUserId); // pin so the customer aggregate shares the id

        db.Set<LocalCredential>().Add(new LocalCredential(user.Id, hasher.Hash(DefaultPassword)));

        var identity = AuthIdentity.Create("local", user.Id, user.Email.Value, now);
        user.LinkIdentity(identity);
        db.Set<AuthIdentity>().Add(identity);

        Check(user.BecomeCustomer(Role.Customer), "promote user to customer");
        db.Set<User>().Add(user);
        await SaveSuppressingDomainEventsAsync(cancellationToken);

        db.Set<Customer>().Add(Customer.Create(
            user.Id,
            new UserName(Slug(firstName + lastName)),
            personalInfo,
            now,
            new[] { new PhoneNumber("+201000000000") },
            new[] { new Domain.Users.Internal.Email(email) }));
        await SaveSuppressingDomainEventsAsync(cancellationToken);

        return user.Id;
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

    // Entity.Id is init-only and the factories generate their own ids; reflection lets us pin
    // deterministic ids for dev seeding without adding id parameters to the domain factories.
    // EF persists the provided (non-empty) key value for these Guid keys.
    private static void SetEntityId(Entity entity, Guid id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id);
}
