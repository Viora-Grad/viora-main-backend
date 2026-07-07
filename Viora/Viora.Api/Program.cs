using DotNetEnv;
using Microsoft.EntityFrameworkCore;
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
using Viora.Domain.Marketing;
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
    builder.Services.AddInterfacedOptions<IManusSettings, ManusSettings>(
        builder.Configuration, "Manus");
    builder.Services.AddInterfacedOptions<IMetaSettings, MetaSettings>(
        builder.Configuration, "Meta");
    #endregion Settings

    var app = builder.Build();

    app.MapHub<ScheduleHub>("/hubs/dashboard");

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
            options.AddPreferredSecuritySchemes("Bearer");
        });

        using (var scope = app.Services.CreateScope()) // apply pending migrations + reference data
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
            await seeder.SeedAsync();
        }

        using (var devScope = app.Services.CreateScope())
        {
            var devSeeder = devScope.ServiceProvider.GetRequiredService<IDevDataSeeder>();
            await devSeeder.SeedAsync();
        }
    }

    // Emits one structured log per HTTP request (method, path, status, elapsed).
    app.UseSerilogRequestLogging();

    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseCors(corsBuilder =>
            corsBuilder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
        );
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
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
