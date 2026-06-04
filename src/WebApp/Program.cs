using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.AspNetCore;
using ServerCheckupLibrary.Checks;
using ServerCheckupLibrary.Hubs;
using WebApp.Platform;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

// Persist data protection keys
builder.Services.AddDataProtection();

// Bind application settings.
builder.Configuration.GetSection(nameof(CheckEmailOptions)).Bind(AppSettings.CheckEmailOptions);
builder.Configuration.GetSection(nameof(CheckDatabaseOptions)).Bind(AppSettings.CheckDatabaseOptions);
builder.Configuration.GetSection(nameof(CheckDatabaseEmailOptions)).Bind(AppSettings.CheckDatabaseEmailOptions);
builder.Configuration.GetSection(nameof(CheckExternalServiceOptions)).Bind(AppSettings.CheckExternalServiceOptions);
builder.Configuration.GetSection(nameof(CheckDotnetVersionOptions)).Bind(AppSettings.CheckDotnetVersionOptions);
builder.Configuration.GetSection(nameof(DevOptions)).Bind(AppSettings.DevOptions);
AppSettings.ServerName = builder.Configuration.GetValue<string>(nameof(AppSettings.ServerName)) ?? "Unknown";
builder.Configuration.GetSection(nameof(AppSettings.RaygunSettings)).Bind(AppSettings.RaygunSettings);

// Configure authentication.
if (AppSettings.DevOptions.UseLocalAuth)
{
    // When running locally, use a built-in authenticated user.
    builder.Services
        .AddAuthentication(LocalAuthenticationHandler.BasicAuthenticationScheme)
        .AddScheme<AuthenticationSchemeOptions, LocalAuthenticationHandler>(
            LocalAuthenticationHandler.BasicAuthenticationScheme, null);
}
else
{
    // When running on the server, require an OIDC login provider (configured in the appsettings file).
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddOpenIdConnect(configureOptions: options =>
        {
            var configSection = builder.Configuration.GetSection("OIDC");

            options.Authority = configSection["Authority"];
            options.ClientId = configSection["ClientId"];
            options.ClientSecret = configSection["ClientSecret"];
            options.CallbackPath = configSection["CallbackPath"];

            options.Scope.Add("email");
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "email" };
        });
}

builder.Services.AddAuthorization();

// Add SignalR
builder.Services.AddSignalR();

// Configure HSTS (max age: two years).
if (!isDevelopment) builder.Services.AddHsts(opts => opts.MaxAge = TimeSpan.FromDays(730));

// Configure application monitoring.
if (!string.IsNullOrEmpty(AppSettings.RaygunSettings.ApiKey))
{
    // Add error logging
    builder.Services
        .AddSingleton(s =>
        {
            var client = new RaygunClient(s.GetService<RaygunSettings>()!, s.GetService<IRaygunUserProvider>()!);
            client.SendingMessage += (_, eventArgs) =>
                eventArgs.Message.Details.Tags.Add(builder.Environment.EnvironmentName);
            return client;
        })
        .AddRaygun(builder.Configuration, opts =>
        {
            opts.ApiKey = AppSettings.RaygunSettings.ApiKey;
            opts.ExcludeErrorsFromLocal = false;
            opts.IgnoreFormFieldNames = ["*Password"];
        })
        .AddRaygunUserProvider()
        .AddHttpContextAccessor(); // needed by RaygunScriptPartial
}

// Configure the UI.
builder.Services.AddRazorPages();
builder.Services.AddWebOptimizer(minifyJavaScript: !isDevelopment);

// Add HttpClient (used by CheckExternalService)
builder.Services.AddHttpClient();

// Build the application.
var app = builder.Build();

// Configure error handling.
if (isDevelopment) app.UseDeveloperExceptionPage(); // Development
else app.UseExceptionHandler("/Error"); // Production or Staging

// Configure the HTTP request pipeline.
if (!string.IsNullOrEmpty(AppSettings.RaygunSettings.ApiKey)) app.UseRaygun();
app
    .UseStatusCodePages()
    .UseHttpsRedirection()
    .UseWebOptimizer()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization();
app.MapRazorPages();
app.MapHub<CheckHub>("/checkHub");

await app.RunAsync();
