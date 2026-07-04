using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Scalar.AspNetCore;
using Serilog;
using Viora.Api.Middleware;
using Viora.Api.OpenApi;
using Viora.Application;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Abstractions.Media;
using Viora.Application.AiRag.Ingestion;
using Viora.Domain.Billings;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.Suspensions;
using Viora.Domain.Scheduling;
using Viora.Domain.Services;
using Viora.Domain.Wallets;
using Viora.Infrastructure;
using Viora.Infrastructure.AiRag;
using Viora.Infrastructure.RealTime.Hubs;
using Viora.Infrastructure.Seeding;
using Viora.Infrastructure.Settings;

// Bootstrap logger: captures anything that fails during startup, before the
// configuration-driven logger is wired up. Replaced once the host is built.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    // Load .env first
    var cwd = Directory.GetCurrentDirectory();
    var envPath = Path.Combine(cwd, ".env");
    Env.Load(envPath);


    var builder = WebApplication.CreateBuilder(args);

    // Read the full Serilog configuration from appsettings (sinks, levels, enrichers)
    // and let it resolve services from DI. This becomes the app's ILogger provider.
    builder.Logging.ClearProviders(); // drop the default providers so Serilog is the sole sink
    builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddControllers(options =>
    {
        options.Conventions.Add(new ProducesResponseConvention());
    });
    builder.Services.AddOpenApi(options =>
    {
        options.AddSchemaTransformer<EnumSchemaTransformer>();
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });
    builder.Services.AddAiRagServices(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddSignalR();


    #region Settings
    builder.Services.AddInterfacedOptions<ISchedulingSettings, SchedulingSettings>(
        builder.Configuration, "Scheduling");
    builder.Services.AddInterfacedOptions<IStorageSettings, StorageConfigurations>(
        builder.Configuration, "Storage");
    builder.Services.AddInterfacedOptions<IOnboardingSettings, OnboardingSettings>(
        builder.Configuration, "Onboarding");
    builder.Services.AddInterfacedOptions<ISuspensionSettings, SuspensionSettings>(
        builder.Configuration, "Suspension");
    builder.Services.AddInterfacedOptions<IServiceSettings, ServiceSettings>(
        builder.Configuration, "Service");
    builder.Services.AddInterfacedOptions<IEmailSettings, EmailSettings>(
        builder.Configuration, "Email");
    builder.Services.AddInterfacedOptions<IAdminMessagingSettings, AdminMessagingSettings>(
        builder.Configuration, "Admins");
    builder.Services.AddInterfacedOptions<IBranchSettings, BranchSettings>(
        builder.Configuration, "Branch");
    builder.Services.AddInterfacedOptions<IPaymentSettings, PaymentSettings>(
        builder.Configuration, "Payment");
    builder.Services.AddInterfacedOptions<IWalletSettings, WalletSettings>(
        builder.Configuration, "Wallet");
    #endregion Settings

    var app = builder.Build();

    app.MapHub<ScheduleHub>("/hubs/dashboard");

    // Offline bulk ingestion: `dotnet run -- ingest-specialty [path]`.
    // Runs the full streaming/batched ingest outside the HTTP pipeline (no request
    // timeout) and exits. Falls back to AiRag:SpecialtyBase:FilePath when no path given.
    if (args.Length > 0 && args[0] == "ingest-specialty")
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var command = sp.GetRequiredService<IngestSpecialtyCommand>();
        var cfg = sp.GetRequiredService<IConfiguration>();

        var path = args.Length > 1 ? args[1] : cfg["AiRag:SpecialtyBase:FilePath"];
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            Log.Error("Specialty file not found: {Path}", path ?? "(null)");
            return;
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        Log.Information("Ingesting specialty inquiries from {Path} ...", path);

        await using var stream = File.OpenRead(path);
        await command.ExecuteAsync(SpecialtyInquiryParser.ParseAsync(stream), CancellationToken.None);

        sw.Stop();
        Log.Information("Done in {Elapsed}.", sw.Elapsed);
        return;
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            // Auto-select the Bearer scheme so the token entered once in the Auth panel
            // is attached to every request (persisted by Scalar across reloads).
            options.AddPreferredSecuritySchemes("Bearer");
        });

        using (var scope = app.Services.CreateScope()) // apply pending migrations + reference data
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
            await seeder.SeedAsync();
        }

        // Dev-only scenario data in its OWN scope -> a fresh DbContext. Otherwise the Role singletons
        // it attaches collide with the Role rows the reference seeder just inserted+tracked on a fresh
        // database ("another instance with the same key is already being tracked").
        using (var devScope = app.Services.CreateScope())
        {
            var devSeeder = devScope.ServiceProvider.GetRequiredService<IDevDataSeeder>();
            await devSeeder.SeedAsync();
        }
    }

    // Emits one structured log per HTTP request (method, path, status, elapsed).
    app.UseSerilogRequestLogging();

    // Skipped in Docker — no dev cert available
    // app.UseHttpsRedirection();
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? throw new InvalidOperationException("AllowedOrigins configuration is missing.");
    app.UseCors(corsBuilder =>
        corsBuilder.WithOrigins(allowedOrigins)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials());
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
