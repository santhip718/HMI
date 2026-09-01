using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using VmcHmi.Api.Middleware;
using VmcHmi.Application;
using VmcHmi.Application.Commands;
using VmcHmi.Application.DTOs;
using VmcHmi.Application.Handlers;
using VmcHmi.Application.Interfaces;
using VmcHmi.Application.Queries;
using VmcHmi.Application.Validators;
using VmcHmi.Infrastructure;
using VmcHmi.Infrastructure.Auth;
using VmcHmi.Infrastructure.Data;
using VmcHmi.Infrastructure.Logging;
using VmcHmi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSettings["SigningKey"]
    ?? builder.Configuration["JWT_SIGNING_KEY"]
    ?? "VmcHmi_SuperSecretSigningKey_ForDevelopmentAndProductionFallback_2026!";

builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = builder.Configuration["DATABASE_URL"];
}
if (IsConnectionUrl(connectionString))
{
    connectionString = BuildConnectionStringFromUrl(connectionString!);
}
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = "Host=localhost;Port=5432;Database=learning_portal;Username=postgres;Password=postgres;SearchPath=hmi";
}

builder.Services.AddDbContext<HmiDbContext>(options =>
    options.UseNpgsql(connectionString,
        o => o.MigrationsHistoryTable("_hmi_migrations", "hmi")));

builder.Services.AddScoped<IMachineSessionRepository, MachineSessionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddScoped<IRequestHandler<LoginQuery, LoginResponse>, LoginHandler>();
builder.Services.AddScoped<IRequestHandler<GetCurrentStateQuery, SessionStateResponse>, GetCurrentStateHandler>();
builder.Services.AddScoped<IRequestHandler<ConfirmChecklistItemCommand>, ConfirmChecklistItemHandler>();
builder.Services.AddScoped<IRequestHandler<UnconfirmChecklistItemCommand>, UnconfirmChecklistItemHandler>();
builder.Services.AddScoped<IRequestHandler<AdvanceStageCommand>, AdvanceStageHandler>();
builder.Services.AddScoped<IRequestHandler<StartOperationCommand>, StartOperationHandler>();
builder.Services.AddScoped<IRequestHandler<StopOperationCommand>, StopOperationHandler>();

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ConfirmChecklistItemCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AdvanceStageCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<StartOperationCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<StopOperationCommandValidator>();

builder.Services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(signingKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "VMC HMI API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("HmiPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { },
    KnownProxies = { }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VMC HMI API");
        c.RoutePrefix = "swagger";
    });
}

// Resilient database seeding with retry loop for cloud cold-starts
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HmiDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var logger = scope.ServiceProvider.GetRequiredService<IAppLogger<Program>>();

    const int maxRetries = 10;
    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Attempting database migration and seeding (attempt {Attempt}/{MaxRetries})...", attempt, maxRetries);
            await SeedData.EnsureSeededAsync(db, hasher);
            logger.LogInformation("Database migration and seeding completed successfully.");
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning("Database connection not ready yet ({Message}). Retrying in 3 seconds...", ex.Message);
            await Task.Delay(3000);
        }
    }
}

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("HmiPolicy");
app.UseAuthentication();
app.UseAuthorization();

// Root & Health check endpoints for cloud load balancers / Render
app.MapGet("/", () => Results.Ok(new { status = "healthy", service = "VMC HMI API", time = DateTime.UtcNow }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }));

app.MapControllers();

app.Run();

static bool IsConnectionUrl(string? value) =>
    !string.IsNullOrWhiteSpace(value) &&
    (value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
     value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase));

static string BuildConnectionStringFromUrl(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var database = uri.AbsolutePath.TrimStart('/');

    var builder = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = database,
        Username = username,
        Password = password,
        SearchPath = "hmi",
        SslMode = Npgsql.SslMode.Prefer
    };
    return builder.ConnectionString;
}

