using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using VMWorkflow.API.Middleware;
using VMWorkflow.Application.Interfaces;
using VMWorkflow.Application.Services;
using VMWorkflow.Domain.Interfaces;
using VMWorkflow.Infrastructure.Data;
using VMWorkflow.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog ---
builder.Host.UseSerilog((ctx, config) =>
{
    config
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: Path.Combine(ctx.Configuration["Logging:LogPath"] ?? "C:\\Logs\\VMWorkflow", "log-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
});

// --- EF Core ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not configured.");
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<WorkflowDbContext>(options =>
    options.UseMySql(connectionString, serverVersion,
        b => b.MigrationsAssembly("VMWorkflow.Infrastructure")));

// --- Application Services ---
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddSingleton<IWorkflowEngine, WorkflowEngine>();
builder.Services.AddSingleton<IScriptGenerationService, FortiGateScriptGenerator>();

// --- Infrastructure Stubs ---
builder.Services.AddScoped<INetBoxService, StubNetBoxService>();
builder.Services.AddScoped<IFortiGateService, StubFortiGateService>();

// --- LDAP / Active Directory ---
builder.Services.AddSingleton<ILdapAuthService, LdapAuthService>();

// --- JWT Authentication ---
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? jwtSection["Key"]
    ?? throw new InvalidOperationException("JWT key not configured. Set JWT_SECRET_KEY environment variable.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// --- Controllers + Swagger ---
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ICTD Workflow API",
        Version = "v1",
        Description = "ICTD Provisioning & Security Workflow Platform"
    });
});

