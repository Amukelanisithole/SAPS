using Database.AppDBContext;
using Database.Entities;
using Database.Seed;
using Logic.BusinessLogic;
using Logic.BusinessLogicInterface;
using Logic.Model.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Globalization;
using System.Text;
using static Logic.Enums.Enums;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    Log.Fatal(e.ExceptionObject as Exception, "SAPS terminated unexpectedly");
    Log.CloseAndFlush();
};

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddControllersWithViews();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
        else
        {
            policy.AllowAnyOrigin();
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditLogWriter, AuditLogWriterLogic>();
builder.Services.AddScoped<IPreliminaryReportLogic, PreliminaryReportLogic>();
builder.Services.AddScoped<IDocketLogic, DocketLogic>();
builder.Services.AddScoped<IUserManagementLogic, UserManagementLogic>();
builder.Services.AddScoped<IStationLogic, StationLogic>();
builder.Services.AddScoped<INotificationLogic, NotificationLogic>();
builder.Services.AddScoped<IReferralLogic, ReferralLogic>();
builder.Services.AddScoped<IEscalationLogic, EscalationLogic>();
builder.Services.AddScoped<IBackupLogic, BackupLogic>();
builder.Services.AddScoped<IPasswordResetLogic, PasswordResetLogic>();
builder.Services.AddScoped<IAuditLogQueryLogic, AuditLogQueryLogic>();
builder.Services.AddScoped<IDocketCommentLogic, DocketCommentLogic>();
builder.Services.AddScoped<IAdminReportLogic, AdminReportLogic>();
builder.Services.AddScoped<ICsvExportService, CsvExportService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailVerificationLogic, EmailVerificationLogic>();
builder.Services.AddHttpClient<IPoliceStationLookupService, PoliceStationLookupService>();
builder.Services.AddScoped<IStationRoutingService, StationRoutingService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IAffidavitLogic, AffidavitLogic>();
builder.Services.AddScoped<IEmergencyAlertLogic, EmergencyAlertLogic>();
builder.Services.AddScoped<IAffidavitPdfService, AffidavitPdfService>();
builder.Services.AddScoped<IDocketEvidenceLogic, DocketEvidenceLogic>();
builder.Services.AddScoped<IDocketPdfService, DocketPdfService>();
builder.Services.AddScoped<IDocketExportLogic, DocketExportLogic>();
builder.Services.AddScoped<IDocketDiaryLogic, DocketDiaryLogic>();
builder.Services.AddScoped<IDocketFollowUpLogic, DocketFollowUpLogic>();
builder.Services.Configure<Logic.Model.DocketSettings>(builder.Configuration.GetSection("Dockets"));
builder.Services.AddHostedService<SAPS.Services.DocketFollowUpService>();

builder.Services.Configure<Logic.Model.AppSettings>(builder.Configuration.GetSection("App"));
builder.Services.PostConfigure<Logic.Model.AppSettings>(options =>
{
    if (string.IsNullOrWhiteSpace(options.UploadsRootPath))
    {
        options.UploadsRootPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "uploads");
    }
});
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
if (string.IsNullOrWhiteSpace(builder.Configuration["Smtp:Host"]))
{
    builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();
}
else
{
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
}

builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("Sms"));
if (string.IsNullOrWhiteSpace(builder.Configuration["Sms:AccountSid"]))
{
    builder.Services.AddScoped<ISmsSender, LoggingSmsSender>();
}
else
{
    builder.Services.AddHttpClient<ISmsSender, TwilioSmsSender>();
}

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];
if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException(
        "JwtSettings:Secret is not configured. Set it via 'dotnet user-secrets set JwtSettings:Secret \"<value>\"' " +
        "in Development, or the JwtSettings__Secret environment variable elsewhere. It must never be committed to appsettings.json.");
}

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(jwtSettings.GetValue<int>("ExpirationInMinutes"));
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    if (!context.Roles.Any())
    {
        context.Roles.AddRange(
            new Role { Id = ERole.Complainant, Name = "Complainant" },
            new Role { Id = ERole.SapsOfficer, Name = "SapsOfficer" },
            new Role { Id = ERole.StationCommander, Name = "StationCommander" },
            new Role { Id = ERole.SystemAdministrator, Name = "SystemAdministrator" }
        );
        context.SaveChanges();
    }

    if (!context.Users.Any(u => u.RoleId == ERole.SystemAdministrator))
    {
        var seedAdmin = app.Configuration.GetSection("SeedAdmin");
        var seedPassword = seedAdmin["Password"];
        if (string.IsNullOrWhiteSpace(seedPassword))
        {
            app.Logger.LogWarning(
                "No SystemAdministrator account exists and SeedAdmin:Password is not configured, so none was created. " +
                "Set it via 'dotnet user-secrets set SeedAdmin:Password \"<value>\"' in Development, or the " +
                "SeedAdmin__Password environment variable elsewhere, then restart.");
        }
        else
        {
            context.Users.Add(new User
            {
                FullName = seedAdmin["FullName"] ?? "System Administrator",
                IdNumber = seedAdmin["IdNumber"] ?? "0000000000000",
                Email = seedAdmin["Email"] ?? "admin@saps.local",
                PhoneNumber = seedAdmin["PhoneNumber"] ?? "0000000000",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedPassword),
                RoleId = ERole.SystemAdministrator,
                StationId = null,
                CreatedDate = DateTime.UtcNow
            });
            context.SaveChanges();
        }
    }

    foreach (var seedTemplate in EmailTemplateSeedData.GetSeedTemplates())
    {
        var existingTemplate = context.EmailTemplates.FirstOrDefault(t => t.Name == seedTemplate.Name);
        if (existingTemplate == null)
        {
            context.EmailTemplates.Add(seedTemplate);
        }
        else
        {
            existingTemplate.Description = seedTemplate.Description;
            existingTemplate.Subject = seedTemplate.Subject;
            existingTemplate.Body = seedTemplate.Body;
            existingTemplate.Placeholders = seedTemplate.Placeholders;
        }
    }
    context.SaveChanges();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();
