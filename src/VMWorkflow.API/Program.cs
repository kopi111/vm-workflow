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
        Title = "VM Workflow API",
        Version = "v1",
        Description = "VM Provisioning & Security Workflow Platform"
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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VM Workflow API v1"));
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors("AppCors");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserBlockingMiddleware>();
app.MapControllers();
app.MapFallbackToFile("index.html");

await app.RunAsync();