// --- CORS ---
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5028", "https://localhost:5028", "http://localhost:5186", "https://localhost:5186", "http://localhost:5173", "https://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// --- Ensure DB Created + Seed ---
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
    db.Database.EnsureCreated();

    // Ensure Scripts table exists (EnsureCreated won't add new tables to an existing DB)
    await db.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS `Scripts` (
            `ScriptId` char(36) NOT NULL COLLATE ascii_general_ci,
            `RequestId` char(36) NOT NULL COLLATE ascii_general_ci,
            `ScriptType` varchar(50) NOT NULL,
            `Content` text NOT NULL,
            `FileName` varchar(250) NOT NULL,
            `GeneratedBy` varchar(100) NOT NULL,
            `GeneratedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`ScriptId`),
            KEY `IX_Scripts_RequestId` (`RequestId`),
            KEY `IX_Scripts_GeneratedAt` (`GeneratedAt`),
            CONSTRAINT `FK_Scripts_Requests_RequestId` FOREIGN KEY (`RequestId`) REFERENCES `Requests` (`RequestId`) ON DELETE CASCADE
        ) CHARACTER SET utf8mb4;");

    // Ensure Schedules table exists (EnsureCreated won't add new tables to an existing DB)
    await db.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS `Schedules` (
            `ScheduleId` char(36) NOT NULL COLLATE ascii_general_ci,
            `Name` varchar(50) NOT NULL,
            `Type` int NOT NULL,
            `Color` varchar(9) NULL,
            `StartAt` datetime(6) NULL,
            `EndAt` datetime(6) NULL,
            `RecurrenceDays` varchar(32) NULL,
            `PreExpirationEventLog` tinyint(1) NOT NULL,
            `NumberOfDaysBefore` int NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `CreatedBy` varchar(64) NOT NULL,
            PRIMARY KEY (`ScheduleId`),
            UNIQUE KEY `IX_Schedules_Name` (`Name`)
        ) CHARACTER SET utf8mb4;");

    // Right-size VARCHAR columns on existing databases (idempotent; no-op if already sized).
    // Drops dead column NOCDetails.FQDN if still present.
    await ResizeSchemaAsync(db);

    await db.SeedDefaultUsersAsync();
    await db.SeedDefaultDropdownOptionsAsync();
    await db.SeedDefaultAdminDataAsync();
}

// --- Security Headers ---
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }
    await next();
});

// --- Middleware Pipeline ---
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionHandler>();
app.UseMiddleware<AuditLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ICTD Workflow API v1"));
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors("AppCors");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserBlockingMiddleware>();
app.MapControllers();
app.Map("/api/{**slug}", (HttpContext ctx) =>
{
    ctx.Response.ContentType = "application/json";
    ctx.Response.StatusCode = 404;
    return ctx.Response.WriteAsync("{\"error\":\"API endpoint not found.\"}");
});
app.MapFallbackToFile("index.html");

await app.RunAsync();

static async Task ResizeSchemaAsync(WorkflowDbContext db)
{
    var statements = new[]
    {
        // Drop dead column on existing schema (NOC FQDN moved to Request.FQDNSuggestion / SysAdmin)
        "ALTER TABLE `NOCDetails` DROP COLUMN `FQDN`",

        // Identity/username columns → varchar(64)
        "ALTER TABLE `Users` MODIFY COLUMN `Username` varchar(64) NOT NULL",
        "ALTER TABLE `Users` MODIFY COLUMN `DisplayName` varchar(100) NOT NULL",
        "ALTER TABLE `Users` MODIFY COLUMN `Email` varchar(254) NOT NULL",
        "ALTER TABLE `Users` MODIFY COLUMN `Role` varchar(30) NOT NULL",
        "ALTER TABLE `Users` MODIFY COLUMN `PasswordHash` varchar(100) NOT NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `CreatedBy` varchar(64) NOT NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `CisoApprovedBy` varchar(64) NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `OpsApprovedBy` varchar(64) NULL",
        "ALTER TABLE `SysAdminDetails` MODIFY COLUMN `SubmittedBy` varchar(64) NOT NULL",
        "ALTER TABLE `DataCenterDetails` MODIFY COLUMN `SubmittedBy` varchar(64) NOT NULL",
        "ALTER TABLE `NOCDetails` MODIFY COLUMN `SubmittedBy` varchar(64) NOT NULL",
        "ALTER TABLE `SOCDetails` MODIFY COLUMN `SubmittedBy` varchar(64) NOT NULL",
        "ALTER TABLE `StatusHistories` MODIFY COLUMN `ChangedBy` varchar(64) NOT NULL",
        "ALTER TABLE `AuditLogs` MODIFY COLUMN `User` varchar(64) NOT NULL",
        "ALTER TABLE `Scripts` MODIFY COLUMN `GeneratedBy` varchar(64) NOT NULL",
        "ALTER TABLE `SecurityProfiles` MODIFY COLUMN `CreatedBy` varchar(64) NULL",
        "ALTER TABLE `Vdoms` MODIFY COLUMN `CreatedBy` varchar(64) NULL",
        "ALTER TABLE `ResourceGroups` MODIFY COLUMN `CreatedBy` varchar(64) NULL",
        "ALTER TABLE `DropdownOptions` MODIFY COLUMN `CreatedBy` varchar(64) NULL",

        // Request — status enums and descriptors
        "ALTER TABLE `Requests` MODIFY COLUMN `ApplicationName` varchar(100) NOT NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `ObjectSlug` varchar(100) NOT NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `Environment` varchar(20) NOT NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `Status` varchar(30) NOT NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `ExternalSyncStatus` varchar(20) NOT NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `SLA` varchar(20) NOT NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `ProgrammingLanguage` varchar(50) NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `Framework` varchar(100) NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `DBMS` varchar(50) NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `AccessGroup` varchar(100) NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `FQDNSuggestion` varchar(253) NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `AuthenticationMethod` varchar(50) NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `CisoDecision` varchar(10) NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `OpsDecision` varchar(10) NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `NetBoxId` varchar(50) NULL",
        "ALTER TABLE `Requests` MODIFY COLUMN `FortiGatePolicyId` varchar(50) NULL",

        // Status history enums
        "ALTER TABLE `StatusHistories` MODIFY COLUMN `OldStatus` varchar(30) NOT NULL",
        "ALTER TABLE `StatusHistories` MODIFY COLUMN `NewStatus` varchar(30) NOT NULL",

        // Schedules
        "ALTER TABLE `Schedules` MODIFY COLUMN `Name` varchar(50) NOT NULL",
        "ALTER TABLE `Schedules` MODIFY COLUMN `Color` varchar(9) NULL",
        "ALTER TABLE `Schedules` MODIFY COLUMN `RecurrenceDays` varchar(32) NULL",

        // SysAdmin
        "ALTER TABLE `SysAdminDetails` MODIFY COLUMN `SensitivityLevel` varchar(30) NOT NULL",
        "ALTER TABLE `SysAdminDetails` MODIFY COLUMN `DatabaseName` varchar(64) NULL",
        "ALTER TABLE `SysAdminDetails` MODIFY COLUMN `DatabaseUsername` varchar(64) NULL",
        "ALTER TABLE `SysAdminDetails` MODIFY COLUMN `Hostname` varchar(64) NOT NULL",

        // DataCenter
        "ALTER TABLE `DataCenterDetails` MODIFY COLUMN `Environment` varchar(30) NOT NULL",
        "ALTER TABLE `DataCenterDetails` MODIFY COLUMN `UplinkSpeed` varchar(20) NOT NULL",
        "ALTER TABLE `DataCenterDetails` MODIFY COLUMN `BareMetalType` varchar(30) NOT NULL",
        "ALTER TABLE `DataCenterDetails` MODIFY COLUMN `PortNumber` varchar(15) NOT NULL",
        "ALTER TABLE `DataCenterDetails` MODIFY COLUMN `DC` varchar(50) NOT NULL",
        "ALTER TABLE `DataCenterDetails` MODIFY COLUMN `RackRoom` varchar(50) NOT NULL",
        "ALTER TABLE `DataCenterDetails` MODIFY COLUMN `RackNumber` varchar(20) NOT NULL",

        // NOC — network fields
        "ALTER TABLE `NOCDetails` MODIFY COLUMN `IPAddress` varchar(45) NOT NULL",
        "ALTER TABLE `NOCDetails` MODIFY COLUMN `SubnetMask` varchar(15) NOT NULL",
        "ALTER TABLE `NOCDetails` MODIFY COLUMN `VLANID` varchar(10) NOT NULL",
        "ALTER TABLE `NOCDetails` MODIFY COLUMN `Gateway` varchar(45) NOT NULL",
        "ALTER TABLE `NOCDetails` MODIFY COLUMN `Port` varchar(15) NOT NULL",
        "ALTER TABLE `NOCDetails` MODIFY COLUMN `VIP` varchar(45) NULL",
        "ALTER TABLE `NOCDetails` MODIFY COLUMN `VirtualIP` varchar(45) NULL",
        "ALTER TABLE `NOCDetails` MODIFY COLUMN `VirtualPort` varchar(15) NULL",
        "ALTER TABLE `NOCDetails` MODIFY COLUMN `VirtualFQDN` varchar(253) NULL",

        // Firewall entries
        "ALTER TABLE `FirewallEntries` MODIFY COLUMN `PolicyName` varchar(100) NOT NULL",
        "ALTER TABLE `FirewallEntries` MODIFY COLUMN `VDOM` varchar(50) NOT NULL",
        "ALTER TABLE `FirewallEntries` MODIFY COLUMN `SourceInterface` varchar(50) NULL",
        "ALTER TABLE `FirewallEntries` MODIFY COLUMN `DestinationInterface` varchar(50) NULL",
        "ALTER TABLE `FirewallEntries` MODIFY COLUMN `SourceIP` varchar(50) NULL",
        "ALTER TABLE `FirewallEntries` MODIFY COLUMN `DestinationIP` varchar(50) NULL",
        "ALTER TABLE `FirewallEntries` MODIFY COLUMN `Schedule` varchar(50) NULL",
        "ALTER TABLE `FirewallEntries` MODIFY COLUMN `Action` varchar(10) NOT NULL",

        // Service entries (FortiGate port/protocol)
        "ALTER TABLE `ServiceEntry` MODIFY COLUMN `ServiceName` varchar(50) NOT NULL",
        "ALTER TABLE `ServiceEntry` MODIFY COLUMN `Port` varchar(5) NOT NULL",
        "ALTER TABLE `ServiceEntry` MODIFY COLUMN `Protocol` varchar(10) NOT NULL",

        // Network path
        "ALTER TABLE `NetworkPathEntries` MODIFY COLUMN `SwitchName` varchar(100) NOT NULL",
        "ALTER TABLE `NetworkPathEntries` MODIFY COLUMN `Port` varchar(50) NOT NULL",
        "ALTER TABLE `NetworkPathEntries` MODIFY COLUMN `LinkSpeed` varchar(20) NULL",

        // Lookup tables
        "ALTER TABLE `SecurityProfiles` MODIFY COLUMN `Name` varchar(50) NOT NULL",
        "ALTER TABLE `Vdoms` MODIFY COLUMN `Name` varchar(50) NOT NULL",
        "ALTER TABLE `DropdownOptions` MODIFY COLUMN `Category` varchar(30) NOT NULL",
        "ALTER TABLE `DropdownOptions` MODIFY COLUMN `Value` varchar(50) NOT NULL",

        // Logs / scripts
        "ALTER TABLE `AutomationLogs` MODIFY COLUMN `Action` varchar(100) NOT NULL",
        "ALTER TABLE `Scripts` MODIFY COLUMN `ScriptType` varchar(20) NOT NULL",
        "ALTER TABLE `Scripts` MODIFY COLUMN `FileName` varchar(255) NOT NULL",
    };

    foreach (var sql in statements)
    {
        try { await db.Database.ExecuteSqlRawAsync(sql); }
        catch (Exception ex)
        {
            // Column may already be dropped, table may not exist yet, or value exceeds new size —
            // log and continue so a one-off failure doesn't block startup.
            Console.WriteLine($"[schema-resize] skipped: {sql}  -> {ex.Message}");
        }
    }
}
