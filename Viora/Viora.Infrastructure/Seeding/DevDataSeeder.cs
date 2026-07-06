using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Security;

namespace Viora.Infrastructure.Seeding;

public interface IDevDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Dev-only scenario data, seeded with RAW SQL (no EF entity graphs). Every INSERT below is hand-written
/// against the physical schema so the whole set is one readable batch of SQL rather than object construction.
///
/// Design:
/// - Realistic Egyptian-healthcare theme; at least five rows for every domain entity.
/// - References canonical reference data seeded by <see cref="DatabaseSeeder"/> (Countries, Roles, Plans,
///   Features, LimitedFeatures, PlanLimitedFeatures, LimitedFeatureAddons) by their fixed ids — it never
///   re-inserts them.
/// - The only C# services used are value PRODUCERS that cannot be expressed as SQL literals:
///     * <see cref="IHasher"/>  -> a valid password hash so every seeded login works (password: Dev123!Pass).
///     * <see cref="ICipher"/>  -> the AES-encrypted Facebook Page token stored at rest.
///     * <see cref="IStorageService"/> -> placeholder blobs so media download endpoints serve a real file.
/// - Idempotent: gated on a pinned sentinel organization id; re-running is a no-op. All inserts run inside a
///   single transaction, so a mid-way failure rolls the whole batch back and leaves a clean slate to retry.
///
/// Intentionally NOT seeded (runtime/plumbing, not test data): RefreshToken, StaffRefreshTokens,
/// ScheduledDomainEvents (outbox). Reference tables owned by <see cref="DatabaseSeeder"/> are excluded per above.
///
/// GUID legend (PREFIX-0000-0000-0000-0000000000NN):
///   1111 Owner-user   2222 Customer-user  3333 Organization  4444 Application  5555 Branch    6666 Service
///   7777 Staff        8888 MediaFile      9999 Subscription   aaaa* operational (appointments, schedules,
///   prescriptions, medical records, invoices, wallets, wallet tx/promises)   bbbb* (marketing, forms, form
///   submissions, inventory, chat sessions, addon orders, subscription addons).
/// </summary>
internal sealed class DevDataSeeder(
    ApplicationDbContext db,
    IHasher hasher,
    ICipher cipher,
    IStorageService storage,
    ILogger<DevDataSeeder> logger) : IDevDataSeeder
{
    private const string DevPassword = "Dev123!Pass";

    // Pinned sentinel: presence of the first organization means the dev set is already seeded.
    private const string Sentinel = "33330000-0000-0000-0000-000000000001";
    private const string EgyptCountryId = "a1b2c3d4-0001-0000-0000-000000000003";
    private const string StarterPlanId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    private const string ProPlanId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    private const string EnterprisePlanId = "cccccccc-cccc-cccc-cccc-cccccccccccc";

    // A minimal valid 1x1 PNG and a tiny valid PDF used as placeholder blobs.
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+M8AAAMBAQDJ/pLvAAAAAElFTkSuQmCC");

    private static readonly byte[] Pdf = System.Text.Encoding.ASCII.GetBytes(
        "%PDF-1.1\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n" +
        "2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n" +
        "3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 200 200]>>endobj\n" +
        "trailer<</Root 1 0 R>>\n%%EOF");

    // Media files: blobs are written to storage from this list AND their rows are inserted (kept in sync here so
    // the download endpoints always find a matching blob). Id, storage key, display name, mime, isPdf.
    private static readonly (string Id, string Key, string Name, string Mime, bool Pdf)[] Media =
    [
        ("88880000-0000-0000-0000-000000000001", "dev/logos/nile-care.png",       "nile-care-logo.png",       "image/png",       false),
        ("88880000-0000-0000-0000-000000000002", "dev/logos/alex-dental.png",     "alex-dental-logo.png",     "image/png",       false),
        ("88880000-0000-0000-0000-000000000003", "dev/logos/giza-physio.png",     "giza-physio-logo.png",     "image/png",       false),
        ("88880000-0000-0000-0000-000000000004", "dev/logos/cairo-derma.png",     "cairo-derma-logo.png",     "image/png",       false),
        ("88880000-0000-0000-0000-000000000005", "dev/logos/luxor-ortho.png",     "luxor-ortho-logo.png",     "image/png",       false),
        ("88880000-0000-0000-0000-000000000006", "dev/legal/nile-aoa.pdf",        "articles-of-association.pdf","application/pdf", true),
        ("88880000-0000-0000-0000-000000000007", "dev/legal/nile-cr.pdf",         "commercial-registration.pdf","application/pdf",true),
        ("88880000-0000-0000-0000-000000000008", "dev/legal/alex-cr.pdf",         "commercial-registration.pdf","application/pdf",true),
        ("88880000-0000-0000-0000-000000000009", "dev/legal/giza-cr.pdf",         "commercial-registration.pdf","application/pdf",true),
        ("88880000-0000-0000-0000-000000000010", "dev/legal/cairo-tax.pdf",       "tax-card.pdf",             "application/pdf",  true),
        ("88880000-0000-0000-0000-000000000011", "dev/legal/luxor-cr.pdf",        "commercial-registration.pdf","application/pdf",true),
        ("88880000-0000-0000-0000-000000000012", "dev/profiles/mona.png",         "mona-said.png",            "image/png",       false),
        ("88880000-0000-0000-0000-000000000013", "dev/profiles/youssef.png",      "youssef-ali.png",          "image/png",       false),
        ("88880000-0000-0000-0000-000000000014", "dev/profiles/laila.png",        "laila-ibrahim.png",        "image/png",       false),
        ("88880000-0000-0000-0000-000000000015", "dev/templates/nile-rx.png",     "nile-rx-template.png",     "image/png",       false),
        ("88880000-0000-0000-0000-000000000016", "dev/templates/alex-rx.png",     "alex-rx-template.png",     "image/png",       false),
        ("88880000-0000-0000-0000-000000000017", "dev/templates/giza-rx.png",     "giza-rx-template.png",     "image/png",       false),
        ("88880000-0000-0000-0000-000000000018", "dev/templates/cairo-rx.png",    "cairo-rx-template.png",    "image/png",       false),
        ("88880000-0000-0000-0000-000000000019", "dev/templates/luxor-rx.png",    "luxor-rx-template.png",    "image/png",       false),
        ("88880000-0000-0000-0000-000000000020", "dev/inventory/gloves.png",      "surgical-gloves.png",      "image/png",       false),
        ("88880000-0000-0000-0000-000000000021", "dev/inventory/syringes.png",    "syringes.png",             "image/png",       false),
        ("88880000-0000-0000-0000-000000000022", "dev/gallery/nile-front.png",    "nile-front.png",           "image/png",       false),
        ("88880000-0000-0000-0000-000000000023", "dev/gallery/alex-front.png",    "alex-front.png",           "image/png",       false),
        ("88880000-0000-0000-0000-000000000024", "dev/gallery/giza-front.png",    "giza-front.png",           "image/png",       false),
        ("88880000-0000-0000-0000-000000000025", "dev/gallery/svc-cardio.png",    "cardiology-room.png",      "image/png",       false),
        ("88880000-0000-0000-0000-000000000026", "dev/gallery/svc-dental.png",    "dental-room.png",          "image/png",       false),
        ("88880000-0000-0000-0000-000000000027", "dev/forms/scan-1.png",          "medical-scan.png",         "image/png",       false),
        ("88880000-0000-0000-0000-000000000028", "dev/forms/scan-2.pdf",          "previous-prescription.pdf","application/pdf",  true),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var alreadySeeded = await db.Database
            .SqlQueryRaw<int>($"SELECT COUNT(1) AS Value FROM Organizations WHERE Id = '{Sentinel}'")
            .FirstAsync(cancellationToken);

        if (alreadySeeded > 0)
        {
            logger.LogInformation("DevData: sentinel organization present, skipping raw-SQL dev seed.");
            return;
        }

        // Produce the few values that cannot be SQL literals.
        var passwordHash = hasher.Hash(DevPassword).Replace("'", "''");
        var facebookToken = cipher.Encrypt("EAAG-DEV-PLACEHOLDER-PAGE-TOKEN").Replace("'", "''");

        // Write placeholder blobs so media download endpoints serve real files.
        foreach (var m in Media)
            await storage.SaveFileAsync(new MemoryStream(m.Pdf ? Pdf : Png), m.Key, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await ExecAsync("media files", BuildMediaSql(), cancellationToken);
            await ExecAsync("users & identity", BuildUsersSql(passwordHash), cancellationToken);
            await ExecAsync("owners & customers", BuildPeopleSql(), cancellationToken);
            await ExecAsync("organizations, applications & legal papers", BuildOrganizationsSql(), cancellationToken);
            await ExecAsync("subscriptions, orders, invoices & usage", BuildBillingSql(), cancellationToken);
            await ExecAsync("branches", BuildBranchesSql(), cancellationToken);
            await ExecAsync("services", BuildServicesSql(), cancellationToken);
            await ExecAsync("staff & assignments", BuildStaffSql(passwordHash), cancellationToken);
            await ExecAsync("schedules & shifts", BuildSchedulingSql(), cancellationToken);
            await ExecAsync("appointments, delays & reminders", BuildAppointmentsSql(), cancellationToken);
            await ExecAsync("forms, prescriptions & templates", BuildClinicalSql(), cancellationToken);
            await ExecAsync("inventory, feedback, records, notifications, visits & suspensions", BuildOperationsSql(), cancellationToken);
            await ExecAsync("wallets, transactions & promises", BuildWalletsSql(), cancellationToken);
            await ExecAsync("marketing & chat sessions", BuildMarketingSql(facebookToken), cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("DevData: raw-SQL dev seed completed.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "DevData: raw-SQL dev seed failed and was rolled back.");
            throw;
        }
    }

    private async Task ExecAsync(string label, string sql, CancellationToken cancellationToken)
    {
        // Run via a raw ADO command rather than ExecuteSqlRaw: EF treats the SQL as a composite format
        // string ({0}-style parameters), so the literal '{' / '}' in our JSON values (form fields, chat
        // history) would be misparsed as format items. A plain DbCommand executes the text verbatim.
        var connection = db.Database.GetDbConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
        await command.ExecuteNonQueryAsync(cancellationToken);
        logger.LogInformation("DevData: seeded {Label}.", label);
    }

    // ---------------------------------------------------------------------------------------------------------
    // MEDIA FILES (OrganizationId left NULL here; org-scoped ones are attributed after Organizations exist).
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildMediaSql()
    {
        var rows = string.Join(",\n", Media.Select(m =>
        {
            var size = (m.Pdf ? Pdf.Length : Png.Length);
            return $"('{m.Id}', N'{m.Key}', N'{m.Mime}', N'{m.Name}', NULL, {size}, '2026-02-01T08:00:00')";
        }));

        return $@"
INSERT INTO [MediaFiles] ([Id], [Key], [MimeType], [Name], [OrganizationId], [SizeInBytes], [UploadedAtUtc]) VALUES
{rows};";
    }

    // ---------------------------------------------------------------------------------------------------------
    // USERS + LOCAL CREDENTIALS + AUTH IDENTITIES + USER ROLES  (Role ids: Registered=1, Owner=2, Customer=4)
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildUsersSql(string passwordHash) => $@"
INSERT INTO [Users] ([Id], [CreatedAt], [Email], [IsEmailVerified], [LastLoginAt], [Status], [PersonalInfo_DateOfBirth], [PersonalInfo_FirstName], [PersonalInfo_Gender], [PersonalInfo_LastName]) VALUES
('11110000-0000-0000-0000-000000000001', '2026-01-10T09:00:00', N'aya.hassan@viora.dev',    1, NULL, N'Active', '1985-03-12', N'Aya',   N'Female', N'Hassan'),
('11110000-0000-0000-0000-000000000002', '2026-01-10T09:00:00', N'sara.mostafa@viora.dev',  1, NULL, N'Active', '1988-07-22', N'Sara',  N'Female', N'Mostafa'),
('11110000-0000-0000-0000-000000000003', '2026-01-10T09:00:00', N'karim.saber@viora.dev',   1, NULL, N'Active', '1983-11-05', N'Karim', N'Male',   N'Saber'),
('11110000-0000-0000-0000-000000000004', '2026-01-10T09:00:00', N'mona.adel@viora.dev',     1, NULL, N'Active', '1990-01-30', N'Mona',  N'Female', N'Adel'),
('11110000-0000-0000-0000-000000000005', '2026-01-10T09:00:00', N'hassan.tarek@viora.dev',  1, NULL, N'Active', '1979-09-17', N'Hassan',N'Male',   N'Tarek'),
('11110000-0000-0000-0000-000000000006', '2026-01-12T09:00:00', N'omar.khaled@viora.dev',   1, NULL, N'Active', '1992-05-08', N'Omar',  N'Male',   N'Khaled'),
('11110000-0000-0000-0000-000000000007', '2026-01-12T09:00:00', N'nadia.farouk@viora.dev',  1, NULL, N'Active', '1987-02-14', N'Nadia', N'Female', N'Farouk'),
('22220000-0000-0000-0000-000000000001', '2026-02-01T09:00:00', N'mona.said@viora.dev',     1, NULL, N'Active', '1995-06-15', N'Mona',  N'Female', N'Said'),
('22220000-0000-0000-0000-000000000002', '2026-02-01T09:00:00', N'youssef.ali@viora.dev',   1, NULL, N'Active', '1991-04-10', N'Youssef',N'Male',  N'Ali'),
('22220000-0000-0000-0000-000000000003', '2026-02-01T09:00:00', N'laila.ibrahim@viora.dev', 1, NULL, N'Active', '1998-12-01', N'Laila', N'Female', N'Ibrahim'),
('22220000-0000-0000-0000-000000000004', '2026-02-01T09:00:00', N'ahmed.samir@viora.dev',   1, NULL, N'Active', '1989-08-25', N'Ahmed', N'Male',   N'Samir'),
('22220000-0000-0000-0000-000000000005', '2026-02-01T09:00:00', N'salma.nabil@viora.dev',   1, NULL, N'Active', '2000-03-19', N'Salma', N'Female', N'Nabil'),
('22220000-0000-0000-0000-000000000006', '2026-02-01T09:00:00', N'tarek.fouad@viora.dev',   1, NULL, N'Active', '1993-10-07', N'Tarek', N'Male',   N'Fouad');

INSERT INTO [LocalCredential] ([UserId], [FailedLoginAttempts], [HashVersion], [HashedPassword], [LastChangedAt]) VALUES
('11110000-0000-0000-0000-000000000001', 0, 1, N'{passwordHash}', NULL),
('11110000-0000-0000-0000-000000000002', 0, 1, N'{passwordHash}', NULL),
('11110000-0000-0000-0000-000000000003', 0, 1, N'{passwordHash}', NULL),
('11110000-0000-0000-0000-000000000004', 0, 1, N'{passwordHash}', NULL),
('11110000-0000-0000-0000-000000000005', 0, 1, N'{passwordHash}', NULL),
('11110000-0000-0000-0000-000000000006', 0, 1, N'{passwordHash}', NULL),
('11110000-0000-0000-0000-000000000007', 0, 1, N'{passwordHash}', NULL),
('22220000-0000-0000-0000-000000000001', 0, 1, N'{passwordHash}', NULL),
('22220000-0000-0000-0000-000000000002', 0, 1, N'{passwordHash}', NULL),
('22220000-0000-0000-0000-000000000003', 0, 1, N'{passwordHash}', NULL),
('22220000-0000-0000-0000-000000000004', 0, 1, N'{passwordHash}', NULL),
('22220000-0000-0000-0000-000000000005', 0, 1, N'{passwordHash}', NULL),
('22220000-0000-0000-0000-000000000006', 0, 1, N'{passwordHash}', NULL);

INSERT INTO [AuthIdentities] ([Id], [CreatedAt], [LastLoginAt], [Provider], [ProviderKey], [UserId]) VALUES
('a1de0000-0000-0000-0000-000000000001', '2026-01-10T09:00:00', NULL, N'local', N'aya.hassan@viora.dev',    '11110000-0000-0000-0000-000000000001'),
('a1de0000-0000-0000-0000-000000000002', '2026-01-10T09:00:00', NULL, N'local', N'sara.mostafa@viora.dev',  '11110000-0000-0000-0000-000000000002'),
('a1de0000-0000-0000-0000-000000000003', '2026-01-10T09:00:00', NULL, N'local', N'karim.saber@viora.dev',   '11110000-0000-0000-0000-000000000003'),
('a1de0000-0000-0000-0000-000000000004', '2026-01-10T09:00:00', NULL, N'local', N'mona.adel@viora.dev',     '11110000-0000-0000-0000-000000000004'),
('a1de0000-0000-0000-0000-000000000005', '2026-01-10T09:00:00', NULL, N'local', N'hassan.tarek@viora.dev',  '11110000-0000-0000-0000-000000000005'),
('a1de0000-0000-0000-0000-000000000006', '2026-01-12T09:00:00', NULL, N'local', N'omar.khaled@viora.dev',   '11110000-0000-0000-0000-000000000006'),
('a1de0000-0000-0000-0000-000000000007', '2026-01-12T09:00:00', NULL, N'local', N'nadia.farouk@viora.dev',  '11110000-0000-0000-0000-000000000007'),
('a1de0000-0000-0000-0000-000000000008', '2026-02-01T09:00:00', NULL, N'local', N'mona.said@viora.dev',     '22220000-0000-0000-0000-000000000001'),
('a1de0000-0000-0000-0000-000000000009', '2026-02-01T09:00:00', NULL, N'local', N'youssef.ali@viora.dev',   '22220000-0000-0000-0000-000000000002'),
('a1de0000-0000-0000-0000-000000000010', '2026-02-01T09:00:00', NULL, N'local', N'laila.ibrahim@viora.dev', '22220000-0000-0000-0000-000000000003'),
('a1de0000-0000-0000-0000-000000000011', '2026-02-01T09:00:00', NULL, N'local', N'ahmed.samir@viora.dev',   '22220000-0000-0000-0000-000000000004'),
('a1de0000-0000-0000-0000-000000000012', '2026-02-01T09:00:00', NULL, N'local', N'salma.nabil@viora.dev',   '22220000-0000-0000-0000-000000000005'),
('a1de0000-0000-0000-0000-000000000013', '2026-02-01T09:00:00', NULL, N'local', N'tarek.fouad@viora.dev',   '22220000-0000-0000-0000-000000000006');

INSERT INTO [UserRole] ([UserId], [RoleId]) VALUES
('11110000-0000-0000-0000-000000000001', 2),
('11110000-0000-0000-0000-000000000002', 2),
('11110000-0000-0000-0000-000000000003', 2),
('11110000-0000-0000-0000-000000000004', 2),
('11110000-0000-0000-0000-000000000005', 2),
('11110000-0000-0000-0000-000000000006', 2),
('11110000-0000-0000-0000-000000000007', 2),
('22220000-0000-0000-0000-000000000001', 4),
('22220000-0000-0000-0000-000000000002', 4),
('22220000-0000-0000-0000-000000000003', 4),
('22220000-0000-0000-0000-000000000004', 4),
('22220000-0000-0000-0000-000000000005', 4),
('22220000-0000-0000-0000-000000000006', 4);";

    // ---------------------------------------------------------------------------------------------------------
    // OWNERS + CUSTOMERS (share the User Id; PersonalInfo stored inline; contact lists are ';'-joined strings)
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildPeopleSql() => $@"
INSERT INTO [Owners] ([Id], [BecameOwnerAt], [NationalityId], [DateOfBirth], [FirstName], [Gender], [LastName]) VALUES
('11110000-0000-0000-0000-000000000001', '2026-01-15T09:00:00', '{EgyptCountryId}', '1985-03-12', N'Aya',   N'Female', N'Hassan'),
('11110000-0000-0000-0000-000000000002', '2026-01-16T09:00:00', '{EgyptCountryId}', '1988-07-22', N'Sara',  N'Female', N'Mostafa'),
('11110000-0000-0000-0000-000000000003', '2026-01-17T09:00:00', '{EgyptCountryId}', '1983-11-05', N'Karim', N'Male',   N'Saber'),
('11110000-0000-0000-0000-000000000004', '2026-01-18T09:00:00', '{EgyptCountryId}', '1990-01-30', N'Mona',  N'Female', N'Adel'),
('11110000-0000-0000-0000-000000000005', '2026-01-19T09:00:00', '{EgyptCountryId}', '1979-09-17', N'Hassan',N'Male',   N'Tarek'),
('11110000-0000-0000-0000-000000000006', '2026-01-20T09:00:00', '{EgyptCountryId}', '1992-05-08', N'Omar',  N'Male',   N'Khaled'),
('11110000-0000-0000-0000-000000000007', '2026-01-21T09:00:00', '{EgyptCountryId}', '1987-02-14', N'Nadia', N'Female', N'Farouk');

INSERT INTO [Customers] ([Id], [Emails], [JoinedAt], [MedicalRecordId], [PhoneNumbers], [ProfilePicId], [UserName], [PersonalInfo_DateOfBirth], [PersonalInfo_FirstName], [PersonalInfo_Gender], [PersonalInfo_LastName]) VALUES
('22220000-0000-0000-0000-000000000001', N'mona.said@viora.dev',     '2026-02-02T09:00:00', 'a5e50000-0000-0000-0000-000000000001', N'+201000000001', '88880000-0000-0000-0000-000000000012', N'monasaid',     '1995-06-15', N'Mona',    N'Female', N'Said'),
('22220000-0000-0000-0000-000000000002', N'youssef.ali@viora.dev',   '2026-02-03T09:00:00', 'a5e50000-0000-0000-0000-000000000002', N'+201000000002', '88880000-0000-0000-0000-000000000013', N'youssefali',  '1991-04-10', N'Youssef', N'Male',   N'Ali'),
('22220000-0000-0000-0000-000000000003', N'laila.ibrahim@viora.dev', '2026-02-04T09:00:00', 'a5e50000-0000-0000-0000-000000000003', N'+201000000003', '88880000-0000-0000-0000-000000000014', N'lailaibrahim','1998-12-01', N'Laila',   N'Female', N'Ibrahim'),
('22220000-0000-0000-0000-000000000004', N'ahmed.samir@viora.dev',   '2026-02-05T09:00:00', 'a5e50000-0000-0000-0000-000000000004', N'+201000000004', NULL,                                   N'ahmedsamir',  '1989-08-25', N'Ahmed',   N'Male',   N'Samir'),
('22220000-0000-0000-0000-000000000005', N'salma.nabil@viora.dev',   '2026-02-06T09:00:00', 'a5e50000-0000-0000-0000-000000000005', N'+201000000005', NULL,                                   N'salmanabil',  '2000-03-19', N'Salma',   N'Female', N'Nabil'),
('22220000-0000-0000-0000-000000000006', N'tarek.fouad@viora.dev',   '2026-02-07T09:00:00', NULL,                                   N'+201000000006', NULL,                                   N'tarekfouad',  '1993-10-07', N'Tarek',   N'Male',   N'Fouad');";

    // ---------------------------------------------------------------------------------------------------------
    // ORGANIZATIONS (+ attribute org-scoped media) + APPLICATIONS + LEGAL PAPERS
    //   Statuses: G1-G3 Active, G4/G5 Suspended (they carry a Suspension row below). ServicesProvided = JSON array.
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildOrganizationsSql() => $@"
INSERT INTO [Organizations] ([Id], [CountryId], [JoinedOnUtc], [LogoId], [OwnerId], [ReferralSource], [ServicesProvided], [Status], [About], [BillingEmail], [RatingAverage], [RatingCount], [ServiceDescription], [SupportEmail], [Name], [Subdomain]) VALUES
('33330000-0000-0000-0000-000000000001', '{EgyptCountryId}', '2026-02-01T10:00:00', '88880000-0000-0000-0000-000000000001', '11110000-0000-0000-0000-000000000001', N'Website',      N'[""Cardiology"",""Dermatology""]', N'Active',    N'A multi-specialty clinic in Cairo.',                  N'billing@nile-care.dev',   4.6, 128, N'General and specialized outpatient care.',      N'support@nile-care.dev',   N'Nile Care Clinic',  N'nile-care'),
('33330000-0000-0000-0000-000000000002', '{EgyptCountryId}', '2026-02-02T10:00:00', '88880000-0000-0000-0000-000000000002', '11110000-0000-0000-0000-000000000002', N'SocialMedia',  N'[""Dentistry & Oral Health""]',    N'Active',    N'A leading dental clinic network in Alexandria.',      N'billing@alex-dental.dev', 4.8,  94, N'Comprehensive oral health and orthodontics.',   N'support@alex-dental.dev', N'Alex Dental Smile', N'alex-dental'),
('33330000-0000-0000-0000-000000000003', '{EgyptCountryId}', '2026-02-03T10:00:00', '88880000-0000-0000-0000-000000000003', '11110000-0000-0000-0000-000000000003', N'GoogleSearch', N'[""Orthopedic Surgery"",""Sports Medicine"",""Neurology""]', N'Active', N'A physiotherapy and rehab centre in Giza.', N'billing@giza-physio.dev', 4.4,  61, N'Orthopaedic, neurological and sports rehab.',   N'support@giza-physio.dev', N'Giza Physio & Rehab',N'giza-physio'),
('33330000-0000-0000-0000-000000000004', '{EgyptCountryId}', '2026-02-04T10:00:00', '88880000-0000-0000-0000-000000000004', '11110000-0000-0000-0000-000000000004', N'Instagram',    N'[""Dermatology"",""Plastic & Reconstructive Surgery""]', N'Suspended', N'A dermatology and aesthetics clinic in Cairo.', N'billing@cairo-derma.dev', 4.1,  40, N'Skin care, laser and cosmetic procedures.',     N'support@cairo-derma.dev', N'Cairo Derma Care',  N'cairo-derma'),
('33330000-0000-0000-0000-000000000005', '{EgyptCountryId}', '2026-02-05T10:00:00', '88880000-0000-0000-0000-000000000005', '11110000-0000-0000-0000-000000000005', N'Friend',       N'[""Orthopedic Surgery"",""General Surgery""]', N'Suspended', N'An orthopaedic hospital in Luxor.',            N'billing@luxor-ortho.dev', 3.9,  22, N'Bone, joint and trauma surgery.',               N'support@luxor-ortho.dev', N'Luxor Ortho Hospital',N'luxor-ortho');

UPDATE [MediaFiles] SET [OrganizationId] = '33330000-0000-0000-0000-000000000001' WHERE [Id] IN ('88880000-0000-0000-0000-000000000001','88880000-0000-0000-0000-000000000015','88880000-0000-0000-0000-000000000020','88880000-0000-0000-0000-000000000021','88880000-0000-0000-0000-000000000022','88880000-0000-0000-0000-000000000025');
UPDATE [MediaFiles] SET [OrganizationId] = '33330000-0000-0000-0000-000000000002' WHERE [Id] IN ('88880000-0000-0000-0000-000000000002','88880000-0000-0000-0000-000000000016','88880000-0000-0000-0000-000000000023','88880000-0000-0000-0000-000000000026');
UPDATE [MediaFiles] SET [OrganizationId] = '33330000-0000-0000-0000-000000000003' WHERE [Id] IN ('88880000-0000-0000-0000-000000000003','88880000-0000-0000-0000-000000000017','88880000-0000-0000-0000-000000000024');
UPDATE [MediaFiles] SET [OrganizationId] = '33330000-0000-0000-0000-000000000004' WHERE [Id] IN ('88880000-0000-0000-0000-000000000004','88880000-0000-0000-0000-000000000018');
UPDATE [MediaFiles] SET [OrganizationId] = '33330000-0000-0000-0000-000000000005' WHERE [Id] IN ('88880000-0000-0000-0000-000000000005','88880000-0000-0000-0000-000000000019');

INSERT INTO [OrganizationApplications] ([Id], [CountryId], [ExpiryDateUtc], [OwnerId], [ProposedServicesType], [ReferralSource], [RejectedBy], [Status], [SubmittedOnUtc], [About], [ApplicationLetter], [BillingEmail], [ProposedName], [ServiceDescription], [SupportEmail]) VALUES
('44440000-0000-0000-0000-000000000001', '{EgyptCountryId}', '2027-01-10T09:00:00', '11110000-0000-0000-0000-000000000001', N'[""Cardiology"",""Dermatology""]', N'Website',      NULL, N'Accepted', '2026-01-05T09:00:00', N'Nile Care provides outpatient care.',        N'We are a healthcare provider seeking to onboard with Viora.', N'billing@nile-care.dev',   N'Nile Care Clinic',    N'General and specialized outpatient care.', N'support@nile-care.dev'),
('44440000-0000-0000-0000-000000000002', '{EgyptCountryId}', '2027-01-11T09:00:00', '11110000-0000-0000-0000-000000000002', N'[""Dentistry & Oral Health""]', N'SocialMedia',  NULL, N'Accepted', '2026-01-06T09:00:00', N'Alex Dental provides oral care.',            N'We are a dental clinic seeking to onboard with Viora.',       N'billing@alex-dental.dev', N'Alex Dental Smile',   N'Comprehensive oral health and orthodontics.', N'support@alex-dental.dev'),
('44440000-0000-0000-0000-000000000003', '{EgyptCountryId}', '2027-01-12T09:00:00', '11110000-0000-0000-0000-000000000003', N'[""Orthopedic Surgery"",""Sports Medicine"",""Neurology""]', N'GoogleSearch', NULL, N'Accepted', '2026-01-07T09:00:00', N'Giza Physio provides rehab.',     N'We are a rehab centre seeking to onboard with Viora.',        N'billing@giza-physio.dev', N'Giza Physio & Rehab', N'Orthopaedic, neurological and sports rehab.', N'support@giza-physio.dev'),
('44440000-0000-0000-0000-000000000004', '{EgyptCountryId}', '2027-01-13T09:00:00', '11110000-0000-0000-0000-000000000004', N'[""Dermatology"",""Plastic & Reconstructive Surgery""]', N'Instagram', NULL, N'Accepted', '2026-01-08T09:00:00', N'Cairo Derma provides skin care.', N'We are a dermatology clinic seeking to onboard with Viora.',  N'billing@cairo-derma.dev', N'Cairo Derma Care',    N'Skin care, laser and cosmetic procedures.',  N'support@cairo-derma.dev'),
('44440000-0000-0000-0000-000000000005', '{EgyptCountryId}', '2027-01-14T09:00:00', '11110000-0000-0000-0000-000000000005', N'[""Orthopedic Surgery"",""General Surgery""]', N'Friend', NULL, N'Accepted', '2026-01-09T09:00:00', N'Luxor Ortho provides surgery.',   N'We are an orthopaedic hospital seeking to onboard with Viora.',N'billing@luxor-ortho.dev', N'Luxor Ortho Hospital',N'Bone, joint and trauma surgery.',            N'support@luxor-ortho.dev'),
('44440000-0000-0000-0000-000000000006', '{EgyptCountryId}', '2027-01-15T09:00:00', '11110000-0000-0000-0000-000000000006', N'[""Dentistry & Oral Health""]', N'SocialMedia', NULL, N'Pending', '2026-01-12T09:00:00', N'Cairo Dental Center provides oral care.', N'We are a dental clinic seeking to onboard with Viora.', N'billing@cairo-dental.dev', N'Cairo Dental Center', N'Family and cosmetic dentistry.', N'support@cairo-dental.dev'),
('44440000-0000-0000-0000-000000000007', '{EgyptCountryId}', '2027-01-16T09:00:00', '11110000-0000-0000-0000-000000000007', N'[""Ophthalmology""]', N'Website', NULL, N'Pending', '2026-01-13T09:00:00', N'Sharm Eye Clinic provides eye care.', N'We are an ophthalmology clinic seeking to onboard with Viora.', N'billing@sharm-eye.dev', N'Sharm Eye Clinic', N'Cataract, retina and refractive surgery.', N'support@sharm-eye.dev');

INSERT INTO [LegalPapers] ([Id], [ApplicationId], [ApprovedById], [AttachmentId], [ExpiryDateUtc], [Status], [SubmissionDateUtc], [Type], [Name]) VALUES
('1e6a0000-0000-0000-0000-000000000001', '44440000-0000-0000-0000-000000000001', '11110000-0000-0000-0000-000000000001', '88880000-0000-0000-0000-000000000006', '2027-01-05T00:00:00', N'Accepted',    '2026-01-05T09:00:00', N'ArticleOfAssociation',    N'Articles of Association'),
('1e6a0000-0000-0000-0000-000000000002', '44440000-0000-0000-0000-000000000001', '11110000-0000-0000-0000-000000000001', '88880000-0000-0000-0000-000000000007', '2027-01-05T00:00:00', N'Accepted',    '2026-01-05T09:00:00', N'CommercialRegistration',  N'Commercial Registration'),
('1e6a0000-0000-0000-0000-000000000003', '44440000-0000-0000-0000-000000000002', NULL,                                   '88880000-0000-0000-0000-000000000008', '2027-01-06T00:00:00', N'Accepted',    '2026-01-06T09:00:00', N'CommercialRegistration',  N'Commercial Registration'),
('1e6a0000-0000-0000-0000-000000000004', '44440000-0000-0000-0000-000000000003', NULL,                                   '88880000-0000-0000-0000-000000000009', '2027-01-07T00:00:00', N'Accepted',    '2026-01-07T09:00:00', N'CommercialRegistration',  N'Commercial Registration'),
('1e6a0000-0000-0000-0000-000000000005', '44440000-0000-0000-0000-000000000006', NULL,                                   '88880000-0000-0000-0000-000000000010', '2027-01-15T00:00:00', N'UnderReview', '2026-01-12T09:00:00', N'TaxCard',                 N'Tax Card'),
('1e6a0000-0000-0000-0000-000000000006', '44440000-0000-0000-0000-000000000005', '11110000-0000-0000-0000-000000000005', '88880000-0000-0000-0000-000000000011', '2027-01-09T00:00:00', N'Accepted',    '2026-01-09T09:00:00', N'CommercialRegistration',  N'Commercial Registration');";

    // ---------------------------------------------------------------------------------------------------------
    // SUBSCRIPTIONS + SUBSCRIPTION ORDERS + INVOICES/ITEMS + ADDON ORDERS + SUBSCRIPTION ADDONS + FEATURE USAGE
    //   OrderStatus ids: Draft=1 Pending=2 Paid=3 Fullfiled=4 Failed=5.  LimitedFeatureAddon ids from AddonData.
    //   LimitedFeature ids: branches ..0001, services_per_branch ..0002, staff_members ..0003, storage ..0004,
    //   marketing_ai_posts ..0005.
    // ---------------------------------------------------------------------------------------------------------
    private string BuildBillingSql() => $@"
INSERT INTO [Subscriptions] ([Id], [OrganizationId], [PlanId], [Status], [SubscriptionsEndTime], [SubscriptionsStartTime]) VALUES
('99990000-0000-0000-0000-000000000001', '33330000-0000-0000-0000-000000000001', '{StarterPlanId}',    N'Active', '2026-08-01T00:00:00', '2026-07-01T00:00:00'),
('99990000-0000-0000-0000-000000000002', '33330000-0000-0000-0000-000000000002', '{ProPlanId}',        N'Active', '2027-01-01T00:00:00', '2026-07-01T00:00:00'),
('99990000-0000-0000-0000-000000000003', '33330000-0000-0000-0000-000000000003', '{StarterPlanId}',    N'Active', '2026-08-01T00:00:00', '2026-07-01T00:00:00'),
('99990000-0000-0000-0000-000000000004', '33330000-0000-0000-0000-000000000004', '{EnterprisePlanId}', N'Active', '2027-07-01T00:00:00', '2026-07-01T00:00:00'),
('99990000-0000-0000-0000-000000000005', '33330000-0000-0000-0000-000000000005', '{StarterPlanId}',    N'Active', '2026-08-01T00:00:00', '2026-07-01T00:00:00');

INSERT INTO [Invoices] ([Id], [BillTo], [CreatedAtUtc], [Currency], [DueDateUtc], [OrganizationId], [OrganizationName], [Sequence], [Status], [TaxPercentage], [ExternalPaymentId], [ExternalPaymentUrl]) VALUES
('a6000000-0000-0000-0000-000000000001', N'aya.hassan@viora.dev',   '2026-07-01T00:00:00', N'EGP', '2026-07-15T00:00:00', '33330000-0000-0000-0000-000000000001', N'Nile Care Clinic',    1001, N'Paid',   14.000000, N'PAY-1001', N'https://pay.dev/1001'),
('a6000000-0000-0000-0000-000000000002', N'sara.mostafa@viora.dev', '2026-07-01T00:00:00', N'EGP', '2026-07-15T00:00:00', '33330000-0000-0000-0000-000000000002', N'Alex Dental Smile',   1002, N'Paid',   14.000000, N'PAY-1002', N'https://pay.dev/1002'),
('a6000000-0000-0000-0000-000000000003', N'karim.saber@viora.dev',  '2026-07-01T00:00:00', N'EGP', '2026-07-15T00:00:00', '33330000-0000-0000-0000-000000000003', N'Giza Physio & Rehab', 1003, N'Issued', 14.000000, N'PAY-1003', N'https://pay.dev/1003'),
('a6000000-0000-0000-0000-000000000004', N'mona.adel@viora.dev',    '2026-07-01T00:00:00', N'EGP', '2026-07-15T00:00:00', '33330000-0000-0000-0000-000000000004', N'Cairo Derma Care',    1004, N'Paid',   14.000000, N'PAY-1004', N'https://pay.dev/1004'),
('a6000000-0000-0000-0000-000000000005', N'hassan.tarek@viora.dev', '2026-07-01T00:00:00', N'EGP', '2026-07-15T00:00:00', '33330000-0000-0000-0000-000000000005', N'Luxor Ortho Hospital',1005, N'Overdue',14.000000, N'PAY-1005', N'https://pay.dev/1005');

INSERT INTO [InvoiceItems] ([InvoiceId], [Description], [DiscountPercentage], [ItemName], [Quantity], [TaxPercentage], [ItemNumber], [PriceAmount], [PriceCurrency]) VALUES
('a6000000-0000-0000-0000-000000000001', N'Starter plan monthly subscription',       0.000000, N'Starter Plan',      1, 14.000000, 1, 99.90,  N'EGP'),
('a6000000-0000-0000-0000-000000000001', N'Extra branches add-on',                   0.000000, N'Branches Add-on',   1, 14.000000, 2, 22.90,  N'EGP'),
('a6000000-0000-0000-0000-000000000002', N'Professional plan semi-annual',           0.000000, N'Professional Plan', 1, 14.000000, 1, 199.90, N'EGP'),
('a6000000-0000-0000-0000-000000000002', N'Extra staff seats add-on',                0.000000, N'Staff Add-on',      1, 14.000000, 2, 54.90,  N'EGP'),
('a6000000-0000-0000-0000-000000000003', N'Starter plan monthly subscription',       0.000000, N'Starter Plan',      1, 14.000000, 1, 99.90,  N'EGP'),
('a6000000-0000-0000-0000-000000000004', N'Enterprise plan annual subscription',     10.000000,N'Enterprise Plan',   1, 14.000000, 1, 399.90, N'EGP'),
('a6000000-0000-0000-0000-000000000005', N'Starter plan monthly subscription',       0.000000, N'Starter Plan',      1, 14.000000, 1, 99.90,  N'EGP');

INSERT INTO [SubscriptionOrder] ([Id], [CreatedDate], [InvoiceId], [OrganizationId], [PlanId], [Status], [SubscriptionId], [SubscriptionOrderType], [TotalPriceAmount], [TotalPriceCurrency]) VALUES
('50000000-0000-0000-0000-000000000001', '2026-07-01T00:00:00', 'a6000000-0000-0000-0000-000000000001', '33330000-0000-0000-0000-000000000001', '{StarterPlanId}',    3, '99990000-0000-0000-0000-000000000001', N'NewSubscription', 99.90,  N'EGP'),
('50000000-0000-0000-0000-000000000002', '2026-07-01T00:00:00', 'a6000000-0000-0000-0000-000000000002', '33330000-0000-0000-0000-000000000002', '{ProPlanId}',        3, '99990000-0000-0000-0000-000000000002', N'NewSubscription', 199.90, N'EGP'),
('50000000-0000-0000-0000-000000000003', '2026-07-01T00:00:00', 'a6000000-0000-0000-0000-000000000003', '33330000-0000-0000-0000-000000000003', '{StarterPlanId}',    2, '99990000-0000-0000-0000-000000000003', N'NewSubscription', 99.90,  N'EGP'),
('50000000-0000-0000-0000-000000000004', '2026-07-01T00:00:00', 'a6000000-0000-0000-0000-000000000004', '33330000-0000-0000-0000-000000000004', '{EnterprisePlanId}', 3, '99990000-0000-0000-0000-000000000004', N'NewSubscription', 399.90, N'EGP'),
('50000000-0000-0000-0000-000000000005', '2026-07-01T00:00:00', 'a6000000-0000-0000-0000-000000000005', '33330000-0000-0000-0000-000000000005', '{StarterPlanId}',    5, '99990000-0000-0000-0000-000000000005', N'NewSubscription', 99.90,  N'EGP');

INSERT INTO [AddonOrders] ([Id], [CreatedDate], [InvoiceId], [OrganizationId], [Status], [SubscriptionId], [TotalPriceAmount], [TotalPriceCurrency]) VALUES
('b6000000-0000-0000-0000-000000000001', '2026-07-02T00:00:00', 'a6000000-0000-0000-0000-000000000001', '33330000-0000-0000-0000-000000000001', 3, '99990000-0000-0000-0000-000000000001', 22.90,  N'EGP'),
('b6000000-0000-0000-0000-000000000002', '2026-07-02T00:00:00', 'a6000000-0000-0000-0000-000000000002', '33330000-0000-0000-0000-000000000002', 3, '99990000-0000-0000-0000-000000000002', 54.90,  N'EGP'),
('b6000000-0000-0000-0000-000000000003', '2026-07-03T00:00:00', NULL,                                   '33330000-0000-0000-0000-000000000003', 2, '99990000-0000-0000-0000-000000000003', 99.90,  N'EGP'),
('b6000000-0000-0000-0000-000000000004', '2026-07-03T00:00:00', NULL,                                   '33330000-0000-0000-0000-000000000004', 1, '99990000-0000-0000-0000-000000000004', 149.90, N'EGP'),
('b6000000-0000-0000-0000-000000000005', '2026-07-04T00:00:00', NULL,                                   '33330000-0000-0000-0000-000000000005', 5, '99990000-0000-0000-0000-000000000005', 22.90,  N'EGP');

INSERT INTO [AddonOrderLimitedFeatures] ([AddonOrderId], [LimitedFeatureId], [Id]) VALUES
('b6000000-0000-0000-0000-000000000001', 'f1a2b3c4-0001-0000-0000-000000000001', 'b6a10000-0000-0000-0000-000000000001'),
('b6000000-0000-0000-0000-000000000002', 'f1a2b3c4-0003-0000-0000-000000000003', 'b6a10000-0000-0000-0000-000000000002'),
('b6000000-0000-0000-0000-000000000003', 'f1a2b3c4-0002-0000-0000-000000000002', 'b6a10000-0000-0000-0000-000000000003'),
('b6000000-0000-0000-0000-000000000004', 'f1a2b3c4-0004-0000-0000-000000000004', 'b6a10000-0000-0000-0000-000000000004'),
('b6000000-0000-0000-0000-000000000005', 'f1a2b3c4-0005-0000-0000-000000000005', 'b6a10000-0000-0000-0000-000000000005');

INSERT INTO [SubscriptionAddon] ([Id], [IsActive], [LimitedFeatureAddonId], [SubscriptionId]) VALUES
('5add0000-0000-0000-0000-000000000001', 1, '11111111-1111-1111-1111-111111111111', '99990000-0000-0000-0000-000000000001'),
('5add0000-0000-0000-0000-000000000002', 1, '33333333-3333-3333-3333-333333333333', '99990000-0000-0000-0000-000000000002'),
('5add0000-0000-0000-0000-000000000003', 1, '22222222-2222-2222-2222-222222222222', '99990000-0000-0000-0000-000000000003'),
('5add0000-0000-0000-0000-000000000004', 1, '44444444-4444-4444-4444-444444444444', '99990000-0000-0000-0000-000000000004'),
('5add0000-0000-0000-0000-000000000005', 0, '11111111-1111-1111-1111-111111111111', '99990000-0000-0000-0000-000000000005');

INSERT INTO [FeatureUsages] ([Id], [LimitedFeatureId], [OrganizationId], [PeriodEnd], [PeriodStart], [Quota]) VALUES
('fea50000-0000-0000-0000-000000000001', 'f1a2b3c4-0001-0000-0000-000000000001', '33330000-0000-0000-0000-000000000001', '2026-08-01T00:00:00', '2026-07-01T00:00:00', 3),
('fea50000-0000-0000-0000-000000000002', 'f1a2b3c4-0003-0000-0000-000000000003', '33330000-0000-0000-0000-000000000001', '2026-08-01T00:00:00', '2026-07-01T00:00:00', 18),
('fea50000-0000-0000-0000-000000000003', 'f1a2b3c4-0005-0000-0000-000000000005', '33330000-0000-0000-0000-000000000001', '2026-08-01T00:00:00', '2026-07-01T00:00:00', 27),
('fea50000-0000-0000-0000-000000000004', 'f1a2b3c4-0001-0000-0000-000000000001', '33330000-0000-0000-0000-000000000002', '2027-01-01T00:00:00', '2026-07-01T00:00:00', 8),
('fea50000-0000-0000-0000-000000000005', 'f1a2b3c4-0003-0000-0000-000000000003', '33330000-0000-0000-0000-000000000002', '2027-01-01T00:00:00', '2026-07-01T00:00:00', 45),
('fea50000-0000-0000-0000-000000000006', 'f1a2b3c4-0005-0000-0000-000000000005', '33330000-0000-0000-0000-000000000002', '2027-01-01T00:00:00', '2026-07-01T00:00:00', 90),
('fea50000-0000-0000-0000-000000000007', 'f1a2b3c4-0001-0000-0000-000000000001', '33330000-0000-0000-0000-000000000003', '2026-08-01T00:00:00', '2026-07-01T00:00:00', 4),
('fea50000-0000-0000-0000-000000000008', 'f1a2b3c4-0005-0000-0000-000000000005', '33330000-0000-0000-0000-000000000003', '2026-08-01T00:00:00', '2026-07-01T00:00:00', 30),
('fea50000-0000-0000-0000-000000000009', 'f1a2b3c4-0001-0000-0000-000000000001', '33330000-0000-0000-0000-000000000004', '2027-07-01T00:00:00', '2026-07-01T00:00:00', 12),
('fea50000-0000-0000-0000-000000000010', 'f1a2b3c4-0005-0000-0000-000000000005', '33330000-0000-0000-0000-000000000005', '2026-08-01T00:00:00', '2026-07-01T00:00:00', 25);";

    // ---------------------------------------------------------------------------------------------------------
    // BRANCHES (+ phone numbers, business hours, gallery). Location = geography::Point(lat, lon, 4326).
    //   TimeZone value object -> unnamed column [Value].  Services -> JSON array (column [Services]).
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildBranchesSql() => $@"
INSERT INTO [Branch] ([Id], [Location], [OpenedAtUtc], [OrganizationId], [Services], [Status], [AddressCity], [AddressCountryId], [AddressNumber], [AddressPostalCode], [AddressState], [AddressStreet], [ContactEmail], [TimeZone_Value]) VALUES
('55550000-0000-0000-0000-000000000001', geography::Point(30.0444, 31.2357, 4326), '2026-02-10T08:00:00', '33330000-0000-0000-0000-000000000001', N'[""Cardiology""]',  N'Active', N'Cairo',      '{EgyptCountryId}', 10, 11511, N'Cairo',      N'Tahrir Street',           N'downtown@nile-care.dev',   N'Africa/Cairo'),
('55550000-0000-0000-0000-000000000002', geography::Point(30.0131, 31.2089, 4326), '2026-02-11T08:00:00', '33330000-0000-0000-0000-000000000001', N'[""Dermatology""]', N'Active', N'Giza',       '{EgyptCountryId}', 22, 12511, N'Giza',       N'Pyramids Road',           N'giza@nile-care.dev',       N'Africa/Cairo'),
('55550000-0000-0000-0000-000000000003', geography::Point(31.2001, 29.9187, 4326), '2026-02-12T08:00:00', '33330000-0000-0000-0000-000000000002', N'[""Dentistry & Oral Health""]', N'Active', N'Alexandria', '{EgyptCountryId}', 5, 21500, N'Alexandria', N'El-Horreya Road',         N'downtown@alex-dental.dev', N'Africa/Cairo'),
('55550000-0000-0000-0000-000000000004', geography::Point(30.0600, 31.2000, 4326), '2026-02-13T08:00:00', '33330000-0000-0000-0000-000000000003', N'[""Orthopedic Surgery"",""Sports Medicine""]', N'Active', N'Giza', '{EgyptCountryId}', 20, 12311, N'Giza', N'El-Sudan Street', N'mohandessin@giza-physio.dev', N'Africa/Cairo'),
('55550000-0000-0000-0000-000000000005', geography::Point(29.9600, 31.2500, 4326), '2026-02-14T08:00:00', '33330000-0000-0000-0000-000000000004', N'[""Dermatology""]', N'Hidden', N'Cairo',      '{EgyptCountryId}', 8,  11728, N'Cairo',      N'Road 9, Maadi',           N'maadi@cairo-derma.dev',    N'Africa/Cairo'),
('55550000-0000-0000-0000-000000000006', geography::Point(25.6872, 32.6396, 4326), '2026-02-15T08:00:00', '33330000-0000-0000-0000-000000000005', N'[""Orthopedic Surgery""]', N'Active', N'Luxor',   '{EgyptCountryId}', 3,  85951, N'Luxor',      N'Corniche El-Nil',         N'main@luxor-ortho.dev',     N'Africa/Cairo'),
('55550000-0000-0000-0000-000000000007', geography::Point(31.2200, 29.9400, 4326), '2026-02-16T08:00:00', '33330000-0000-0000-0000-000000000002', N'[""Dentistry & Oral Health""]', N'Active', N'Alexandria', '{EgyptCountryId}', 12, 21600, N'Alexandria', N'Victor Emmanuel Square', N'sidigaber@alex-dental.dev', N'Africa/Cairo');

INSERT INTO [BranchPhoneNumber] ([BranchId], [PhoneNumber]) VALUES
('55550000-0000-0000-0000-000000000001', N'+20233334441'),
('55550000-0000-0000-0000-000000000001', N'+20233334442'),
('55550000-0000-0000-0000-000000000002', N'+20233334443'),
('55550000-0000-0000-0000-000000000003', N'+20334445551'),
('55550000-0000-0000-0000-000000000004', N'+20233336661'),
('55550000-0000-0000-0000-000000000005', N'+20233337771'),
('55550000-0000-0000-0000-000000000006', N'+20952228881'),
('55550000-0000-0000-0000-000000000007', N'+20334449991');

INSERT INTO [BranchBusinessHour] ([BranchId], [Day], [CloseTime], [OpenTime]) VALUES
('55550000-0000-0000-0000-000000000001', 1, '17:00:00', '09:00:00'),
('55550000-0000-0000-0000-000000000001', 2, '17:00:00', '09:00:00'),
('55550000-0000-0000-0000-000000000001', 3, '17:00:00', '09:00:00'),
('55550000-0000-0000-0000-000000000003', 1, '18:00:00', '10:00:00'),
('55550000-0000-0000-0000-000000000003', 2, '18:00:00', '10:00:00'),
('55550000-0000-0000-0000-000000000004', 3, '18:00:00', '10:00:00'),
('55550000-0000-0000-0000-000000000006', 0, '19:00:00', '11:00:00');

INSERT INTO [BranchGallery] ([BranchId], [MediaFileId]) VALUES
('55550000-0000-0000-0000-000000000001', '88880000-0000-0000-0000-000000000022'),
('55550000-0000-0000-0000-000000000003', '88880000-0000-0000-0000-000000000023'),
('55550000-0000-0000-0000-000000000004', '88880000-0000-0000-0000-000000000024'),
('55550000-0000-0000-0000-000000000002', '88880000-0000-0000-0000-000000000025'),
('55550000-0000-0000-0000-000000000007', '88880000-0000-0000-0000-000000000026');";

    // ---------------------------------------------------------------------------------------------------------
    // SERVICES (+ gallery). Duration = time. Type = ServiceType value string. Discount optional (mostly NULL).
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildServicesSql() => @"
INSERT INTO [Service] ([Id], [BranchId], [Duration], [Status], [Type], [CostAmount], [CostCurrency], [Description], [Name], [DiscountEndDateUtc], [DiscountPercentage], [DiscountReason], [DiscountStartDateUtc]) VALUES
('66660000-0000-0000-0000-000000000001', '55550000-0000-0000-0000-000000000001', '00:30:00', N'Active', N'Cardiology',   200.00, N'EGP', N'Full cardiology consultation and review.', N'Cardiology Consultation', NULL, NULL, NULL, NULL),
('66660000-0000-0000-0000-000000000002', '55550000-0000-0000-0000-000000000001', '00:20:00', N'Active', N'Cardiology',   150.00, N'EGP', N'Resting electrocardiogram.',               N'ECG Test',               '2026-08-31T00:00:00', 15, N'Summer promo', '2026-07-01T00:00:00'),
('66660000-0000-0000-0000-000000000003', '55550000-0000-0000-0000-000000000002', '00:30:00', N'Active', N'Dermatology',  180.00, N'EGP', N'Skin screening and mole check.',           N'Skin Screening',          NULL, NULL, NULL, NULL),
('66660000-0000-0000-0000-000000000004', '55550000-0000-0000-0000-000000000003', '00:40:00', N'Active', N'Dentistry & Oral Health', 250.00, N'EGP', N'Professional scaling and polishing.', N'Dental Cleaning', NULL, NULL, NULL, NULL),
('66660000-0000-0000-0000-000000000005', '55550000-0000-0000-0000-000000000003', '01:00:00', N'Active', N'Dentistry & Oral Health', 350.00, N'EGP', N'Composite resin restoration.',        N'Tooth Filling',   NULL, NULL, NULL, NULL),
('66660000-0000-0000-0000-000000000006', '55550000-0000-0000-0000-000000000004', '01:00:00', N'Active', N'Orthopedic Surgery', 300.00, N'EGP', N'Manual therapy and exercise programme.', N'Physiotherapy Session', NULL, NULL, NULL, NULL),
('66660000-0000-0000-0000-000000000007', '55550000-0000-0000-0000-000000000004', '00:45:00', N'Active', N'Sports Medicine',    350.00, N'EGP', N'Biomechanical screening and plan.',      N'Sports Injury Assessment', NULL, NULL, NULL, NULL),
('66660000-0000-0000-0000-000000000008', '55550000-0000-0000-0000-000000000005', '00:30:00', N'Disabled', N'Dermatology', 220.00, N'EGP', N'Acne assessment and treatment plan.',     N'Acne Treatment',         NULL, NULL, NULL, NULL),
('66660000-0000-0000-0000-000000000009', '55550000-0000-0000-0000-000000000005', '00:45:00', N'Active', N'Plastic & Reconstructive Surgery', 900.00, N'EGP', N'Botulinum toxin cosmetic session.', N'Botox Session', NULL, NULL, NULL, NULL),
('66660000-0000-0000-0000-000000000010', '55550000-0000-0000-0000-000000000006', '00:30:00', N'Active', N'Orthopedic Surgery', 280.00, N'EGP', N'Fracture review and re-casting.',        N'Fracture Follow-up',     NULL, NULL, NULL, NULL),
('66660000-0000-0000-0000-000000000011', '55550000-0000-0000-0000-000000000006', '01:30:00', N'Active', N'Orthopedic Surgery', 500.00, N'EGP', N'Structured post-operative rehab.',       N'Post-Op Rehab',          NULL, NULL, NULL, NULL),
('66660000-0000-0000-0000-000000000012', '55550000-0000-0000-0000-000000000007', '00:30:00', N'Active', N'Dentistry & Oral Health', 200.00, N'EGP', N'Braces or aligner assessment.',      N'Orthodontic Consultation', NULL, NULL, NULL, NULL);

INSERT INTO [ServiceGallery] ([MediaFileId], [ServiceId]) VALUES
('88880000-0000-0000-0000-000000000025', '66660000-0000-0000-0000-000000000001'),
('88880000-0000-0000-0000-000000000025', '66660000-0000-0000-0000-000000000002'),
('88880000-0000-0000-0000-000000000026', '66660000-0000-0000-0000-000000000004'),
('88880000-0000-0000-0000-000000000026', '66660000-0000-0000-0000-000000000005'),
('88880000-0000-0000-0000-000000000022', '66660000-0000-0000-0000-000000000006');";

    // ---------------------------------------------------------------------------------------------------------
    // STAFF (+ branch/service/role joins, invitation tokens). Staff can log in with Username + Dev123!Pass.
    //   StaffRole references seeded Role ids (Registered=1, Admin=3). Gender/StaffStatus stored as strings.
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildStaffSql(string passwordHash) => $@"
INSERT INTO [Staff] ([Id], [CreatedAt], [DateOfBirth], [DeletedAt], [FirstName], [Gender], [HashedPassword], [LastName], [OrganizationId], [PhoneNumber], [StaffStatus], [Username]) VALUES
('77770000-0000-0000-0000-000000000001', '2026-02-20T08:00:00', '1982-05-04', NULL, N'Amr',    N'Male',   N'{passwordHash}', N'Zaki',   '33330000-0000-0000-0000-000000000001', N'+201111111101', N'Active',    N'dr.amr'),
('77770000-0000-0000-0000-000000000002', '2026-02-20T08:00:00', '1990-09-14', NULL, N'Hana',   N'Female', N'{passwordHash}', N'Lotfy',  '33330000-0000-0000-0000-000000000001', N'+201111111102', N'Active',    N'dr.hana'),
('77770000-0000-0000-0000-000000000003', '2026-02-21T08:00:00', '1986-01-22', NULL, N'Nour',   N'Female', N'{passwordHash}', N'Adel',   '33330000-0000-0000-0000-000000000002', N'+201111111103', N'Active',    N'dr.nour'),
('77770000-0000-0000-0000-000000000004', '2026-02-22T08:00:00', '1984-07-30', NULL, N'Sameh',  N'Male',   N'{passwordHash}', N'Ezz',    '33330000-0000-0000-0000-000000000003', N'+201111111104', N'Active',    N'dr.sameh'),
('77770000-0000-0000-0000-000000000005', '2026-02-23T08:00:00', '1991-03-11', NULL, N'Rania',  N'Female', N'{passwordHash}', N'Fahmy',  '33330000-0000-0000-0000-000000000004', N'+201111111105', N'Suspended', N'dr.rania'),
('77770000-0000-0000-0000-000000000006', '2026-02-24T08:00:00', '1988-12-19', NULL, N'Bishoy', N'Male',   N'{passwordHash}', N'Nabil',  '33330000-0000-0000-0000-000000000005', N'+201111111106', N'Active',    N'dr.bishoy'),
('77770000-0000-0000-0000-000000000007', '2026-02-25T08:00:00', '1993-06-06', NULL, N'Yara',   N'Female', N'{passwordHash}', N'Kamal',  '33330000-0000-0000-0000-000000000002', N'+201111111107', N'Pending',   N'dr.yara');

INSERT INTO [StaffBranch] ([BranchId], [StaffId]) VALUES
('55550000-0000-0000-0000-000000000001', '77770000-0000-0000-0000-000000000001'),
('55550000-0000-0000-0000-000000000002', '77770000-0000-0000-0000-000000000002'),
('55550000-0000-0000-0000-000000000003', '77770000-0000-0000-0000-000000000003'),
('55550000-0000-0000-0000-000000000004', '77770000-0000-0000-0000-000000000004'),
('55550000-0000-0000-0000-000000000005', '77770000-0000-0000-0000-000000000005'),
('55550000-0000-0000-0000-000000000006', '77770000-0000-0000-0000-000000000006'),
('55550000-0000-0000-0000-000000000007', '77770000-0000-0000-0000-000000000007');

INSERT INTO [StaffService] ([ServiceId], [StaffId]) VALUES
('66660000-0000-0000-0000-000000000001', '77770000-0000-0000-0000-000000000001'),
('66660000-0000-0000-0000-000000000002', '77770000-0000-0000-0000-000000000001'),
('66660000-0000-0000-0000-000000000003', '77770000-0000-0000-0000-000000000002'),
('66660000-0000-0000-0000-000000000004', '77770000-0000-0000-0000-000000000003'),
('66660000-0000-0000-0000-000000000005', '77770000-0000-0000-0000-000000000003'),
('66660000-0000-0000-0000-000000000006', '77770000-0000-0000-0000-000000000004'),
('66660000-0000-0000-0000-000000000007', '77770000-0000-0000-0000-000000000004'),
('66660000-0000-0000-0000-000000000008', '77770000-0000-0000-0000-000000000005'),
('66660000-0000-0000-0000-000000000010', '77770000-0000-0000-0000-000000000006'),
('66660000-0000-0000-0000-000000000012', '77770000-0000-0000-0000-000000000007');

INSERT INTO [StaffRole] ([RoleId], [StaffId]) VALUES
(3, '77770000-0000-0000-0000-000000000001'),
(1, '77770000-0000-0000-0000-000000000002'),
(3, '77770000-0000-0000-0000-000000000003'),
(1, '77770000-0000-0000-0000-000000000004'),
(3, '77770000-0000-0000-0000-000000000005'),
(1, '77770000-0000-0000-0000-000000000006'),
(3, '77770000-0000-0000-0000-000000000007');

INSERT INTO [StaffInvitationTokens] ([Id], [CreatedAt], [Expiration], [RevokedAt], [StaffId], [TokenHash], [UsedAt]) VALUES
('57a70000-0000-0000-0000-000000000001', '2026-02-20T08:00:00', '2026-02-27T08:00:00', NULL, '77770000-0000-0000-0000-000000000001', N'dev-staff-token-hash-01', '2026-02-21T10:00:00'),
('57a70000-0000-0000-0000-000000000002', '2026-02-20T08:00:00', '2026-02-27T08:00:00', NULL, '77770000-0000-0000-0000-000000000002', N'dev-staff-token-hash-02', '2026-02-21T10:00:00'),
('57a70000-0000-0000-0000-000000000003', '2026-02-21T08:00:00', '2026-02-28T08:00:00', NULL, '77770000-0000-0000-0000-000000000003', N'dev-staff-token-hash-03', '2026-02-22T10:00:00'),
('57a70000-0000-0000-0000-000000000004', '2026-02-22T08:00:00', '2026-03-01T08:00:00', NULL, '77770000-0000-0000-0000-000000000004', N'dev-staff-token-hash-04', NULL),
('57a70000-0000-0000-0000-000000000005', '2026-02-23T08:00:00', '2026-03-02T08:00:00', '2026-02-24T09:00:00', '77770000-0000-0000-0000-000000000005', N'dev-staff-token-hash-05', NULL);";

    // ---------------------------------------------------------------------------------------------------------
    // SCHEDULES + SHIFTS + SCHEDULE CANCELLATIONS. Times are 'HH:mm:ss'; DayOfWeek 0=Sunday..6=Saturday.
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildSchedulingSql() => @"
INSERT INTO [Schedules] ([Id], [BranchId], [DayOfWeek]) VALUES
('5c000000-0000-0000-0000-000000000001', '55550000-0000-0000-0000-000000000001', 1),
('5c000000-0000-0000-0000-000000000002', '55550000-0000-0000-0000-000000000001', 2),
('5c000000-0000-0000-0000-000000000003', '55550000-0000-0000-0000-000000000003', 1),
('5c000000-0000-0000-0000-000000000004', '55550000-0000-0000-0000-000000000004', 3),
('5c000000-0000-0000-0000-000000000005', '55550000-0000-0000-0000-000000000005', 4),
('5c000000-0000-0000-0000-000000000006', '55550000-0000-0000-0000-000000000006', 0),
('5c000000-0000-0000-0000-000000000007', '55550000-0000-0000-0000-000000000002', 1);

INSERT INTO [Shifts] ([Id], [EndTime], [ScheduleId], [StaffId], [StartTime]) VALUES
('5f000000-0000-0000-0000-000000000001', '17:00:00', '5c000000-0000-0000-0000-000000000001', '77770000-0000-0000-0000-000000000001', '09:00:00'),
('5f000000-0000-0000-0000-000000000002', '17:00:00', '5c000000-0000-0000-0000-000000000002', '77770000-0000-0000-0000-000000000001', '09:00:00'),
('5f000000-0000-0000-0000-000000000003', '17:00:00', '5c000000-0000-0000-0000-000000000003', '77770000-0000-0000-0000-000000000003', '09:00:00'),
('5f000000-0000-0000-0000-000000000004', '18:00:00', '5c000000-0000-0000-0000-000000000004', '77770000-0000-0000-0000-000000000004', '10:00:00'),
('5f000000-0000-0000-0000-000000000005', '15:00:00', '5c000000-0000-0000-0000-000000000005', '77770000-0000-0000-0000-000000000005', '09:00:00'),
('5f000000-0000-0000-0000-000000000006', '19:00:00', '5c000000-0000-0000-0000-000000000006', '77770000-0000-0000-0000-000000000006', '11:00:00'),
('5f000000-0000-0000-0000-000000000007', '17:00:00', '5c000000-0000-0000-0000-000000000007', '77770000-0000-0000-0000-000000000002', '09:00:00');

INSERT INTO [ScheduleCancellations] ([Id], [CancellationDate], [Reason], [ShiftId]) VALUES
('5cc00000-0000-0000-0000-000000000001', '2026-07-06T00:00:00', N'Doctor on emergency leave.',       '5f000000-0000-0000-0000-000000000001'),
('5cc00000-0000-0000-0000-000000000002', '2026-07-07T00:00:00', N'Public holiday.',                  '5f000000-0000-0000-0000-000000000002'),
('5cc00000-0000-0000-0000-000000000003', '2026-07-08T00:00:00', N'Equipment maintenance.',           '5f000000-0000-0000-0000-000000000003'),
('5cc00000-0000-0000-0000-000000000004', '2026-07-09T00:00:00', N'Staff training session.',          '5f000000-0000-0000-0000-000000000004'),
('5cc00000-0000-0000-0000-000000000005', '2026-07-10T00:00:00', N'Branch temporarily closed.',       '5f000000-0000-0000-0000-000000000005');";

    // ---------------------------------------------------------------------------------------------------------
    // APPOINTMENTS (+ delays, reminders). Enum ints: CreatedBy(None=0,Customer=1,Staff=2),
    //   PayMethod(Cash=0,Wallet=1,Online=2), RequestPlatform(None=0,Web=1,Mobile=2). Status stored as string.
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildAppointmentsSql() => @"
INSERT INTO [Appointments] ([Id], [AppointmentQueueNumber], [BranchId], [CreatedAt], [CreatedBy], [CustomerId], [EstimatedDurationMinutes], [IsCheckedIn], [LastUpdatedAt], [PayMethod], [PaymentId], [RequestPlatform], [ReservationDate], [ServiceId], [StaffId], [Status]) VALUES
('a1110000-0000-0000-0000-000000000001', 1, '55550000-0000-0000-0000-000000000001', '2026-06-27T09:00:00', 1, '22220000-0000-0000-0000-000000000001', 30, 1, '2026-06-28T10:30:00', 0, NULL, 1, '2026-06-28T10:00:00', '66660000-0000-0000-0000-000000000001', '77770000-0000-0000-0000-000000000001', N'Completed'),
('a1110000-0000-0000-0000-000000000002', 1, '55550000-0000-0000-0000-000000000003', '2026-06-28T09:00:00', 1, '22220000-0000-0000-0000-000000000002', 40, 1, '2026-06-29T11:40:00', 2, NULL, 1, '2026-06-29T11:00:00', '66660000-0000-0000-0000-000000000004', '77770000-0000-0000-0000-000000000003', N'Completed'),
('a1110000-0000-0000-0000-000000000003', 1, '55550000-0000-0000-0000-000000000004', '2026-06-29T09:00:00', 2, '22220000-0000-0000-0000-000000000003', 60, 1, '2026-06-30T10:00:00', 1, NULL, 2, '2026-06-30T09:00:00', '66660000-0000-0000-0000-000000000006', '77770000-0000-0000-0000-000000000004', N'Completed'),
('a1110000-0000-0000-0000-000000000004', 1, '55550000-0000-0000-0000-000000000005', '2026-06-30T09:00:00', 1, '22220000-0000-0000-0000-000000000004', 30, 1, '2026-07-01T14:30:00', 0, NULL, 1, '2026-07-01T14:00:00', '66660000-0000-0000-0000-000000000008', '77770000-0000-0000-0000-000000000005', N'Completed'),
('a1110000-0000-0000-0000-000000000005', 1, '55550000-0000-0000-0000-000000000006', '2026-07-01T09:00:00', 1, '22220000-0000-0000-0000-000000000005', 30, 1, '2026-07-02T10:30:00', 2, NULL, 2, '2026-07-02T10:00:00', '66660000-0000-0000-0000-000000000010', '77770000-0000-0000-0000-000000000006', N'Completed'),
('a1110000-0000-0000-0000-000000000006', 2, '55550000-0000-0000-0000-000000000001', '2026-07-05T09:00:00', 1, '22220000-0000-0000-0000-000000000001', 20, 1, NULL, 1, NULL, 1, '2026-07-06T12:00:00', '66660000-0000-0000-0000-000000000002', '77770000-0000-0000-0000-000000000001', N'InProgress'),
('a1110000-0000-0000-0000-000000000007', 2, '55550000-0000-0000-0000-000000000003', '2026-07-06T09:00:00', 1, '22220000-0000-0000-0000-000000000002', 60, 0, NULL, 0, NULL, 2, '2026-07-08T11:00:00', '66660000-0000-0000-0000-000000000005', '77770000-0000-0000-0000-000000000003', N'Waiting'),
('a1110000-0000-0000-0000-000000000008', 1, '55550000-0000-0000-0000-000000000007', '2026-07-07T09:00:00', 1, '22220000-0000-0000-0000-000000000006', 30, 0, NULL, 2, NULL, 1, '2026-07-10T13:00:00', '66660000-0000-0000-0000-000000000012', '77770000-0000-0000-0000-000000000007', N'NotArrived');

INSERT INTO [ScheduleDelays] ([Id], [AppointmentId], [DelayDuration], [Initiator], [OccurrenceTime], [Reason]) VALUES
('5de00000-0000-0000-0000-000000000001', 'a1110000-0000-0000-0000-000000000001', '00:15:00', N'Staff',    '2026-06-28T10:00:00', N'Previous consultation ran long.'),
('5de00000-0000-0000-0000-000000000002', 'a1110000-0000-0000-0000-000000000002', '00:10:00', N'Customer', '2026-06-29T11:00:00', N'Patient arrived late.'),
('5de00000-0000-0000-0000-000000000003', 'a1110000-0000-0000-0000-000000000003', '00:20:00', N'Staff',    '2026-06-30T09:00:00', N'Equipment setup delay.'),
('5de00000-0000-0000-0000-000000000004', 'a1110000-0000-0000-0000-000000000006', '00:05:00', N'Staff',    '2026-07-06T12:00:00', N'Room changeover.'),
('5de00000-0000-0000-0000-000000000005', 'a1110000-0000-0000-0000-000000000007', '00:25:00', N'Customer', '2026-07-08T11:00:00', N'Paperwork not completed.');

INSERT INTO [Reminders] ([Id], [AppointmentId], [Body], [CreatedAt], [ScheduledFor], [Title]) VALUES
('4e110000-0000-0000-0000-000000000001', 'a1110000-0000-0000-0000-000000000004', N'Your appointment is tomorrow at 2 PM.',  '2026-06-30T09:00:00', '2026-07-01T08:00:00', N'Appointment Reminder'),
('4e110000-0000-0000-0000-000000000002', 'a1110000-0000-0000-0000-000000000005', N'Your appointment is tomorrow at 10 AM.', '2026-07-01T09:00:00', '2026-07-02T08:00:00', N'Appointment Reminder'),
('4e110000-0000-0000-0000-000000000003', 'a1110000-0000-0000-0000-000000000006', N'Your appointment is today at 12 PM.',    '2026-07-05T09:00:00', '2026-07-06T08:00:00', N'Appointment Reminder'),
('4e110000-0000-0000-0000-000000000004', 'a1110000-0000-0000-0000-000000000007', N'Your appointment is on the 8th at 11 AM.','2026-07-06T09:00:00', '2026-07-08T08:00:00', N'Appointment Reminder'),
('4e110000-0000-0000-0000-000000000005', 'a1110000-0000-0000-0000-000000000008', N'Your appointment is on the 10th at 1 PM.','2026-07-07T09:00:00', '2026-07-10T08:00:00', N'Appointment Reminder');";

    // ---------------------------------------------------------------------------------------------------------
    // FORMS + FORM SUBMISSIONS (+ medias) + PRESCRIPTIONS (+ items) + PRESCRIPTION TEMPLATES.
    //   Forms.ServiceId, FormSubmission/Prescription.AppointmentId are one-to-one (unique) -> distinct parents.
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildClinicalSql() => @"
INSERT INTO [Forms] ([Id], [Fields], [ServiceId], [StaffId], [Name]) VALUES
('b2220000-0000-0000-0000-000000000001', N'{""fields"":[{""type"":""textarea"",""name"":""symptoms"",""label"":""Describe your symptoms"",""required"":true}]}', '66660000-0000-0000-0000-000000000001', '77770000-0000-0000-0000-000000000001', N'Cardiology Intake'),
('b2220000-0000-0000-0000-000000000002', N'{""fields"":[{""type"":""text"",""name"":""last_cleaning"",""label"":""Last dental cleaning"",""required"":false}]}', '66660000-0000-0000-0000-000000000004', '77770000-0000-0000-0000-000000000003', N'Dental Intake'),
('b2220000-0000-0000-0000-000000000003', N'{""fields"":[{""type"":""textarea"",""name"":""injury"",""label"":""Describe the injury"",""required"":true}]}', '66660000-0000-0000-0000-000000000006', '77770000-0000-0000-0000-000000000004', N'Physio Intake'),
('b2220000-0000-0000-0000-000000000004', N'{""fields"":[{""type"":""file"",""name"":""photos"",""label"":""Skin photos"",""accept"":"".jpg,.png"",""required"":false}]}', '66660000-0000-0000-0000-000000000008', '77770000-0000-0000-0000-000000000005', N'Dermatology Intake'),
('b2220000-0000-0000-0000-000000000005', N'{""fields"":[{""type"":""text"",""name"":""fracture_site"",""label"":""Fracture site"",""required"":true}]}', '66660000-0000-0000-0000-000000000010', '77770000-0000-0000-0000-000000000006', N'Orthopedic Intake');

INSERT INTO [FormSubmissions] ([Id], [AppointmentId], [CreatedAt], [FormId], [Submission]) VALUES
('b3330000-0000-0000-0000-000000000001', 'a1110000-0000-0000-0000-000000000001', '2026-06-28T10:00:00', 'b2220000-0000-0000-0000-000000000001', N'{""symptoms"":""Chest tightness during exercise.""}'),
('b3330000-0000-0000-0000-000000000002', 'a1110000-0000-0000-0000-000000000002', '2026-06-29T11:00:00', 'b2220000-0000-0000-0000-000000000002', N'{""last_cleaning"":""About a year ago.""}'),
('b3330000-0000-0000-0000-000000000003', 'a1110000-0000-0000-0000-000000000003', '2026-06-30T09:00:00', 'b2220000-0000-0000-0000-000000000003', N'{""injury"":""Twisted right ankle playing football.""}'),
('b3330000-0000-0000-0000-000000000004', 'a1110000-0000-0000-0000-000000000004', '2026-07-01T14:00:00', 'b2220000-0000-0000-0000-000000000004', N'{""photos"":""uploaded""}'),
('b3330000-0000-0000-0000-000000000005', 'a1110000-0000-0000-0000-000000000005', '2026-07-02T10:00:00', 'b2220000-0000-0000-0000-000000000005', N'{""fracture_site"":""Left wrist.""}');

INSERT INTO [FormSubmissionMedias] ([Id], [FormSubmissionId], [MediaId]) VALUES
('b33a0000-0000-0000-0000-000000000001', 'b3330000-0000-0000-0000-000000000001', '88880000-0000-0000-0000-000000000027'),
('b33a0000-0000-0000-0000-000000000002', 'b3330000-0000-0000-0000-000000000002', '88880000-0000-0000-0000-000000000028'),
('b33a0000-0000-0000-0000-000000000003', 'b3330000-0000-0000-0000-000000000003', '88880000-0000-0000-0000-000000000027'),
('b33a0000-0000-0000-0000-000000000004', 'b3330000-0000-0000-0000-000000000004', '88880000-0000-0000-0000-000000000027'),
('b33a0000-0000-0000-0000-000000000005', 'b3330000-0000-0000-0000-000000000005', '88880000-0000-0000-0000-000000000028');

INSERT INTO [Prescriptions] ([Id], [AppointmentId], [CreatedAt]) VALUES
('a4440000-0000-0000-0000-000000000001', 'a1110000-0000-0000-0000-000000000001', '2026-06-28T10:45:00'),
('a4440000-0000-0000-0000-000000000002', 'a1110000-0000-0000-0000-000000000002', '2026-06-29T11:50:00'),
('a4440000-0000-0000-0000-000000000003', 'a1110000-0000-0000-0000-000000000003', '2026-06-30T10:15:00'),
('a4440000-0000-0000-0000-000000000004', 'a1110000-0000-0000-0000-000000000004', '2026-07-01T14:45:00'),
('a4440000-0000-0000-0000-000000000005', 'a1110000-0000-0000-0000-000000000005', '2026-07-02T10:40:00');

INSERT INTO [PrescriptionItems] ([Id], [PrescriptionId], [Dose], [Duration], [Frequency], [Name], [Note]) VALUES
('a44a0000-0000-0000-0000-000000000001', 'a4440000-0000-0000-0000-000000000001', N'75mg',   30, 1, N'Aspirin',       N'Take after breakfast'),
('a44a0000-0000-0000-0000-000000000002', 'a4440000-0000-0000-0000-000000000001', N'20mg',   30, 1, N'Atorvastatin',  N'Take at night'),
('a44a0000-0000-0000-0000-000000000003', 'a4440000-0000-0000-0000-000000000002', N'500mg',  7,  3, N'Amoxicillin',   N'Take after meals'),
('a44a0000-0000-0000-0000-000000000004', 'a4440000-0000-0000-0000-000000000002', N'200mg',  5,  2, N'Ibuprofen',     N'For pain relief'),
('a44a0000-0000-0000-0000-000000000005', 'a4440000-0000-0000-0000-000000000003', N'50mg',   10, 2, N'Diclofenac',    N'With food'),
('a44a0000-0000-0000-0000-000000000006', 'a4440000-0000-0000-0000-000000000003', N'500mg',  14, 1, N'Paracetamol',   N'If needed for pain'),
('a44a0000-0000-0000-0000-000000000007', 'a4440000-0000-0000-0000-000000000004', N'10mg',   90, 1, N'Isotretinoin',  N'Avoid sun exposure'),
('a44a0000-0000-0000-0000-000000000008', 'a4440000-0000-0000-0000-000000000005', N'500mg',  30, 2, N'Calcium + VitD',N'Daily supplement'),
('a44a0000-0000-0000-0000-000000000009', 'a4440000-0000-0000-0000-000000000005', N'250mg',  7,  2, N'Naproxen',      N'After meals');

INSERT INTO [PrescriptionTemplates] ([Id], [BottomMargin], [LeftMargin], [OrganizationId], [RightMargin], [TemplateMediaId], [TopMargin], [Name]) VALUES
('a7e50000-0000-0000-0000-000000000001', 20.0, 15.0, '33330000-0000-0000-0000-000000000001', 15.0, '88880000-0000-0000-0000-000000000015', 25.0, N'Nile Care Letterhead'),
('a7e50000-0000-0000-0000-000000000002', 20.0, 15.0, '33330000-0000-0000-0000-000000000002', 15.0, '88880000-0000-0000-0000-000000000016', 25.0, N'Alex Dental Letterhead'),
('a7e50000-0000-0000-0000-000000000003', 20.0, 15.0, '33330000-0000-0000-0000-000000000003', 15.0, '88880000-0000-0000-0000-000000000017', 25.0, N'Giza Physio Letterhead'),
('a7e50000-0000-0000-0000-000000000004', 20.0, 15.0, '33330000-0000-0000-0000-000000000004', 15.0, '88880000-0000-0000-0000-000000000018', 25.0, N'Cairo Derma Letterhead'),
('a7e50000-0000-0000-0000-000000000005', 20.0, 15.0, '33330000-0000-0000-0000-000000000005', 15.0, '88880000-0000-0000-0000-000000000019', 25.0, N'Luxor Ortho Letterhead');";

    // ---------------------------------------------------------------------------------------------------------
    // INVENTORY (+ movements), FEEDBACK, MEDICAL RECORDS (+ allergies), NOTIFICATIONS, ORG VISITS, SUSPENSIONS.
    //   InventoryMovementType: 'Restock' | 'Consume'.  Suspension Reason/Source stored as strings.
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildOperationsSql() => @"
INSERT INTO [InventoryItem] ([Id], [BranchId], [ItemImageId], [MinimumThreshold], [Name], [Quantity], [Notes]) VALUES
('b4440000-0000-0000-0000-000000000001', '55550000-0000-0000-0000-000000000001', '88880000-0000-0000-0000-000000000020', 30, N'Surgical Gloves (Box)',   120, N'Latex-free, medium.'),
('b4440000-0000-0000-0000-000000000002', '55550000-0000-0000-0000-000000000001', '88880000-0000-0000-0000-000000000021', 25, N'Disposable Syringes 5ml',  75, N'Single-use, sterile.'),
('b4440000-0000-0000-0000-000000000003', '55550000-0000-0000-0000-000000000003', NULL,                                   20, N'Gauze Rolls',              18, N'Sterile cotton gauze.'),
('b4440000-0000-0000-0000-000000000004', '55550000-0000-0000-0000-000000000004', NULL,                                   50, N'Alcohol Swabs (Pack)',    200, N'70% isopropyl.'),
('b4440000-0000-0000-0000-000000000005', '55550000-0000-0000-0000-000000000005', NULL,                                   15, N'Examination Couch Paper',  40, NULL),
('b4440000-0000-0000-0000-000000000006', '55550000-0000-0000-0000-000000000006', NULL,                                   10, N'Plaster Bandages',         28, N'Orthopedic casting rolls.');

INSERT INTO [InventoryMovement] ([Id], [InventoryItemId], [MovementType], [OccurredAtUtc], [PerformedByUserId], [Quantity]) VALUES
('b44b0000-0000-0000-0000-000000000001', 'b4440000-0000-0000-0000-000000000001', N'Restock', '2026-06-20T08:00:00', '11110000-0000-0000-0000-000000000001', 160),
('b44b0000-0000-0000-0000-000000000002', 'b4440000-0000-0000-0000-000000000001', N'Consume', '2026-07-03T08:00:00', '11110000-0000-0000-0000-000000000001', 40),
('b44b0000-0000-0000-0000-000000000003', 'b4440000-0000-0000-0000-000000000002', N'Restock', '2026-06-20T08:00:00', '11110000-0000-0000-0000-000000000001', 100),
('b44b0000-0000-0000-0000-000000000004', 'b4440000-0000-0000-0000-000000000002', N'Consume', '2026-07-03T08:00:00', '11110000-0000-0000-0000-000000000001', 25),
('b44b0000-0000-0000-0000-000000000005', 'b4440000-0000-0000-0000-000000000003', N'Restock', '2026-06-21T08:00:00', '11110000-0000-0000-0000-000000000002', 30),
('b44b0000-0000-0000-0000-000000000006', 'b4440000-0000-0000-0000-000000000003', N'Consume', '2026-07-04T08:00:00', '11110000-0000-0000-0000-000000000002', 12),
('b44b0000-0000-0000-0000-000000000007', 'b4440000-0000-0000-0000-000000000004', N'Restock', '2026-06-22T08:00:00', '11110000-0000-0000-0000-000000000003', 240),
('b44b0000-0000-0000-0000-000000000008', 'b4440000-0000-0000-0000-000000000004', N'Consume', '2026-07-05T08:00:00', '11110000-0000-0000-0000-000000000003', 40),
('b44b0000-0000-0000-0000-000000000009', 'b4440000-0000-0000-0000-000000000005', N'Restock', '2026-06-23T08:00:00', '11110000-0000-0000-0000-000000000004', 50),
('b44b0000-0000-0000-0000-000000000010', 'b4440000-0000-0000-0000-000000000006', N'Restock', '2026-06-24T08:00:00', '11110000-0000-0000-0000-000000000005', 28);

INSERT INTO [Feedback] ([Id], [BranchId], [EditedOnUtc], [SubmittedOnUtc], [UserId], [RatingBranch], [RatingService], [RatingSystem], [Comment]) VALUES
('fee00000-0000-0000-0000-000000000001', '55550000-0000-0000-0000-000000000001', NULL, '2026-06-28T12:00:00', '22220000-0000-0000-0000-000000000001', 9, 10, 8, N'Excellent cardiologist and short wait.'),
('fee00000-0000-0000-0000-000000000002', '55550000-0000-0000-0000-000000000003', NULL, '2026-06-29T13:00:00', '22220000-0000-0000-0000-000000000002', 8, 9,  9, N'Clean clinic and friendly staff.'),
('fee00000-0000-0000-0000-000000000003', '55550000-0000-0000-0000-000000000004', '2026-07-01T09:00:00', '2026-06-30T11:00:00', '22220000-0000-0000-0000-000000000003', 7, 8, 7, N'Good physio, parking was hard.'),
('fee00000-0000-0000-0000-000000000004', '55550000-0000-0000-0000-000000000005', NULL, '2026-07-01T16:00:00', '22220000-0000-0000-0000-000000000004', 6, 7, 8, NULL),
('fee00000-0000-0000-0000-000000000005', '55550000-0000-0000-0000-000000000006', NULL, '2026-07-02T12:00:00', '22220000-0000-0000-0000-000000000005', 8, 8, 9, N'Very professional orthopedic team.');

INSERT INTO [MedicalRecords] ([Id], [BloodGlucose], [CustomerId], [HeartRate], [Weight], [BloodPressureDiastolic], [BloodPressureSystolic]) VALUES
('a5e50000-0000-0000-0000-000000000001', 95,  '22220000-0000-0000-0000-000000000001', 72, 68.5, 80, 120),
('a5e50000-0000-0000-0000-000000000002', 102, '22220000-0000-0000-0000-000000000002', 78, 82.0, 85, 128),
('a5e50000-0000-0000-0000-000000000003', 88,  '22220000-0000-0000-0000-000000000003', 68, 55.2, 75, 110),
('a5e50000-0000-0000-0000-000000000004', 110, '22220000-0000-0000-0000-000000000004', 84, 90.3, 90, 135),
('a5e50000-0000-0000-0000-000000000005', 92,  '22220000-0000-0000-0000-000000000005', 70, 60.0, 78, 118);

INSERT INTO [MedicalRecordAllergies] ([MedicalRecordId], [Allergy]) VALUES
('a5e50000-0000-0000-0000-000000000001', N'Penicillin'),
('a5e50000-0000-0000-0000-000000000001', N'Peanuts'),
('a5e50000-0000-0000-0000-000000000002', N'Dust'),
('a5e50000-0000-0000-0000-000000000003', N'Latex'),
('a5e50000-0000-0000-0000-000000000004', N'Aspirin');

INSERT INTO [Notifications] ([Id], [Body], [IsRead], [RecipientId], [SentAt], [Title]) VALUES
('40110000-0000-0000-0000-000000000001', N'Your appointment has been confirmed.',        1, '22220000-0000-0000-0000-000000000001', '2026-06-27T10:00:00', N'Appointment Confirmed'),
('40110000-0000-0000-0000-000000000002', N'Your prescription is ready to view.',         0, '22220000-0000-0000-0000-000000000002', '2026-06-29T12:00:00', N'Prescription Ready'),
('40110000-0000-0000-0000-000000000003', N'Please complete your intake form.',           0, '22220000-0000-0000-0000-000000000003', '2026-06-29T09:00:00', N'Action Required'),
('40110000-0000-0000-0000-000000000004', N'A new appointment was booked at your branch.',1, '11110000-0000-0000-0000-000000000001', '2026-06-27T09:05:00', N'New Booking'),
('40110000-0000-0000-0000-000000000005', N'Your subscription payment succeeded.',        1, '11110000-0000-0000-0000-000000000002', '2026-07-01T00:10:00', N'Payment Received'),
('40110000-0000-0000-0000-000000000006', N'Inventory item is below its threshold.',      0, '11110000-0000-0000-0000-000000000003', '2026-07-04T08:05:00', N'Low Stock Alert');

INSERT INTO [OrganizationVisits] ([Id], [CustomerId], [OrganizationId], [VisitedAt]) VALUES
('07120000-0000-0000-0000-000000000001', '22220000-0000-0000-0000-000000000001', '33330000-0000-0000-0000-000000000001', '2026-06-28T10:00:00'),
('07120000-0000-0000-0000-000000000002', '22220000-0000-0000-0000-000000000002', '33330000-0000-0000-0000-000000000002', '2026-06-29T11:00:00'),
('07120000-0000-0000-0000-000000000003', '22220000-0000-0000-0000-000000000003', '33330000-0000-0000-0000-000000000003', '2026-06-30T09:00:00'),
('07120000-0000-0000-0000-000000000004', '22220000-0000-0000-0000-000000000004', '33330000-0000-0000-0000-000000000004', '2026-07-01T14:00:00'),
('07120000-0000-0000-0000-000000000005', '22220000-0000-0000-0000-000000000005', '33330000-0000-0000-0000-000000000005', '2026-07-02T10:00:00'),
('07120000-0000-0000-0000-000000000006', '22220000-0000-0000-0000-000000000001', '33330000-0000-0000-0000-000000000002', '2026-07-03T10:00:00');

INSERT INTO [Suspensions] ([Id], [OrganizationId], [OwnerId], [Reason], [ScheduledDeletionDateUtc], [Source], [SuspendedById], [SuspensionDateUtc], [Notes], [OrganizationName]) VALUES
('50550000-0000-0000-0000-000000000001', '33330000-0000-0000-0000-000000000004', '11110000-0000-0000-0000-000000000004', N'PolicyViolation', '2026-08-05T00:00:00', N'Admin',  NULL, '2026-07-05T00:00:00', N'Repeated advertising policy violations.', N'Cairo Derma Care'),
('50550000-0000-0000-0000-000000000002', '33330000-0000-0000-0000-000000000005', '11110000-0000-0000-0000-000000000005', N'ExpiredLicense',  '2026-08-06T00:00:00', N'System', NULL, '2026-07-06T00:00:00', N'Medical license expired.',                N'Luxor Ortho Hospital'),
('50550000-0000-0000-0000-000000000003', NULL,                                   '11110000-0000-0000-0000-000000000001', N'Other',           '2026-08-01T00:00:00', N'Admin',  NULL, '2026-07-01T00:00:00', N'Historical suspension, later cleared.',   N'Nile Care Clinic'),
('50550000-0000-0000-0000-000000000004', NULL,                                   '11110000-0000-0000-0000-000000000002', N'PolicyViolation', '2026-08-02T00:00:00', N'System', NULL, '2026-07-02T00:00:00', N'Historical suspension, later cleared.',   N'Alex Dental Smile'),
('50550000-0000-0000-0000-000000000005', NULL,                                   '11110000-0000-0000-0000-000000000003', N'Other',           '2026-08-03T00:00:00', N'Admin',  NULL, '2026-07-03T00:00:00', N'Historical suspension, later cleared.',   N'Giza Physio & Rehab');";

    // ---------------------------------------------------------------------------------------------------------
    // WALLETS + WALLET TRANSACTIONS + WALLET PROMISES. Money -> Amount/Currency. Type: Credit|Debit.
    //   Purpose: Checkout|Recharge|Payout|Refund|Payment. PromiseStatus: Pending|Refunded|Completed|Failed.
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildWalletsSql() => @"
INSERT INTO [Wallets] ([Id], [BranchId], [OpenedAtUtc], [UserId], [BalanceAmount], [BalanceCurrency], [CurrencyCode]) VALUES
('a7770000-0000-0000-0000-000000000001', '55550000-0000-0000-0000-000000000001', '2026-02-10T08:00:00', NULL, 4200.00, N'EGP', N'EGP'),
('a7770000-0000-0000-0000-000000000002', '55550000-0000-0000-0000-000000000003', '2026-02-12T08:00:00', NULL, 3200.00, N'EGP', N'EGP'),
('a7770000-0000-0000-0000-000000000003', '55550000-0000-0000-0000-000000000004', '2026-02-13T08:00:00', NULL, 1500.00, N'EGP', N'EGP'),
('a7770000-0000-0000-0000-000000000004', NULL, '2026-02-02T09:00:00', '22220000-0000-0000-0000-000000000001', 250.00,  N'EGP', N'EGP'),
('a7770000-0000-0000-0000-000000000005', NULL, '2026-02-03T09:00:00', '22220000-0000-0000-0000-000000000002', 400.00,  N'EGP', N'EGP'),
('a7770000-0000-0000-0000-000000000006', NULL, '2026-01-15T09:00:00', '11110000-0000-0000-0000-000000000001', 1000.00, N'EGP', N'EGP');

INSERT INTO [WalletTransactions] ([Id], [CreatedAtUtc], [Description], [Purpose], [ReferenceId], [Type], [WalletId], [Amount], [Currency], [RunningBalanceAmount], [RunningBalanceCurrency]) VALUES
('a8880000-0000-0000-0000-000000000001', '2026-06-15T10:00:00', N'Branch float top-up.',    N'Recharge', N'RCG-0001', N'Credit', 'a7770000-0000-0000-0000-000000000001', 5000.00, N'EGP', 5000.00, N'EGP'),
('a8880000-0000-0000-0000-000000000002', '2026-06-16T10:00:00', N'Wallet recharge.',        N'Recharge', N'RCG-0002', N'Credit', 'a7770000-0000-0000-0000-000000000004', 300.00,  N'EGP', 300.00,  N'EGP'),
('a8880000-0000-0000-0000-000000000003', '2026-06-28T10:30:00', N'Consultation payment.',   N'Payment',  N'PAY-0001', N'Debit',  'a7770000-0000-0000-0000-000000000004', 50.00,   N'EGP', 250.00,  N'EGP'),
('a8880000-0000-0000-0000-000000000004', '2026-06-17T10:00:00', N'Wallet recharge.',        N'Recharge', N'RCG-0003', N'Credit', 'a7770000-0000-0000-0000-000000000005', 400.00,  N'EGP', 400.00,  N'EGP'),
('a8880000-0000-0000-0000-000000000005', '2026-06-30T10:00:00', N'Daily payout to owner.',  N'Payout',   N'PYO-0001', N'Debit',  'a7770000-0000-0000-0000-000000000001', 800.00,  N'EGP', 4200.00, N'EGP'),
('a8880000-0000-0000-0000-000000000006', '2026-06-18T10:00:00', N'Owner wallet recharge.',  N'Recharge', N'RCG-0004', N'Credit', 'a7770000-0000-0000-0000-000000000006', 1000.00, N'EGP', 1000.00, N'EGP');

INSERT INTO [WalletPromises] ([Id], [DestinationTransactionId], [ExpiresAtUtc], [FromWalletId], [ScheduledEventId], [SourceTransactionId], [Status], [ToWalletId], [Amount], [Currency]) VALUES
('a9990000-0000-0000-0000-000000000001', NULL, '2026-07-20T00:00:00', 'a7770000-0000-0000-0000-000000000004', NULL, 'a9995000-0000-0000-0000-000000000001', N'Completed', 'a7770000-0000-0000-0000-000000000001', 50.00,  N'EGP'),
('a9990000-0000-0000-0000-000000000002', NULL, '2026-07-21T00:00:00', 'a7770000-0000-0000-0000-000000000005', NULL, 'a9995000-0000-0000-0000-000000000002', N'Pending',   'a7770000-0000-0000-0000-000000000002', 100.00, N'EGP'),
('a9990000-0000-0000-0000-000000000003', NULL, '2026-07-22T00:00:00', 'a7770000-0000-0000-0000-000000000006', NULL, 'a9995000-0000-0000-0000-000000000003', N'Pending',   'a7770000-0000-0000-0000-000000000003', 200.00, N'EGP'),
('a9990000-0000-0000-0000-000000000004', NULL, '2026-07-23T00:00:00', 'a7770000-0000-0000-0000-000000000001', NULL, 'a9995000-0000-0000-0000-000000000004', N'Completed', 'a7770000-0000-0000-0000-000000000004', 30.00,  N'EGP'),
('a9990000-0000-0000-0000-000000000005', NULL, '2026-07-24T00:00:00', 'a7770000-0000-0000-0000-000000000002', NULL, 'a9995000-0000-0000-0000-000000000005', N'Failed',    'a7770000-0000-0000-0000-000000000005', 75.00,  N'EGP');";

    // ---------------------------------------------------------------------------------------------------------
    // MARKETING (chat sessions, messages, encrypted page credentials) + CHAT SESSIONS + NOTIFICATION TOKENS.
    //   MarketingPostStatus: Draft|Archived|Published|Failed. Role: User|Assistant. Source: User|Groq|Manus.
    //   Intent: MarketingContent|FinalizePost. Page credential token is AES-encrypted at rest.
    // ---------------------------------------------------------------------------------------------------------
    private static string BuildMarketingSql(string facebookToken) => $@"
INSERT INTO [MarketingChatSessions] ([Id], [CreatedAtUtc], [FacebookPostId], [LatestImageUrl], [LatestManusIdea], [OrganizationId], [PendingManusTaskId], [PendingManusTaskUrl], [PostLink], [PostMessage], [Status], [Title], [UpdatedAtUtc], [UserId]) VALUES
('b1110000-0000-0000-0000-000000000001', '2026-07-01T09:00:00', N'1306700202519706_1001', NULL, N'Promote the summer cardiology checkup offer.', '33330000-0000-0000-0000-000000000001', NULL, NULL, N'https://nile-care.dev/offers/summer', N'Beat the heat with a heart-healthy summer! Book your cardiology checkup today.', N'Published', N'Summer Checkup Campaign', '2026-07-01T10:00:00', '11110000-0000-0000-0000-000000000001'),
('b1110000-0000-0000-0000-000000000002', '2026-07-02T09:00:00', NULL, N'https://manus.dev/img/smile.png', N'Announce the new teeth-whitening service.', '33330000-0000-0000-0000-000000000002', NULL, NULL, NULL, NULL, N'Draft', N'Whitening Launch', '2026-07-02T09:30:00', '11110000-0000-0000-0000-000000000002'),
('b1110000-0000-0000-0000-000000000003', '2026-07-03T09:00:00', NULL, NULL, N'Share sports-injury recovery tips.', '33330000-0000-0000-0000-000000000003', N'manus-task-9931', N'https://manus.dev/tasks/9931', NULL, NULL, N'Draft', N'Recovery Tips', '2026-07-03T09:15:00', '11110000-0000-0000-0000-000000000003'),
('b1110000-0000-0000-0000-000000000004', '2026-07-04T09:00:00', N'1306700202519706_1004', NULL, N'Introduce the new laser skin treatment.', '33330000-0000-0000-0000-000000000004', NULL, NULL, N'https://cairo-derma.dev/laser', N'Glow this season with our new laser skin treatment. Limited slots available!', N'Published', N'Laser Treatment Promo', '2026-07-04T10:00:00', '11110000-0000-0000-0000-000000000004'),
('b1110000-0000-0000-0000-000000000005', '2026-07-05T09:00:00', NULL, NULL, N'Post about the orthopedic open day.', '33330000-0000-0000-0000-000000000005', NULL, NULL, NULL, NULL, N'Draft', N'Open Day', '2026-07-05T09:10:00', '11110000-0000-0000-0000-000000000005');

INSERT INTO [MarketingChatMessages] ([Id], [Content], [CreatedAtUtc], [DetectedIntent], [Role], [SessionId], [Source]) VALUES
('b1120000-0000-0000-0000-000000000001', N'Draft a post about our summer cardiology checkup offer.', '2026-07-01T09:01:00', N'MarketingContent', N'User',      'b1110000-0000-0000-0000-000000000001', N'User'),
('b1120000-0000-0000-0000-000000000002', N'Here is a draft promoting your summer cardiology checkup.', '2026-07-01T09:02:00', N'MarketingContent', N'Assistant', 'b1110000-0000-0000-0000-000000000001', N'Manus'),
('b1120000-0000-0000-0000-000000000003', N'Looks great, publish it.', '2026-07-01T09:05:00', N'FinalizePost', N'User',      'b1110000-0000-0000-0000-000000000001', N'User'),
('b1120000-0000-0000-0000-000000000004', N'Your post has been published to Facebook.', '2026-07-01T10:00:00', N'FinalizePost', N'Assistant', 'b1110000-0000-0000-0000-000000000001', N'Groq'),
('b1120000-0000-0000-0000-000000000005', N'Announce our new teeth-whitening service.', '2026-07-02T09:01:00', N'MarketingContent', N'User',      'b1110000-0000-0000-0000-000000000002', N'User'),
('b1120000-0000-0000-0000-000000000006', N'Here is a draft announcing your whitening service.', '2026-07-02T09:02:00', N'MarketingContent', N'Assistant', 'b1110000-0000-0000-0000-000000000002', N'Manus'),
('b1120000-0000-0000-0000-000000000007', N'Give me sports-injury recovery tips to share.', '2026-07-03T09:01:00', N'MarketingContent', N'User',      'b1110000-0000-0000-0000-000000000003', N'User'),
('b1120000-0000-0000-0000-000000000008', N'Working on your recovery-tips post.', '2026-07-03T09:02:00', N'MarketingContent', N'Assistant', 'b1110000-0000-0000-0000-000000000003', N'Manus'),
('b1120000-0000-0000-0000-000000000009', N'Introduce our new laser skin treatment.', '2026-07-04T09:01:00', N'MarketingContent', N'User',      'b1110000-0000-0000-0000-000000000004', N'User'),
('b1120000-0000-0000-0000-000000000010', N'Your laser treatment post has been published.', '2026-07-04T10:00:00', N'FinalizePost', N'Assistant', 'b1110000-0000-0000-0000-000000000004', N'Groq'),
('b1120000-0000-0000-0000-000000000011', N'Post about our orthopedic open day next week.', '2026-07-05T09:01:00', N'MarketingContent', N'User',      'b1110000-0000-0000-0000-000000000005', N'User'),
('b1120000-0000-0000-0000-000000000012', N'Here is a draft for your open day announcement.', '2026-07-05T09:02:00', N'MarketingContent', N'Assistant', 'b1110000-0000-0000-0000-000000000005', N'Manus');

INSERT INTO [MetaPageCredentials] ([Id], [AccessToken], [CreatedAtUtc], [IsActive], [OrganizationId], [PageId], [UpdatedAtUtc]) VALUES
('b1130000-0000-0000-0000-000000000001', N'{facebookToken}', '2026-07-01T08:00:00', 1, '33330000-0000-0000-0000-000000000001', N'1306700202519701', '2026-07-01T08:00:00'),
('b1130000-0000-0000-0000-000000000002', N'{facebookToken}', '2026-07-02T08:00:00', 1, '33330000-0000-0000-0000-000000000002', N'1306700202519702', '2026-07-02T08:00:00'),
('b1130000-0000-0000-0000-000000000003', N'{facebookToken}', '2026-07-03T08:00:00', 1, '33330000-0000-0000-0000-000000000003', N'1306700202519703', '2026-07-03T08:00:00'),
('b1130000-0000-0000-0000-000000000004', N'{facebookToken}', '2026-07-04T08:00:00', 1, '33330000-0000-0000-0000-000000000004', N'1306700202519704', '2026-07-04T08:00:00'),
('b1130000-0000-0000-0000-000000000005', N'{facebookToken}', '2026-07-05T08:00:00', 1, '33330000-0000-0000-0000-000000000005', N'1306700202519705', '2026-07-05T08:00:00');

INSERT INTO [ChatSessions] ([Id], [CreatedAt], [HistoryJson], [LastActiveAt], [Title], [UserId]) VALUES
('b5550000-0000-0000-0000-000000000001', '2026-07-01T09:00:00', N'[{{""role"":""user"",""text"":""What should I eat before a heart checkup?""}}]', '2026-07-01T09:05:00', N'Heart checkup prep', '22220000-0000-0000-0000-000000000001'),
('b5550000-0000-0000-0000-000000000002', '2026-07-02T09:00:00', N'[{{""role"":""user"",""text"":""How often should I get my teeth cleaned?""}}]', '2026-07-02T09:05:00', N'Dental hygiene', '22220000-0000-0000-0000-000000000002'),
('b5550000-0000-0000-0000-000000000003', '2026-07-03T09:00:00', N'[{{""role"":""user"",""text"":""Best stretches for a sprained ankle?""}}]', '2026-07-03T09:05:00', N'Ankle recovery', '22220000-0000-0000-0000-000000000003'),
('b5550000-0000-0000-0000-000000000004', '2026-07-04T09:00:00', N'[{{""role"":""user"",""text"":""Is laser treatment safe for sensitive skin?""}}]', '2026-07-04T09:05:00', N'Laser safety', '22220000-0000-0000-0000-000000000004'),
('b5550000-0000-0000-0000-000000000005', '2026-07-05T09:00:00', N'[{{""role"":""user"",""text"":""How long is recovery after a wrist fracture?""}}]', '2026-07-05T09:05:00', N'Fracture recovery', '22220000-0000-0000-0000-000000000005');

INSERT INTO [UserNotificationTokens] ([Id], [CreatedAt], [DeviceToken], [IsRevoked], [UserId]) VALUES
('07770000-0000-0000-0000-000000000001', '2026-02-02T09:00:00', N'dev-device-token-cust-01', 0, '22220000-0000-0000-0000-000000000001'),
('07770000-0000-0000-0000-000000000002', '2026-02-03T09:00:00', N'dev-device-token-cust-02', 0, '22220000-0000-0000-0000-000000000002'),
('07770000-0000-0000-0000-000000000003', '2026-02-04T09:00:00', N'dev-device-token-cust-03', 0, '22220000-0000-0000-0000-000000000003'),
('07770000-0000-0000-0000-000000000004', '2026-01-15T09:00:00', N'dev-device-token-owner-01', 0, '11110000-0000-0000-0000-000000000001'),
('07770000-0000-0000-0000-000000000005', '2026-01-16T09:00:00', N'dev-device-token-owner-02', 0, '11110000-0000-0000-0000-000000000002'),
('07770000-0000-0000-0000-000000000006', '2026-01-17T09:00:00', N'dev-device-token-owner-03', 1, '11110000-0000-0000-0000-000000000003');";
}
