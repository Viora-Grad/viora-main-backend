using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Viora.Api.Middleware;
using Viora.Api.OpenApi;
using Viora.Application;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Abstractions.Media;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.Suspensions;
using Viora.Domain.Scheduling;
using Viora.Domain.Services;
using Viora.Infrastructure;
using Viora.Infrastructure.Seeding;
using Viora.Infrastructure.Settings;


// Load .env first
var cwd = Directory.GetCurrentDirectory();
var envPath = Path.Combine(cwd, ".env");
Env.Load(envPath);


var builder = WebApplication.CreateBuilder(args);
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
});

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
#endregion Settings

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope(); // apply pending migrations on startup
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? throw new InvalidOperationException("AllowedOrigins configuration is missing.");
app.UseCors(builder =>
    builder.WithOrigins(allowedOrigins)
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials());
app.Run();
