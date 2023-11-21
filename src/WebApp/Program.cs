using CheckServerSetup.Checks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Runtime.InteropServices;
using WebApp.Platform;

var builder = WebApplication.CreateBuilder(args);

// Persist data protection keys
var keysFolder = Path.Combine(builder.Configuration["PersistedFilesBasePath"] ?? "./", "DataProtectionKeys");
var dataProtectionBuilder = builder.Services.AddDataProtection().PersistKeysToFileSystem(Directory.CreateDirectory(keysFolder));
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    dataProtectionBuilder.ProtectKeysWithDpapi(protectToLocalMachine: true);

// Bind application settings.
builder.Configuration.GetSection(nameof(CheckEmailOptions))
    .Bind(ApplicationSettings.CheckEmailOptions);
builder.Configuration.GetSection(nameof(CheckDatabaseOptions))
    .Bind(ApplicationSettings.CheckDatabaseOptions);
builder.Configuration.GetSection(nameof(CheckDatabaseEmailOptions))
    .Bind(ApplicationSettings.CheckDatabaseEmailOptions);
builder.Configuration.GetSection(nameof(CheckExternalServiceOptions))
    .Bind(ApplicationSettings.CheckExternalServiceOptions);
builder.Configuration.GetSection(nameof(CheckDotnetVersionOptions))
    .Bind(ApplicationSettings.CheckDotnetVersionOptions);
builder.Configuration.GetSection(nameof(DevOptions)).Bind(ApplicationSettings.DevOptions);
ApplicationSettings.ServerName =
    builder.Configuration.GetValue<string>(nameof(ApplicationSettings.ServerName)) ?? "Unknown";

// Configure authentication.
if (ApplicationSettings.DevOptions.UseLocalAuth)
{
    // When running locally, use a built-in authenticated user.
    builder.Services
        .AddAuthentication(LocalAuthenticationHandler.BasicAuthenticationScheme)
        .AddScheme<AuthenticationSchemeOptions, LocalAuthenticationHandler>(
            LocalAuthenticationHandler.BasicAuthenticationScheme, null);
}
else
{
    // When running on the server, require an Azure AD login account (configured in the app settings file).
    builder.Services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
}

builder.Services.AddAuthorization();

// Configure HSTS (max age: two years).
if (!builder.Environment.IsDevelopment()) builder.Services.AddHsts(opts => opts.MaxAge = TimeSpan.FromDays(730));

// Configure the UI.
builder.Services.AddRazorPages().AddMicrosoftIdentityUI();
builder.Services.AddWebOptimizer();

// Build the application.
var app = builder.Build();
var env = app.Environment;

// Configure the HTTP request pipeline.
if (env.IsProduction())
{
    // Production
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // Staging or Development
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseWebOptimizer();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
