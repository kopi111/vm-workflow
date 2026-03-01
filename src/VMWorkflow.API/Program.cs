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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString != null && connectionString.Contains("Server="))
{
    builder.Services.AddDbContext<WorkflowDbContext>(options =>
        options.UseSqlServer(connectionString,
            b => b.MigrationsAssembly("VMWorkflow.Infrastructure")));
}
else
{
    builder.Services.AddDbContext<WorkflowDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=vmworkflow.db"));
}

// --- Application Services ---
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddSingleton<IWorkflowEngine, WorkflowEngine>();
builder.Services.AddSingleton<IScriptGenerationService, FortiGateScriptGenerator>();

// --- Infrastructure Stubs ---
builder.Services.AddScoped<INetBoxService, StubNetBoxService>();
builder.Services.AddScoped<IFortiGateService, StubFortiGateService>();

// --- JWT Authentication ---
var jwtSection = builder.Configuration.GetSection("Jwt");
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!))
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

// --- CORS (dev mode) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// --- Ensure DB Created (dev) ---
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
    db.Database.EnsureCreated();
}

// --- Middleware Pipeline ---
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionHandler>();
app.UseMiddleware<AuditLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VM Workflow API v1"));
}

app.UseCors("DevCors");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserBlockingMiddleware>();
app.MapControllers();

app.Run();
