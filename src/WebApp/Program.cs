using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.AspNetCore;
using ServerCheckupLibrary.Checks;
using System.Runtime.InteropServices;
using WebApp.Platform;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

// Persist data protection keys
var keysFolder = Path.Combine(builder.Configuration["PersistedFilesBasePath"] ?? "./", "DataProtectionKeys");
var dataProtectionBuilder =
    builder.Services.AddDataProtection().PersistKeysToFileSystem(Directory.CreateDirectory(keysFolder));
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
builder.Configuration.GetSection(nameof(ApplicationSettings.RaygunSettings))
    .Bind(ApplicationSettings.RaygunSettings);

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
        .AddMicrosoftIdentityWebApp(builder.Configuration);
}

builder.Services.AddAuthorization();

// Configure HSTS (max age: two years).
if (!isDevelopment) builder.Services.AddHsts(opts => opts.MaxAge = TimeSpan.FromDays(730));

// Configure application monitoring.
if (!string.IsNullOrEmpty(ApplicationSettings.RaygunSettings.ApiKey))
{
    // Add error logging
    builder.Services
        .AddSingleton(s =>
        {
            var client = new RaygunClient(s.GetService<RaygunSettings>()!, s.GetService<IRaygunUserProvider>()!);
            client.SendingMessage += (_, eventArgs) =>
            {
                eventArgs.Message.Details.Tags.Add(builder.Environment.EnvironmentName);
            };
            return client;
        })
        .AddRaygun(builder.Configuration, opts =>
        {
            opts.ApiKey = ApplicationSettings.RaygunSettings.ApiKey;
            opts.ExcludeErrorsFromLocal = false;
            opts.IgnoreFormFieldNames = ["*Password"];
        })
        .AddRaygunUserProvider()
        .AddHttpContextAccessor(); // needed by RaygunScriptPartial
}

// Configure the UI.
builder.Services.AddRazorPages().AddMicrosoftIdentityUI();
builder.Services.AddWebOptimizer(minifyJavaScript: !isDevelopment);

// Build the application.
var app = builder.Build();

// Configure error handling.
if (isDevelopment) app.UseDeveloperExceptionPage(); // Development
else app.UseExceptionHandler("/Error"); // Production or Staging

// Configure the HTTP request pipeline.
if (!string.IsNullOrEmpty(ApplicationSettings.RaygunSettings.ApiKey)) app.UseRaygun();
app
    .UseStatusCodePages()
    .UseHttpsRedirection()
    .UseWebOptimizer()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization();
app.MapRazorPages();

await app.RunAsync();
