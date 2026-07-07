# Viora — Comprehensive Test Analysis & Plan

> **Project:** Viora (.NET 10, Clean Architecture)  
> **Framework:** MSTest 4.0.2 + Moq 4.20.72  
> **Pattern:** Arrange-Act-Assert (strict), max 4 tests/function  
> **Date:** 2026-07-06  
> **Priorities:** Staff → Appointments → Reminders → Notifications → Authentication

---

## Table of Contents

1. [Deliverable 1: Layer-by-Layer Analysis](#deliverable-1-layer-by-layer-analysis)
   - [Architecture Overview](#architecture-overview)
   - [Domain Layer](#domain-layer)
   - [Application Layer](#application-layer)
   - [Infrastructure Layer](#infrastructure-layer)
   - [Api Layer](#api-layer)
   - [Existing Test Coverage](#existing-test-coverage)
   - [Coverage Gaps](#coverage-gaps)
   - [Refresh Token Architecture](#refresh-token-architecture)
2. [Deliverable 2: Test Plan](#deliverable-2-test-plan)
   - [Priority 1: Staff](#priority-1-staff)
   - [Priority 2: Appointments](#priority-2-appointments)
   - [Priority 3: Reminders](#priority-3-reminders)
   - [Priority 4: Notifications](#priority-4-notifications)
   - [Priority 5: Authentication](#priority-5-authentication)
   - [Infrastructure Cross-Cutting](#infrastructure-cross-cutting)
3. [Deliverable 3: Sample Tests](#deliverable-3-sample-tests)
   - [Domain: AppointmentTests.cs](#domain-appointmenttestscs)
   - [Domain: UserTests.cs](#domain-usertestscs)
   - [Application: CancelAppointmentCommandHandlerTests.cs](#application-cancelappointmentcommandhandlertestscs)
   - [Infrastructure: RefreshTokenServiceTests.cs](#infrastructure-refreshtokenservicetestscs)
   - [Infrastructure: AuthenticationServiceTests.cs](#infrastructure-authenticationservicetestscs)

---

## Deliverable 1: Layer-by-Layer Analysis

### Architecture Overview

```
Viora.slnx
├── Viora.Api          (ASP.NET Controllers, Middleware, OpenAPI)
├── Viora.Application   (CQRS Commands/Queries, Handlers, Validators, Abstractions)
├── Viora.Domain        (Entities, Value Objects, Interfaces, Errors, Events)
├── Viora.Infrastructure (EF Core, Repositories, Auth, External Services)
└── Viora.Test          (MSTest + Moq)
```

**Reference graph:**
```
Api → Application, Infrastructure
Application → Domain
Infrastructure → Application, Domain
Domain → (none)
Test → Domain (currently only)
```

**Key architectural patterns:**
- **CQRS** via MediatR: `ICommand<T>`, `IQuery<T>`, `ICommandHandler<T>`, `IQueryHandler<T>`
- **Result pattern**: `Result`/`Result<T>` with `Error` records + `ErrorCategory` enum — no exceptions for business logic
- **Domain Events**: `IDomainEvent : INotification` raised on entities, handled by MediatR notification handlers
- **Value Objects**: Strongly typed wrappers in `Internal/` sub-folders with implicit conversions
- **Repository pattern**: Interface in Domain, implementation in Infrastructure
- **Unit of Work**: `IUnitOfWork` wrapping EF Core's `SaveChangesAsync`
- **Validation**: FluentValidation via MediatR pipeline behavior
- **Pipeline behaviors**: Logging → Validation → LimitedFeature → QueryCaching

---

### Domain Layer

**Location:** `Viora.Domain/`  
**Package:** `Viora.Domain.csproj` (net10.0, no project dependencies)

#### Base Abstractions (`Viora.Domain/Abstractions/`)

| File | Type | Key Members |
|------|------|-------------|
| `Entity.cs` | Abstract class | `Id`, `DomainEvents`, `RaiseDomainEvent()`, `ClearDomainEvents()` |
| `Result.cs` | Class | `IsSuccess`, `IsFailure`, `Error`, `Value`; static `Success()`, `Failure()` |
| `Result<T>` | Generic class | Inherits `Result`; typed `Value` with access guard |
| `Error.cs` | Record | `Name`, `Description`, `Category`; static `NoError`, `NullValue` |
| `IDomainEvent.cs` | Interface | `: INotification` (MediatR) |
| `IUnitOfWork.cs` | Interface | `SaveChangesAsync()`, `BeginTransactionAsync()` |
| `BaseSpecification.cs` | Abstract | Criteria, Includes, Ordering, Paging |

#### Priority Entity: Staff

**File:** `Viora.Domain/Staffs/Staff.cs`  
**Aggregate Root:** Yes  
**Key State:** `OrganizationId`, `FirstName`, `LastName`, `Username`, `HashedPassword`, `DateOfBirth`, `Gender`, `StaffStatus`, `PhoneNumber`, `CreatedAt`, `DeletedAt`, `Roles`, `Branches`, `Services`  
**Statuses:** `Pending → Active/Suspended` (via soft-delete)

| Method | Returns | Preconditions | Side Effects |
|--------|---------|---------------|--------------|
| `Create()` | `Staff` | — | Pending status |
| `AddRoles()` | `void` | non-null, non-empty | Guards duplicates |
| `UpdateRoles()` | `void` | non-null, non-empty | Clears + replaces |
| `AssignBranches()` | `void` | non-null, non-empty | Guards duplicates |
| `AssignServices()` | `void` | non-null, non-empty | Guards duplicates |
| `UpdateServices()` | `void` | non-null, non-empty | Clears + replaces |
| `SetStaffProperties()` | `void` | — | Sets all profile fields |
| `Activate()` | `Result` | Not Active, ValidInstance (all fields + branches > 0) | Status → Active |
| `Suspend()` | `Result` | Not Suspended | Status → Suspended |
| `Delete()` | `Result` | — | Sets `DeletedAt` |
| `RemoveRoles()` | `void` | non-null, non-empty | |
| `SeedActiveStaff()` | `Staff` | — | Static factory for seeding |

**Error codes:** `StaffErrors` — `StaffNotFound`, `StaffAlreadyExists`, `StaffAlreadyActive`, `InvalidStaffInstance`, `StaffAlreadySuspended`

**Value Objects** (`Staffs/Internal/`): `FirstName`, `LastName`, `Username`, `HashedPassword`, `Gender` (enum), `StaffStatus` (enum)

---

#### Priority Entity: StaffToken

**File:** `Viora.Domain/Staffs/StaffToken.cs`  
**Aggregate Root:** No (owned by Staff)

| Method | Returns | Preconditions | Side Effects |
|--------|---------|---------------|--------------|
| `Create()` | `StaffToken` | — | — |
| `IsValid(now)` | `bool` | — | Check: not revoked, not used, not expired |
| `Revoke()` | `void` | Not already revoked | Sets `RevokedAt` |
| `MarkAsUsed()` | `void` | Not already used | Sets `UsedAt` |

**Lifecycle:** Each token has independent `RevokedAt` and `UsedAt`. A user can have multiple active tokens simultaneously; revocation is per-token.

---

#### Priority Entity: Appointment

**File:** `Viora.Domain/Appointments/Appointment.cs`  
**Aggregate Root:** Yes  
**Statuses:** `NotArrived → Waiting → InProgress → Completed | NoShow | Canceled`

| Method | Returns | Preconditions | Domain Event |
|--------|---------|---------------|--------------|
| `Book()` | `Appointment` | — | `AppointmentBookedEvent` |
| `CheckIn()` | `Result` | Status == NotArrived (for Customer); time within 30min window | `AppointmentCheckedInEvent` |
| `Start()` | `Result` | Status == Waiting | — |
| `Complete()` | `Result` | Status != Completed; time not too early | `AppointmentCompletedEvent` |
| `Delay(TimeSpan)` | `Result` | Status != Completed/InProgress | `AppointmentDelayedEvent` |
| `Delay(DateTime)` | `Result` | Status != Completed/InProgress | `AppointmentDelayedEvent` |
| `NoShow()` | `Result` | Status == NotArrived; time >= ReservationDate | `AppointmentNoShowEvent` |
| `Cancel()` | `Result` | Status != Completed/InProgress; for Customer: 2h window | `AppointmentCanceledEvent` |

**Error codes:** `AppointmentErrors` — `CheckInProhibited`, `StartProhibited`, `CompleteProhibited`, `DelayProhibited`, `CancellationProhibited`, `NoShowProhibited`, `InvalidAppointmentTime`, `AppointmentTimeConflict`, `CheckInNotWithinAcceptableWindow`, `NoShowTimeInvalid`

**Value Objects/Enums:** `CustomerStatus` ({NotArrived, Waiting, InProgress, Completed, NoShow, Canceled}), `Creator` ({None, Customer, Staff}), `PaymentMethod` ({Cash, Wallet, Online}), `Platform` ({None, Web, Mobile})

---

#### Priority Entity: Reminder

**File:** `Viora.Domain/Reminders/Reminder.cs`  
**Aggregate Root:** Yes (associated with Appointment)

| Method | Returns | Preconditions | Domain Event |
|--------|---------|---------------|--------------|
| `Create()` | `Reminder` | — | `ReminderCreatedEvent` (commented out) |

**Value Objects:** `TItle` (validates non-empty, ≤200 chars), `Body` (string wrapper)

**Error codes:** `ReminderErrors` — `ReminderCustomerMissing`, `ReminderAppointmentNotCompleted`

---

#### Priority Entity: Notification

**File:** `Viora.Domain/Notifications/Notification.cs`  
**Aggregate Root:** Yes

| Method | Returns | Preconditions | Side Effects |
|--------|---------|---------------|--------------|
| `Create()` | `Notification` | — | Creates with IsRead=false |
| `MarkAsRead()` | `void` | — | Sets IsRead=true |

**Value Objects:** `Title` (string wrapper), `Body` (string wrapper)

---

#### Priority Entity: User (Authentication)

**File:** `Viora.Domain/Users/Identity/User.cs`  
**Aggregate Root:** Yes  
**Statuses:** `Active | Deactivated | Deleted`

| Method | Returns | Preconditions |
|--------|---------|---------------|
| `Create()` | `User` | Assigns `Role.Registered` |
| `LinkIdentity()` | `Result` | Not null, not duplicate provider+key |
| `RecordLogin()` | `void` | — |
| `PromoteToOwner()` | `Result` | Not already Owner |
| `BecomeCustomer()` | `Result` | Not already Customer |
| `AddRole()` | `Result` | Not already assigned |
| `Activate()` | `void` | — |
| `Deactivate()` | `void` | — |
| `MarkAsDeleted()` | `void` | — |
| `VerifyEmail()` | `void` | — |

**Error codes:** `UserErrors` — `NotFound`, `InvalidCredentials`, `EmailInUse`, `AlreadyOwner`, `AlreadyCustomer`, `RoleAlreadyAssigned`, `IdentityLinked`

---

#### Priority Entity: RefreshToken (Infrastructure-level)

**File:** `Viora.Infrastructure/Authentication/RefreshToken.cs`  
**Scope:** Internal (Infrastructure)

| Method | Returns | Preconditions |
|--------|---------|---------------|
| `Create()` | `RefreshToken` | — |
| `Revoke()` | `void` | — |

#### Priority Entity: StaffRefreshToken (Infrastructure-level)

**File:** `Viora.Infrastructure/Authentication/StaffRefreshToken.cs`  
**Scope:** Internal (Infrastructure)

| Method | Returns | Preconditions |
|--------|---------|---------------|
| `Create()` | `StaffRefreshToken` | — |
| `Revoke()` | `void` | — |

---

#### Entity Relationship Diagram

```
User (1) ────── (0..*) RefreshToken     [User can have many active refresh tokens]
User (1) ────── (0..*) AuthIdentity     [User can have many OAuth identities]
User (1) ────── (0..*) Role             [via UserRoleAssignment]
Staff (1) ────── (0..*) StaffToken       [Staff can have many invitation/refresh tokens]
Staff (1) ────── (0..*) Role             [via StaffRoles]
Staff (1) ────── (0..*) Branch           [via assignment table]
Staff (1) ────── (0..*) Service          [via assignment table]
Appointment ────── Staff (N:1)
Appointment ────── Customer (N:1)
Appointment ────── Service (N:1)
Appointment ────── Branch (N:1)
Appointment (1) ────── (0..*) Reminder
Reminder ────── Appointment (N:1)
Notification ────── User (N:1)
```

---

### Application Layer

**Location:** `Viora.Application/`  
**CQRS:** Each feature has `{Action}/{Action}Command.cs`, `{Action}/{Action}CommandHandler.cs`, optionally `{Action}/{Action}Validator.cs`

#### Handler Pattern (Command)

```csharp
internal sealed class XxxCommandHandler(
    IStaffRepository staffRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock,
    IUserContext context
) : ICommandHandler<XxxCommand>
{
    public async Task<Result> Handle(XxxCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct);
        if (entity is null) return Result.Success(); // or throw NotFoundException
        // authorization check: context.OrganizationId vs entity.OrganizationId
        entity.SomeMethod(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
```

#### Handler Pattern (Query)

```csharp
internal sealed class GetXxxQueryHandler(
    IXxxRepository repository
) : IQueryHandler<GetXxxQuery, Domain.Xxx.Entity>
{
    public async Task<Result<Domain.Xxx.Entity>> Handle(...)
    {
        var entity = await repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Not found");
        return Result.Success(entity);
    }
}
```

#### Validator Pattern

```csharp
internal class CreateXxxCommandValidator : AbstractValidator<CreateXxxCommand>
{
    public CreateXxxCommandValidator(IDateTimeProvider clock)
    {
        RuleFor(x => x.SomeField).NotEmpty();
        RuleFor(x => x.Date).GreaterThan(() => clock.UtcNow);
    }
}
```

#### Pipeline Behaviors (registered in this order)

1. `LoggingBehavior<,>` — Serilog logging
2. `ValidationBehavior<,>` — runs FluentValidation validators
3. `LimitedFeaturePipelineBehavior<,>` — quota check
4. `QueryCachingBehavior<,>` — cache-first for `ICachedQuery`

---

### Infrastructure Layer

**Location:** `Viora.Infrastructure/`

#### Key Services

| Service | File | Dependencies | Complexity |
|---------|------|--------------|------------|
| `AuthenticationService` | `Authentication/AuthenticationService.cs` | 10+ deps | Very High (login, register, refresh, social, staff auth, logout, password change) |
| `RefreshTokenService` | `Authentication/RefreshTokenService.cs` | 2 deps | Medium (generate, hash, expiry) |
| `JwtService` | `Authentication/JwtService.cs` | 2 deps | Medium (token generation) |
| `TokenValidator` | `Authentication/TokenValidator.cs` | IConfiguration | Medium (Google token validation) |
| `UserContext` | `Authentication/UserContext.cs` | IHttpContextAccessor | Low |
| `Hasher` | `Security/Hasher.cs` | — | Low |
| `Cipher` | `Security/Cipher.cs` | — | Low |
| `DateTimeProvider` | `Clock/DateTimeProvider.cs` | — | Trivial |
| `NotificationService` | `NotificationService/NotificationService.cs` | Firebase | Medium |
| `ScheduleNotifier` | `RealTime/ScheduleNotifier.cs` | SignalR Hub | Medium |

#### Repository Implementations (~40+)

All implement Domain interfaces via generic `Repository<T>` base. Examples:
- `StaffRepository` → `IStaffRepository`
- `AppointmentsRepository` → `IAppointmentsRepository`
- `NotificationRepository` → `INotificationRepository`
- `ReminderRepository` → `IReminderRepository`
- `RefreshTokenRepository` → internal
- `StaffRefreshTokenRepository` → internal

#### Authentication Repositories (Infrastructure-internal)

| Repository | Methods |
|-----------|---------|
| `LocalCredentialRepository` | Add, GetByUserIdAsync |
| `RefreshTokenRepository` | Add, GetByTokenAsync, GetActiveTokenByUserIdAsync |
| `StaffRefreshTokenRepository` | Add, GetByTokenAsync, GetActiveStaffTokenByStaffIdAsync |

---

### Api Layer

**Location:** `Viora.Api/`

| Controller | Key Endpoints |
|-----------|---------------|
| `AuthController` | POST register, login, refresh, social-login, logout, change-password |
| `AppointmentsController` | POST book, PATCH check-in, PATCH complete, PATCH cancel, PATCH no-show, PATCH delay |
| `StaffsController` | CRUD, invitations, roles |
| `RemindersController` | CRUD |
| `NotificationsController` | GET, PATCH mark-read |
| GlobalExceptionMiddleware | Catches all exceptions → ProblemDetails |
| ActionResultMapper | `result.ToActionResult()` maps Result → IActionResult |

---

### Existing Test Coverage

**Project:** `Viora.Test/Viora.Test.csproj`  
**Structure:**
```
Compenents/
├── Api/                    (empty)
├── Application/            (empty)
├── Domain/
│   └── Staffs/
│       ├── StaffTests.cs           (20 tests)
│       └── StaffTokenTests.cs      (10 tests)
└── Infrastructure/         (empty)
```

#### StaffTests.cs Coverage (20 tests)

| Group | Tests | Pattern |
|-------|-------|---------|
| Create (3) | ValidInput, WithoutId, WithProvidedId | Pure unit |
| AddRoles (4) | Null, Empty, Valid, Duplicate | Pure unit + exception |
| AssignBranches (3) | Null, Empty, Valid | Pure unit + exception |
| AssignServices (4) | Null, Empty, Valid, Duplicate | Pure unit + exception |
| SetStaffProperties (1) | All assigned | Pure unit |
| Activate (4) | AlreadyActive, MissingProperties, MissingBranches, ValidInstance | Result pattern |
| Suspend (2) | FirstCall, AlreadySuspended | Result pattern |
| Delete (1) | Sets DeletedAt | Pure unit |
| RemoveRoles (3) | Null, Empty, Existing, NonExistent | Pure unit + exception |
| SeedActiveStaff (2) | AllFields, DefaultDateOfBirth | Pure unit |

#### StaffTokenTests.cs Coverage (10 tests)

| Group | Tests | Pattern |
|-------|-------|---------|
| Create (1) | SetsAllFields | Pure unit |
| IsValid (5) | Valid, Revoked, Used, EqualsExpiration, AfterExpiration | Result pattern |
| Revoke (2) | SetsRevokedAt, DoubleRevoke | Pure + exception |
| MarkAsUsed (2) | SetsUsedAt, DoubleUse | Pure + exception |

**Total: 30 tests, all Domain-only. Zero Application, Infrastructure, or Api tests.**

---

### Coverage Gaps

#### Gap Severity Legend
- **CRITICAL** — Core business logic with zero coverage
- **HIGH** — Complex logic with multiple branches
- **MEDIUM** — Simple logic or well-isolated
- **LOW** — Trivial pass-through

#### Domain Gaps

| Entity | Method | Lines of Logic | Branches | Severity | Reason |
|--------|--------|----------------|----------|----------|--------|
| **Appointment** | `Book()` | 20 | 1 | **CRITICAL** | Aggregate root factory, raises event |
| **Appointment** | `CheckIn()` | 23 | 4 | **CRITICAL** | Two paths (Staff vs Customer), window check |
| **Appointment** | `Start()` | 8 | 2 | HIGH | Status guard |
| **Appointment** | `Complete()` | 12 | 3 | **CRITICAL** | Status guard + time guard + event |
| **Appointment** | `Delay(TimeSpan)` | 9 | 2 | HIGH | Status guard + event |
| **Appointment** | `Delay(DateTime)` | 10 | 2 | HIGH | Status guard + event |
| **Appointment** | `NoShow()` | 9 | 3 | HIGH | Status guard + time guard + event |
| **Appointment** | `Cancel()` | 14 | 4 | **CRITICAL** | Two creator paths, status guard, 2h window + event |
| **Reminder** | `Create()` | 8 | 1 | MEDIUM | Simple factory |
| **Notification** | `Create()` | 5 | 1 | MEDIUM | Simple factory |
| **Notification** | `MarkAsRead()` | 2 | 1 | MEDIUM | Simple setter |
| **User** | `Create()` | 5 | 1 | **CRITICAL** | Factory with default role |
| **User** | `LinkIdentity()` | 8 | 3 | HIGH | Null + duplicate guard |
| **User** | `PromoteToOwner()` | 5 | 2 | HIGH | Already-owner check |
| **User** | `BecomeCustomer()` | 5 | 2 | HIGH | Already-customer check |
| **User** | `AddRole()` | 5 | 2 | HIGH | Already-assigned check |
| **User** | `Activate()`/`Deactivate()`/`MarkAsDeleted()` | 2 each | 1 | MEDIUM | Simple setters |
| **User** | `RecordLogin()` | 2 | 1 | LOW | Simple setter |
| **User** | `VerifyEmail()` | 2 | 1 | LOW | Simple setter |
| **StaffRoles** | `Create()` | 8 | 2 | MEDIUM | Guard checks |

#### Application Gaps

| Handler | Dependencies | Logic Complexity | Severity | Reason |
|---------|-------------|------------------|----------|--------|
| All Appointment handlers (7+ commands) | 4-5 deps | Medium-High | **CRITICAL** | Total gap — 13 sub-folders, 0 tests |
| All Authentication handlers (via IAuthenticationService) | 10+ deps | Very High | **CRITICAL** | login, register, refresh, social auth |
| All Staff handlers (14 sub-folders) | 3-5 deps | Medium | **CRITICAL** | Total gap |
| All Reminder handlers (3 sub-folders) | 3-4 deps | Medium | HIGH | Total gap |
| All Notification handlers (3 sub-folders) | 2-3 deps | Low-Medium | HIGH | Total gap |
| Pipeline Behaviors (4) | 2-3 deps each | Medium | HIGH | Cross-cutting concerns |
| All Validators | 1 dep (IDateTimeProvider) | Low | MEDIUM | Validation rules |

#### Infrastructure Gaps

| Service | Dependencies | Logic Complexity | Severity | Reason |
|---------|-------------|------------------|----------|--------|
| `AuthenticationService` | 10+ deps | Very High | **CRITICAL** | Central auth logic, token rotation, multiple paths |
| `RefreshTokenService` | 2 deps | Medium | **CRITICAL** | Token generation/hashing — security-critical |
| `JwtService` | 2 deps | Medium | **CRITICAL** | JWT creation, signing — security-critical |
| `TokenValidator` | 1 dep | Medium | MEDIUM | Google OAuth validation |
| `NotificationService` | 2 deps | Medium | MEDIUM | Push notification via Firebase |
| `Hasher` | 0 | Low | MEDIUM | BCrypt wrapper |
| All repositories (~40) | DbContext | Low | MEDIUM | Integration-level, but some query logic |

#### Api Gaps

| Component | Severity | Reason |
|-----------|----------|--------|
| All controllers (28) | **CRITICAL** | Zero integration tests |
| `GlobalExceptionMiddleware` | HIGH | Error handling path |
| `ActionResultMapper` | HIGH | Error → HTTP status mapping |

---

### Refresh Token Architecture

**This is critical — the user specifically called out refresh token semantics.**

#### Current Design

There are **two parallel refresh token systems**:

1. **User Refresh Tokens** (`Viora.Infrastructure.Authentication.RefreshToken`)
   - Associated with `User.Id`
   - Stored in SQL via EF Core
   - Used by `AuthenticationService.RefreshTokenAsync()` — **does NOT rotate** (the same token can be reused)
   - Used by `AuthenticationService.LogoutAsync()` — revokes by hash lookup
   - `ChangePassword` / `UpdatePassword` — revokes active token

2. **Staff Refresh Tokens** (`Viora.Infrastructure.Authentication.StaffRefreshToken`)
   - Associated with `Staff.Id`
   - Stored in SQL via EF Core
   - Used by `AuthenticationService.RefreshStaffTokenAsync()` — **rotates** (old revoked, new created)
   - Used by `AuthenticationService.AuthenticateStaffAsync()` — revokes previous active token

3. **Staff Invitation Tokens** (`Viora.Domain.Staffs.StaffToken`)
   - Domain entity, used for email-based staff invitations
   - Independent lifecycle: `Created → Used | Revoked`
   - Validation: `IsValid(now) = !IsRevoked && !IsUsed && now < Expiration`

#### Multi-Token Behavior

| System | Multiple Active Tokens | Individual Revocation | Token Rotation |
|--------|------------------------|----------------------|----------------|
| User Refresh (`RefreshToken`) | **Yes** (no prior revoke on refresh) | **Yes** (`LogoutAsync` looks up by hash) | **No** (same token reused) |
| Staff Refresh (`StaffRefreshToken`) | **No** (previous revoked on new auth) | **Yes** (per-token revoke) | **Yes** (rotation on refresh) |
| Staff Invitation (`StaffToken`) | **Yes** (independent lifecycle) | **Yes** (per-token `Revoke()`) | N/A (one-time use) |

#### Gaps in Current Design

1. `RefreshToken.RefreshTokenAsync()` (line 82-123 of AuthenticationService.cs) — the token is **not revoked after use** and a **new one is not issued**. This means the same refresh token can be used multiple times, which is a **security concern**.
2. `StaffRefreshToken` rotation is properly implemented — old revoked, new created.
3. `StaffToken` (Domain) has proper independent lifecycle with `IsValid()`, `Revoke()`, `MarkAsUsed()`. This token type is already well-covered by `StaffTokenTests.cs`.

---

## Deliverable 2: Test Plan

### Testing Rules

- **Arrange-Act-Assert** (strict)
- **Max 4 tests** per function/method
- **Coverage MUST include**: happy path + invalid input + dependencies + error handling
- **Naming pattern**: `MethodName_Scenario_ExpectedOutcome`
- **Framework**: MSTest (`[TestClass]`, `[TestMethod]`)
- **Mocking**: Moq
- **File structure**: `Viora.Test/Compenents/{Layer}/{Feature}/{EntityName}Tests.cs`

### Priority 1: Staff

#### Domain — Staff (already covered: 20 tests)

| Function | Existing | Needed | Priority |
|----------|----------|--------|----------|
| `Create` | 3 tests | OK | — |
| `AddRoles` | 4 tests | OK | — |
| `AssignBranches` | 3 tests | OK | — |
| `AssignServices` | 4 tests | OK | — |
| `SetStaffProperties` | 1 test | OK | — |
| `Activate` | 4 tests | OK | — |
| `Suspend` | 2 tests | OK | — |
| `Delete` | 1 test | OK | — |
| `RemoveRoles` | 3 tests | OK | — |
| `SeedActiveStaff` | 2 tests | OK | — |
| `UpdateRoles` | 0 | Add 3: null, empty, valid | HIGH |
| `UpdateServices` | 0 | Add 3: null, empty, valid | HIGH |

#### Domain — StaffToken (already covered: 10 tests)

| Function | Existing | Needed | Priority |
|----------|----------|--------|----------|
| `Create` | 1 test | OK | — |
| `IsValid` | 5 tests | OK | — |
| `Revoke` | 2 tests | OK | — |
| `MarkAsUsed` | 2 tests | OK | — |

#### Domain — StaffRoles (0 tests, MEDIUM priority)

| Function | Test Scenarios |
|----------|---------------|
| `Create` | Valid → sets all fields; Empty StaffId → throws; Invalid RoleId → throws |
| `Revoke` | First call → sets RevokedAt; Already revoked → no-op |

#### Application — Staff Command Handlers (0 tests, CRITICAL)

| Handler | Test Scenarios |
|---------|---------------|
| **RegisterStaffCommandHandler** | Valid staff → creates + saves; Missing org → unauthorized; Duplicate username → failure |
| **DeleteStaffCommandHandler** | Valid staff → deletes + saves; Staff not found → success (idempotent); Different org → unauthorized |
| **CreateStaffInvitationCommandHandler** | Valid → creates token; Staff not found → not found; Invalid role → failure |
| **ActivateStaffCommandHandler** | Valid → activates; Already active → failure; Invalid props → failure |
| **SuspendStaffCommandHandler** | Valid → suspends; Already suspended → failure |
| **UpdateStaffRolesCommandHandler** | Valid → updates roles; Null roles → validation failure |
| **AssignStaffBranchesCommandHandler** | Valid → assigns; Empty → validation failure |

#### Application — Staff Query Handlers (0 tests, HIGH)

| Handler | Test Scenarios |
|---------|---------------|
| **GetStaffQueryHandler** | Staff found → returns staff; Staff not found → throws NotFoundException |
| **GetOrganizationStaffQueryHandler** | Staff exist → returns list; Empty org → empty list |

### Priority 2: Appointments

#### Domain — Appointment (0 tests, CRITICAL)

| Function | Max Tests | Scenarios |
|----------|-----------|-----------|
| `Book` | 4 | Valid booking → sets all fields; With null customer → ok; With customer status override; Raises AppointmentBookedEvent |
| `CheckIn` (Customer) | 4 | Valid check-in within window → success; Already checked in → CheckInProhibited; Too early (before 30min window) → CheckInNotWithinAcceptableWindow; Check-in with Staff creator → always allowed |
| `CheckIn` (Staff) | 2 | Staff check-in → bypasses window check; Staff check-in → sets InProgress + IsCheckedIn |
| `Start` | 3 | Waiting status → success; NotArrived → StartProhibited; InProgress → StartProhibited |
| `Complete` | 4 | InProgress → success; Already completed → CompleteProhibited; Too early (before -1h) → CompleteProhibited; Raises AppointmentCompletedEvent |
| `Delay(TimeSpan)` | 3 | Valid delay → updates ReservationDate; Completed → DelayProhibited; Raises AppointmentDelayedEvent |
| `Delay(DateTime)` | 3 | Valid delay → updates ReservationDate; InProgress → DelayProhibited; Raises AppointmentDelayedEvent |
| `NoShow` | 4 | NotArrived + time >= reservation → success; Already checked in → NoShowProhibited; NoShow time before reservation → NoShowTimeInvalid; Raises AppointmentNoShowEvent |
| `Cancel` (Customer) | 4 | Valid cancel with 2h+ window → success; Cancel within 2h → CancellationProhibited; Completed → CancellationProhibited; Raises AppointmentCanceledEvent |
| `Cancel` (Staff) | 2 | Staff cancel → bypasses 2h window; InProgress → CancellationProhibited |

#### Application — Appointment Command Handlers (0 tests, CRITICAL)

| Handler | Test Scenarios |
|---------|---------------|
| **CreateAppointmentCommandHandler** | Valid → creates + saves + returns; Service not found → exception; Staff not available → conflict |
| **CancelAppointmentCommandHandler** | Valid → cancels; Appointment not found → exception; Already completed → failure |
| **CheckInAppointmentCommandHandler** | Valid → checks in; Appointment not found → exception; Invalid state → failure |
| **CompleteAppointmentCommandHandler** | Valid → completes; Not found → exception |
| **NoShowAppointmentCommandHandler** | Valid → no-show; Not found → exception |
| **DelayAppointmentCommandHandler** | Valid → delays; Not found → exception |

#### Application — Appointment Validators (0 tests, MEDIUM)

| Validator | Test Scenarios |
|-----------|---------------|
| **CreateAppointmentValidator** | Valid command → passes; Empty ServiceId → fails; Empty StaffId → fails; Past ReservationDate → fails; Invalid enum values → fails |

### Priority 3: Reminders

#### Domain — Reminder (0 tests, MEDIUM)

| Function | Max Tests | Scenarios |
|----------|-----------|-----------|
| `Create` | 3 | Valid → sets all fields; With null body → ok; Creates with correct AppointmentId |

#### Application — Reminder Handlers (0 tests, HIGH)

| Handler | Test Scenarios |
|---------|---------------|
| **CreateReminderCommandHandler** | Valid → creates + saves; Appointment not found → exception; Missing customer → ReminderCustomerMissing error |
| **GetReminderQueryHandler** | Found → returns; Not found → throws NotFoundException |

#### Application — Reminder Validator (0 tests, MEDIUM)

| Validator | Test Scenarios |
|-----------|---------------|
| **CreateReminderCommandValidator** | Valid → passes; Empty AppointmentId → fails; Empty Title → fails; Title > 100 chars → fails; ScheduledFor in past → fails |

### Priority 4: Notifications

#### Domain — Notification (0 tests, MEDIUM)

| Function | Max Tests | Scenarios |
|----------|-----------|-----------|
| `Create` | 2 | Valid → sets all fields, IsRead=false; Creates with correct RecipientId |
| `MarkAsRead` | 2 | Unread → sets IsRead=true; Already read → idempotent (no error) |

#### Application — Notification Handlers (0 tests, HIGH)

| Handler | Test Scenarios |
|---------|---------------|
| **GetNotificationQueryHandler** | Found → returns; Not found → throws NotFoundException |
| **GetUserNotificationsQueryHandler** | Has notifications → returns list; Empty → empty list |
| **MarkNotificationReadCommandHandler** | Valid → marks read; Not found → exception |

### Priority 5: Authentication

#### Domain — User (0 tests, CRITICAL)

| Function | Max Tests | Scenarios |
|----------|-----------|-----------|
| `Create` | 3 | Valid → creates with Registered role; Sets CreatedAt; Generates new Guid |
| `LinkIdentity` | 4 | Valid → adds identity; Null → failure; Duplicate provider+key → failure; Multiple identities allowed |
| `PromoteToOwner` | 3 | Valid → adds Owner role; Already owner → failure; Adds to roles collection |
| `BecomeCustomer` | 3 | Valid → adds Customer role; Already customer → failure; Adds to roles collection |
| `AddRole` | 3 | Valid → adds role; Already assigned → failure; Multiple different roles allowed |
| `Activate` | 2 | Deactivated → Active; Already Active → stays Active |
| `Deactivate` | 2 | Active → Deactivated; Already Deactivated → stays Deactivated |
| `MarkAsDeleted` | 1 | Sets status to Deleted |
| `VerifyEmail` | 2 | Unverified → verified; Already verified → stays verified |
| `RecordLogin` | 1 | Sets LastLoginAt |

#### Application — Authentication Command Handlers (0 tests, CRITICAL)

| Handler | Test Scenarios |
|---------|---------------|
| **LoginCommandHandler** | Valid credentials → returns AuthResult; Invalid password → failure; User not found → failure; Locked account → failure |
| **RegisterCommandHandler** | Valid → creates user + credential; Email in use → failure |
| **RefreshTokenCommandHandler** | Valid token → returns AuthResult; Expired → failure; Invalid hash → failure |
| **LogoutCommandHandler** | Valid → revokes; Token not found → exception |
| **ChangePasswordCommandHandler** | Valid → changes + revokes tokens; Wrong current PW → failure; No local credential → failure |

#### Infrastructure — AuthenticationService (0 tests, CRITICAL)

| Function | Max Tests | Key Scenarios |
|----------|-----------|---------------|
| `LocalLoginAsync` | 4 | Valid email+password → AuthResult; Invalid password → InvalidCredentials; No local credential → InvalidCredentials; Records login + creates refresh token |
| `RefreshTokenAsync` | 4 | Valid token → AuthResult; Expired token → InvalidToken; Different hash → InvalidToken; **Reuses same token (no revocation)** — document behavior |
| `RefreshStaffTokenAsync` | 4 | Valid → rotates token; Expired → InvalidToken; Wrong hash → InvalidToken |
| `RegisterAsync` | 4 | Valid → creates user; Email in use → failure; Creates local credential; Links identity |
| `AuthenticateStaffAsync` | 3 | Valid staff → AuthResult; Generates + saves refresh token; Revokes previous active token |
| `SocialLoginAsync` | 3 | Valid identity → AuthResult; Creates refresh token; Records login |
| `ChangePassword` | 3 | Valid → updates + revokes; Wrong password → failure; No credential → failure |
| `UpdatePassword` | 2 | Valid → updates; User not found → failure |
| `LogoutAsync` | 2 | Valid token → revokes; Token not found → exception |

#### Infrastructure — RefreshTokenService (0 tests, CRITICAL)

| Function | Max Tests | Scenarios |
|----------|-----------|-----------|
| `GenerateRefreshToken` | 3 | Returns 64-byte base64 string; Non-empty; Different each call |
| `HashToken` | 4 | Different token → different hash; Same token → consistent hash; Null input → throws; Verifiable with same secret |
| `GetExpiryDate` | 2 | Returns future date; Uses configured expiry days |

#### Infrastructure — JwtService (0 tests, CRITICAL)

| Function | Max Tests | Scenarios |
|----------|-----------|-----------|
| `GenerateToken` | 4 | Returns valid JWT; Contains sub claim; Contains jti claim; Contains custom claims |

---

### Infrastructure Cross-Cutting

#### Repository Integration Tests (MEDIUM priority — consider after unit tests)

| Repository | Key Queries to Test |
|------------|-------------------|
| `RefreshTokenRepository` | Add + GetByTokenAsync; GetActiveTokenByUserIdAsync |
| `StaffRefreshTokenRepository` | Add + GetByTokenAsync; GetActiveStaffTokenByStaffIdAsync |
| `AppointmentsRepository` | OverlapsAsync (date range conflict detection); Add + GetByIdAsync |
| `ReminderRepository` | Add + GetByAppointmentIdAsync |
| `NotificationRepository` | Add + GetByUserIdAsync |

---

## Deliverable 3: Sample Tests

> All tests follow existing project conventions:
> - Namespace: `Viora.Test.Compenents.{Layer}.{Feature}`
> - Naming: `MethodName_Scenario_ExpectedOutcome`
> - AAA strictly separated
> - MSTest + Moq
> - Max 4 tests per function

---

### Domain: AppointmentTests.cs

**Path:** `Viora.Test/Compenents/Domain/Appointments/AppointmentTests.cs`

> Tests the `Book`, `CheckIn`, `Complete`, `Cancel`, and `NoShow` methods of `Appointment`.  
> File must be created at:  
> **`/mnt/d/GradProject/Viora/Viora.Test/Compenents/Domain/Appointments/AppointmentTests.cs`**

```csharp
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;

namespace Viora.Test.Compenents.Domain.Appointments;

[TestClass]
public sealed class AppointmentTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid ServiceId = Guid.NewGuid();
    private static readonly Guid StaffId = Guid.NewGuid();
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly DateTime BaseReservation = new(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);

    // ===== Book =====

    [TestMethod]
    public void Book_ValidInput_SetsAllFields()
    {
        // Arrange
        DateTime createdAt = new(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Wallet, null,
            Creator.Customer, Platform.Web, 30, createdAt);

        // Assert
        Assert.IsNotNull(appointment);
        Assert.AreEqual(CustomerId, appointment.CustomerId);
        Assert.AreEqual(ServiceId, appointment.ServiceId);
        Assert.AreEqual(StaffId, appointment.StaffId);
        Assert.AreEqual(BranchId, appointment.BranchId);
        Assert.AreEqual(BaseReservation, appointment.ReservationDate);
        Assert.AreEqual(1, appointment.AppointmentQueueNumber);
        Assert.AreEqual(PaymentMethod.Wallet, appointment.PayMethod);
        Assert.AreEqual(CustomerStatus.NotArrived, appointment.Status);
        Assert.AreEqual(Creator.Customer, appointment.CreatedBy);
        Assert.AreEqual(Platform.Web, appointment.RequestPlatform);
        Assert.AreEqual(30, appointment.EstimatedDurationMinutes);
        Assert.AreEqual(createdAt, appointment.CreatedAt);
        Assert.IsFalse(appointment.IsCheckedIn);
    }

    [TestMethod]
    public void Book_WithNullCustomerId_AllowsAnonymousBooking()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Appointment appointment = Appointment.Book(
            null, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Staff, Platform.Mobile, 30, createdAt);

        // Assert
        Assert.IsNull(appointment.CustomerId);
    }

    [TestMethod]
    public void Book_RaisesAppointmentBookedEvent()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Online, null,
            Creator.Customer, Platform.Web, 30, createdAt);

        // Assert
        Assert.AreEqual(1, appointment.DomainEvents.Count);
        Assert.IsInstanceOfType(appointment.DomainEvents.Single(), typeof(AppointmentBookedEvent));
    }

    [TestMethod]
    public void Book_CustomerStatusOverride_AppliesProvidedStatus()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash,
            CustomerStatus.Waiting, Creator.Customer, Platform.Web, 30, createdAt);

        // Assert
        Assert.AreEqual(CustomerStatus.Waiting, appointment.Status);
    }

    // ===== CheckIn =====

    [TestMethod]
    public void CheckIn_CustomerWithinWindow_ReturnsSuccessAndUpdatesStatus()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime checkInTime = BaseReservation.AddMinutes(-15);

        // Act
        Result result = appointment.CheckIn(checkInTime, Creator.Customer);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.InProgress, appointment.Status);
        Assert.IsTrue(appointment.IsCheckedIn);
        Assert.AreEqual(checkInTime, appointment.LastUpdatedAt);
    }

    [TestMethod]
    public void CheckIn_CustomerTooEarly_ReturnsFailure()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime checkInTime = BaseReservation.AddMinutes(-31);

        // Act
        Result result = appointment.CheckIn(checkInTime, Creator.Customer);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CheckInNotWithinAcceptableWindow, result.Error);
        Assert.IsFalse(appointment.IsCheckedIn);
    }

    [TestMethod]
    public void CheckIn_CustomerAlreadyCheckedIn_ReturnsFailure()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);

        // Act
        Result result = appointment.CheckIn(BaseReservation.AddMinutes(-10), Creator.Customer);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CheckInProhibited, result.Error);
    }

    [TestMethod]
    public void CheckIn_ByStaff_BypassesWindowCheck()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime checkInTime = BaseReservation.AddMinutes(-60); // far outside 30min window

        // Act
        Result result = appointment.CheckIn(checkInTime, Creator.Staff);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.InProgress, appointment.Status);
        Assert.IsTrue(appointment.IsCheckedIn);
    }

    // ===== Complete =====

    [TestMethod]
    public void Complete_ValidInProgress_ReturnsSuccess()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);
        DateTime completeTime = BaseReservation.AddMinutes(25);

        // Act
        Result result = appointment.Complete(completeTime);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.Completed, appointment.Status);
        Assert.AreEqual(completeTime, appointment.LastUpdatedAt);
    }

    [TestMethod]
    public void Complete_WhenAlreadyCompleted_ReturnsFailure()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);
        appointment.Complete(BaseReservation.AddMinutes(25));

        // Act
        Result result = appointment.Complete(BaseReservation.AddMinutes(30));

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CompleteProhibited, result.Error);
    }

    [TestMethod]
    public void Complete_TooEarly_ReturnsFailure()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);
        DateTime tooEarly = BaseReservation.AddHours(-2);

        // Act
        Result result = appointment.Complete(tooEarly);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CompleteProhibited, result.Error);
    }

    [TestMethod]
    public void Complete_RaisesAppointmentCompletedEvent()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);
        appointment.ClearDomainEvents();
        DateTime completeTime = BaseReservation.AddMinutes(25);

        // Act
        appointment.Complete(completeTime);

        // Assert
        Assert.AreEqual(1, appointment.DomainEvents.Count);
        Assert.IsInstanceOfType(appointment.DomainEvents.Single(), typeof(AppointmentCompletedEvent));
    }

    // ===== Cancel =====

    [TestMethod]
    public void Cancel_ByCustomerOutsideWindow_ReturnsSuccess()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime cancelTime = BaseReservation.AddHours(-3);

        // Act
        Result result = appointment.Cancel(cancelTime, BranchId, Creator.Customer);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.Canceled, appointment.Status);
        Assert.AreEqual(cancelTime, appointment.LastUpdatedAt);
    }

    [TestMethod]
    public void Cancel_ByCustomerInsideTwoHourWindow_ReturnsFailure()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime cancelTime = BaseReservation.AddHours(-1);

        // Act
        Result result = appointment.Cancel(cancelTime, BranchId, Creator.Customer);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CancellationProhibited, result.Error);
    }

    [TestMethod]
    public void Cancel_ByStaff_AlwaysAllowed()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime cancelTime = BaseReservation.AddMinutes(-30); // inside 2h window

        // Act
        Result result = appointment.Cancel(cancelTime, BranchId, Creator.Staff);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.Canceled, appointment.Status);
    }

    [TestMethod]
    public void Cancel_WhenCompleted_ReturnsFailure()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);
        appointment.Complete(BaseReservation.AddMinutes(25));

        // Act
        Result result = appointment.Cancel(DateTime.UtcNow, BranchId, Creator.Staff);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CancellationProhibited, result.Error);
    }

    // ===== NoShow =====

    [TestMethod]
    public void NoShow_NotArrivedAfterReservation_ReturnsSuccess()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        DateTime noShowTime = BaseReservation.AddHours(1);

        // Act
        Result result = appointment.NoShow(noShowTime);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.NoShow, appointment.Status);
        Assert.AreEqual(noShowTime, appointment.LastUpdatedAt);
    }

    [TestMethod]
    public void NoShow_WhenAlreadyCheckedIn_ReturnsFailure()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.CheckIn(BaseReservation.AddMinutes(-15), Creator.Customer);

        // Act
        Result result = appointment.NoShow(BaseReservation.AddHours(1));

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.NoShowProhibited, result.Error);
    }

    [TestMethod]
    public void NoShow_TimeBeforeReservation_ReturnsFailure()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);

        // Act
        Result result = appointment.NoShow(BaseReservation.AddHours(-1));

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.NoShowTimeInvalid, result.Error);
    }

    [TestMethod]
    public void NoShow_RaisesAppointmentNoShowEvent()
    {
        // Arrange
        Appointment appointment = Appointment.Book(
            CustomerId, ServiceId, StaffId, BranchId, null,
            BaseReservation, 1, PaymentMethod.Cash, null,
            Creator.Customer, Platform.Web, 30, DateTime.UtcNow);
        appointment.ClearDomainEvents();

        // Act
        appointment.NoShow(BaseReservation.AddHours(1));

        // Assert
        Assert.AreEqual(1, appointment.DomainEvents.Count);
        Assert.IsInstanceOfType(appointment.DomainEvents.Single(), typeof(AppointmentNoShowEvent));
    }
}
```

---

### Domain: UserTests.cs

**Path:** `Viora.Test/Compenents/Domain/Users/UserTests.cs`

> Tests the `User` entity — `Create`, `LinkIdentity`, `PromoteToOwner`, `BecomeCustomer`, `AddRole`.  
> File must be created at:  
> **`/mnt/d/GradProject/Viora/Viora.Test/Compenents/Domain/Users/UserTests.cs`**

```csharp
using Viora.Domain.Abstractions;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Test.Compenents.Domain.Users;

[TestClass]
public sealed class UserTests
{
    private static readonly PersonalInfo PersonalInfo = new(
        "John", "Doe", new DateOnly(1990, 1, 1), null, null);

    private static readonly Email Email = "john@example.com";

    // ===== Create =====

    [TestMethod]
    public void Create_ValidInput_SetsAllFieldsAndRegisteredRole()
    {
        // Arrange
        DateTime utcNow = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        User user = User.Create(PersonalInfo, Email, utcNow);

        // Assert
        Assert.IsNotNull(user);
        Assert.AreNotEqual(Guid.Empty, user.Id);
        Assert.AreEqual(Email, user.Email);
        Assert.AreEqual(utcNow, user.CreatedAt);
        Assert.AreEqual(AccountStatus.Active, user.Status);
        Assert.IsFalse(user.IsEmailVerified);
        Assert.IsNull(user.LastLoginAt);
        Assert.AreEqual(1, user.Roles.Count);
        Assert.AreSame(Role.Registered, user.Roles.Single());
    }

    [TestMethod]
    public void Create_DifferentCalls_GenerateDifferentIds()
    {
        // Arrange
        DateTime utcNow = DateTime.UtcNow;

        // Act
        User user1 = User.Create(PersonalInfo, Email, utcNow);
        User user2 = User.Create(PersonalInfo, Email, utcNow);

        // Assert
        Assert.AreNotEqual(user1.Id, user2.Id);
    }

    // ===== LinkIdentity =====

    [TestMethod]
    public void LinkIdentity_ValidIdentity_AddsToCollection()
    {
        // Arrange
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        AuthIdentity identity = AuthIdentity.Create("google", Guid.NewGuid(), "google-id-123", DateTime.UtcNow);

        // Act
        Result result = user.LinkIdentity(identity);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, user.Identities.Count);
        Assert.AreSame(identity, user.Identities.Single());
    }

    [TestMethod]
    public void LinkIdentity_NullIdentity_ReturnsFailure()
    {
        // Arrange
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        // Act
        Result result = user.LinkIdentity(null!);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.EmptyField, result.Error);
        Assert.AreEqual(0, user.Identities.Count);
    }

    [TestMethod]
    public void LinkIdentity_DuplicateProviderAndKey_ReturnsFailure()
    {
        // Arrange
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        AuthIdentity identity = AuthIdentity.Create("google", Guid.NewGuid(), "google-id-123", DateTime.UtcNow);
        user.LinkIdentity(identity);

        // Act
        AuthIdentity duplicate = AuthIdentity.Create("google", Guid.NewGuid(), "google-id-123", DateTime.UtcNow);
        Result result = user.LinkIdentity(duplicate);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.IdentityLinked, result.Error);
        Assert.AreEqual(1, user.Identities.Count);
    }

    [TestMethod]
    public void LinkIdentity_DifferentProviders_BothAllowed()
    {
        // Arrange
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        AuthIdentity google = AuthIdentity.Create("google", Guid.NewGuid(), "google-id", DateTime.UtcNow);
        AuthIdentity facebook = AuthIdentity.Create("facebook", Guid.NewGuid(), "fb-id", DateTime.UtcNow);

        // Act
        user.LinkIdentity(google);
        user.LinkIdentity(facebook);

        // Assert
        Assert.AreEqual(2, user.Identities.Count);
    }

    // ===== PromoteToOwner =====

    [TestMethod]
    public void PromoteToOwner_NotYetOwner_AddsRoleAndReturnsSuccess()
    {
        // Arrange
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        // Act
        Result result = user.PromoteToOwner(Role.Owner);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(user.Roles.Any(r => r.Id == Role.Owner.Id));
    }

    [TestMethod]
    public void PromoteToOwner_WhenAlreadyOwner_ReturnsFailure()
    {
        // Arrange
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        user.PromoteToOwner(Role.Owner);

        // Act
        Result result = user.PromoteToOwner(Role.Owner);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.AlreadyOwner, result.Error);
    }

    [TestMethod]
    public void PromoteToOwner_KeepsExistingRoles()
    {
        // Arrange
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        // Act
        user.PromoteToOwner(Role.Owner);

        // Assert
        Assert.AreEqual(2, user.Roles.Count);
        Assert.IsTrue(user.Roles.Any(r => r.Id == Role.Registered.Id));
        Assert.IsTrue(user.Roles.Any(r => r.Id == Role.Owner.Id));
    }

    // ===== BecomeCustomer =====

    [TestMethod]
    public void BecomeCustomer_NotYetCustomer_AddsRoleAndReturnsSuccess()
    {
        // Arrange
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);

        // Act
        Result result = user.BecomeCustomer(Role.Customer);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(user.Roles.Any(r => r.Id == Role.Customer.Id));
    }

    [TestMethod]
    public void BecomeCustomer_WhenAlreadyCustomer_ReturnsFailure()
    {
        // Arrange
        User user = User.Create(PersonalInfo, Email, DateTime.UtcNow);
        user.BecomeCustomer(Role.Customer);

        // Act
        Result result = user.BecomeCustomer(Role.Customer);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.AlreadyCustomer, result.Error);
    }
}
```

---

### Application: CancelAppointmentCommandHandlerTests.cs

**Path:** `Viora.Test/Compenents/Application/Appointments/CancelAppointmentCommandHandlerTests.cs`

> Tests the `CancelAppointmentCommandHandler` with mocked dependencies.  
> File must be created at:  
> **`/mnt/d/GradProject/Viora/Viora.Test/Compenents/Application/Appointments/CancelAppointmentCommandHandlerTests.cs`**

```csharp
using Moq;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Appointments.CancelAppointment;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Appointments.Internal;

namespace Viora.Test.Compenents.Application.Appointments;

[TestClass]
public sealed class CancelAppointmentCommandHandlerTests
{
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IAppointmentsRepository> _appointmentRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();

    private readonly DateTime _fixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);
    private readonly CancelAppointmentCommandHandler _handler;

    public CancelAppointmentCommandHandlerTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(_fixedNow);
        _userContextMock.Setup(c => c.OrganizationId).Returns(BranchId);

        _handler = new CancelAppointmentCommandHandler(
            _appointmentRepoMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            _userContextMock.Object);
    }

    private static Appointment CreateTestAppointment(
        Guid? customerId = null,
        CustomerStatus status = CustomerStatus.NotArrived,
        DateTime? reservationDate = null)
    {
        var resDate = reservationDate ?? new DateTime(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);
        return Appointment.Book(
            customerId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            BranchId,
            null,
            resDate,
            1,
            PaymentMethod.Cash,
            status == CustomerStatus.NotArrived ? null : status,
            Creator.Customer,
            Platform.Web,
            30,
            new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public async Task Handle_ValidAppointment_ReturnsSuccessAndCancels()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        var command = new CancelAppointmentCommand(appointment.Id);

        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(CustomerStatus.Canceled, appointment.Status);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_AppointmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var command = new CancelAppointmentCommand(appointmentId);

        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_CompletedAppointment_ReturnsFailure()
    {
        // Arrange
        var appointment = CreateTestAppointment(status: CustomerStatus.Completed);
        var command = new CancelAppointmentCommand(appointment.Id);

        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(AppointmentErrors.CancellationProhibited, result.Error);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_DifferentOrganization_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        var command = new CancelAppointmentCommand(appointment.Id);

        // Simulate a different organization context
        _userContextMock.Setup(c => c.OrganizationId).Returns(Guid.NewGuid());

        _appointmentRepoMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
```

---

### Infrastructure: RefreshTokenServiceTests.cs

**Path:** `Viora.Test/Compenents/Infrastructure/Authentication/RefreshTokenServiceTests.cs`

> Tests the `RefreshTokenService` — generation, hashing, and expiry.  
> `RefreshTokenService` is `internal` so tests must use `InternalsVisibleTo` or reflection.  
> Add to `Viora.Infrastructure.csproj`:
> ```xml
> <ItemGroup>
>     <InternalsVisibleTo Include="Viora.Test" />
> </ItemGroup>
> ```
> File must be created at:  
> **`/mnt/d/GradProject/Viora/Viora.Test/Compenents/Infrastructure/Authentication/RefreshTokenServiceTests.cs`**

```csharp
using Microsoft.Extensions.Configuration;
using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Infrastructure.Authentication;

namespace Viora.Test.Compenents.Infrastructure.Authentication;

[TestClass]
public sealed class RefreshTokenServiceTests
{
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly RefreshTokenService _service;

    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public RefreshTokenServiceTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(FixedNow);

        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(s => s.Value).Returns("7");

        _configMock
            .Setup(c => c.GetSection("RefreshToken:Expiry_Days"))
            .Returns(configSectionMock.Object);
        _configMock
            .Setup(c => c.GetValue<int>("RefreshToken:Expiry_Days"))
            .Returns(7);
        _configMock
            .Setup(c => c.GetValue<string>("RefreshToken:Secret"))
            .Returns("test-secret-key-32-chars-long!!");

        _service = new RefreshTokenService(_configMock.Object, _clockMock.Object);
    }

    // ===== GenerateRefreshToken =====

    [TestMethod]
    public void GenerateRefreshToken_ReturnsNonEmptyBase64String()
    {
        // Act
        string token = _service.GenerateRefreshToken();

        // Assert
        Assert.IsNotNull(token);
        Assert.IsTrue(token.Length > 0);
        // 64 random bytes → base64 = 88 characters (with padding)
        Assert.AreEqual(88, token.Length);
    }

    [TestMethod]
    public void GenerateRefreshToken_ConsecutiveCalls_ReturnsDifferentTokens()
    {
        // Act
        string token1 = _service.GenerateRefreshToken();
        string token2 = _service.GenerateRefreshToken();

        // Assert
        Assert.AreNotEqual(token1, token2);
    }

    [TestMethod]
    public void GenerateRefreshToken_CanBeDecodedTo64Bytes()
    {
        // Act
        string token = _service.GenerateRefreshToken();
        byte[] decoded = Convert.FromBase64String(token);

        // Assert
        Assert.AreEqual(64, decoded.Length);
    }

    // ===== HashToken =====

    [TestMethod]
    public void HashToken_SameToken_ReturnsSameHash()
    {
        // Arrange
        string token = _service.GenerateRefreshToken();

        // Act
        string hash1 = _service.HashToken(token);
        string hash2 = _service.HashToken(token);

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void HashToken_DifferentTokens_ReturnsDifferentHashes()
    {
        // Arrange
        string token1 = _service.GenerateRefreshToken();
        string token2 = _service.GenerateRefreshToken();

        // Act
        string hash1 = _service.HashToken(token1);
        string hash2 = _service.HashToken(token2);

        // Assert
        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void HashToken_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => _service.HashToken(null!));
    }

    [TestMethod]
    public void HashToken_ReturnsBase64String()
    {
        // Arrange
        string token = _service.GenerateRefreshToken();

        // Act
        string hash = _service.HashToken(token);

        // Assert
        Assert.IsNotNull(hash);
        // HMACSHA256 output is 32 bytes → base64 = 44 characters (with padding)
        Assert.AreEqual(44, hash.Length);
    }

    // ===== GetExpiryDate =====

    [TestMethod]
    public void GetExpiryDate_ReturnsDateInFuture()
    {
        // Act
        DateTime expiry = _service.GetExpiryDate();

        // Assert
        Assert.IsTrue(expiry > FixedNow);
    }

    [TestMethod]
    public void GetExpiryDate_UsesConfiguredExpiryDays()
    {
        // Act
        DateTime expiry = _service.GetExpiryDate();

        // Assert
        Assert.AreEqual(FixedNow.AddDays(7), expiry);
    }
}
```

---

### Infrastructure: AuthenticationServiceTests.cs

**Path:** `Viora.Test/Compenents/Infrastructure/Authentication/AuthenticationServiceTests.cs`

> Tests `AuthenticationService.LocalLoginAsync` — the most complex authentication flow.  
> File must be created at:  
> **`/mnt/d/GradProject/Viora/Viora.Test/Compenents/Infrastructure/Authentication/AuthenticationServiceTests.cs`**

```csharp
using Moq;
using System.Security.Claims;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;
using Viora.Infrastructure.Authentication;
using Viora.Infrastructure.Repositories.Authentication;

namespace Viora.Test.Compenents.Infrastructure.Authentication;

[TestClass]
public sealed class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IHasher> _hasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly Mock<IIdentityRepository> _identityRepoMock = new();
    private readonly Mock<IOrganizationRepository> _orgRepoMock = new();
    private readonly Mock<RefreshTokenService> _refreshTokenServiceMock = null!;
    private readonly Mock<LocalCredentialRepository> _localCredRepoMock = new();
    private readonly Mock<RefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<StaffRefreshTokenRepository> _staffRefreshTokenRepoMock = new();
    private readonly Mock<ApplicationDbContext> _dbContextMock = new();

    private readonly AuthenticationService _service;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime FixedNow = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    public AuthenticationServiceTests()
    {
        _clockMock.Setup(c => c.UtcNow).Returns(FixedNow);

        // Build a real RefreshTokenService for the mock constructor
        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(c => c.GetValue<int>("RefreshToken:Expiry_Days")).Returns(7);
        configMock.Setup(c => c.GetValue<string>("RefreshToken:Secret"))
            .Returns("test-secret-key-32-chars-long!!");

        _refreshTokenServiceMock = new Mock<RefreshTokenService>(configMock.Object, _clockMock.Object)
        { CallBase = true };

        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<IEnumerable<Claim>>()))
            .Returns("fake-jwt-token");

        _service = new AuthenticationService(
            _userRepoMock.Object,
            _jwtServiceMock.Object,
            _hasherMock.Object,
            _unitOfWorkMock.Object,
            _clockMock.Object,
            _identityRepoMock.Object,
            _orgRepoMock.Object,
            _refreshTokenServiceMock.Object,
            _localCredRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _staffRefreshTokenRepoMock.Object,
            _dbContextMock.Object);
    }

    private static User CreateTestUser()
    {
        var personalInfo = new PersonalInfo("John", "Doe", new DateOnly(1990, 1, 1), null, null);
        var email = new Email("john@example.com");
        return User.Create(personalInfo, email, FixedNow);
    }

    // ===== LocalLoginAsync =====

    [TestMethod]
    public async Task LocalLoginAsync_ValidCredentials_ReturnsAuthResult()
    {
        // Arrange
        var user = CreateTestUser();
        var localCredential = new LocalCredential(user.Id, "hashed-password");

        _userRepoMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _localCredRepoMock
            .Setup(r => r.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(localCredential);

        _hasherMock.Setup(h => h.Verify("correct-password", "hashed-password")).Returns(true);

        _jwtServiceMock
            .Setup(j => j.GenerateToken(user.Id, It.IsAny<IEnumerable<Claim>>()))
            .Returns("access-token-value");

        _refreshTokenServiceMock
            .Setup(r => r.GenerateRefreshToken())
            .Returns("raw-refresh-token");

        _refreshTokenServiceMock
            .Setup(r => r.HashToken("raw-refresh-token"))
            .Returns("hashed-refresh-token");

        _refreshTokenServiceMock
            .Setup(r => r.GetExpiryDate())
            .Returns(FixedNow.AddDays(7));

        // Act
        Result<AuthResult> result = await _service.LocalLoginAsync(
            "john@example.com", "correct-password", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(user.Id, result.Value.UserId);
        Assert.AreEqual("access-token-value", result.Value.AccessToken);
        Assert.AreEqual("raw-refresh-token", result.Value.RefreshToken);
        _refreshTokenRepoMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task LocalLoginAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("unknown@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Result<AuthResult> result = await _service.LocalLoginAsync(
            "unknown@example.com", "any-password", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.InvalidCredentials, result.Error);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task LocalLoginAsync_NoLocalCredential_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();

        _userRepoMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _localCredRepoMock
            .Setup(r => r.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LocalCredential?)null);

        // Act
        Result<AuthResult> result = await _service.LocalLoginAsync(
            "john@example.com", "any-password", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.InvalidCredentials, result.Error);
    }

    [TestMethod]
    public async Task LocalLoginAsync_WrongPassword_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestUser();
        var localCredential = new LocalCredential(user.Id, "hashed-password");

        _userRepoMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _localCredRepoMock
            .Setup(r => r.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(localCredential);

        _hasherMock.Setup(h => h.Verify("wrong-password", "hashed-password")).Returns(false);

        // Act
        Result<AuthResult> result = await _service.LocalLoginAsync(
            "john@example.com", "wrong-password", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(UserErrors.InvalidCredentials, result.Error);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // failed login recorded
    }
}
```

---

## Appendix: Test File Creation Plan

| # | File Path | Tests | Priority |
|---|-----------|-------|----------|
| 1 | `Viora.Test/Compenents/Domain/Appointments/AppointmentTests.cs` | ~28 | CRITICAL |
| 2 | `Viora.Test/Compenents/Domain/Users/UserTests.cs` | ~16 | CRITICAL |
| 3 | `Viora.Test/Compenents/Domain/Appointments/AppointmentDelayTests.cs` | ~8 | HIGH |
| 4 | `Viora.Test/Compenents/Domain/Notifications/NotificationTests.cs` | ~6 | MEDIUM |
| 5 | `Viora.Test/Compenents/Domain/Reminders/ReminderTests.cs` | ~4 | MEDIUM |
| 6 | `Viora.Test/Compenents/Domain/Staffs/StaffRolesTests.cs` | ~6 | MEDIUM |
| 7 | `Viora.Test/Compenents/Application/Staffs/RegisterStaffCommandHandlerTests.cs` | ~8 | CRITICAL |
| 8 | `Viora.Test/Compenents/Application/Staffs/DeleteStaffCommandHandlerTests.cs` | ~6 | CRITICAL |
| 9 | `Viora.Test/Compenents/Application/Appointments/CancelAppointmentCommandHandlerTests.cs` | ~8 | CRITICAL |
| 10 | `Viora.Test/Compenents/Application/Appointments/CreateAppointmentCommandHandlerTests.cs` | ~8 | CRITICAL |
| 11 | `Viora.Test/Compenents/Application/Reminders/CreateReminderCommandHandlerTests.cs` | ~6 | HIGH |
| 12 | `Viora.Test/Compenents/Application/Notifications/GetNotificationQueryHandlerTests.cs` | ~4 | HIGH |
| 13 | `Viora.Test/Compenents/Application/Notifications/MarkNotificationReadCommandHandlerTests.cs` | ~4 | HIGH |
| 14 | `Viora.Test/Compenents/Infrastructure/Authentication/AuthenticationServiceTests.cs` | ~16 | CRITICAL |
| 15 | `Viora.Test/Compenents/Infrastructure/Authentication/RefreshTokenServiceTests.cs` | ~10 | CRITICAL |
| 16 | `Viora.Test/Compenents/Infrastructure/Authentication/JwtServiceTests.cs` | ~6 | CRITICAL |
| 17 | `Viora.Test/Compenents/Api/Controllers/AuthControllerTests.cs` | ~8 | HIGH |
| 18 | `Viora.Test/Compenents/Api/Middleware/GlobalExceptionMiddlewareTests.cs` | ~4 | HIGH |
| 19 | `Viora.Test/Compenents/Api/Extensions/ActionResultMapperTests.cs` | ~6 | HIGH |

**Total: ~158 new tests across all priorities**

---

## Appendix: Infrastructure.csproj Changes Required

To test `internal` infrastructure classes, add to `Viora.Infrastructure.csproj`:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="Viora.Test" />
</ItemGroup>
```

Similarly, for `Viora.Application.csproj`:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="Viora.Test" />
</ItemGroup>
```

Also update `Viora.Test.csproj` to add project references:

```xml
<ItemGroup>
  <ProjectReference Include="..\Viora.Application\Viora.Application.csproj" />
  <ProjectReference Include="..\Viora.Infrastructure\Viora.Infrastructure.csproj" />
</ItemGroup>
```
