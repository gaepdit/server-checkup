using ServerCheckupLibrary.Hubs;
using WebApp.Platform;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

// Configure logging.
builder.Logging.ClearProviders().AddZLoggerConsole(options => options.UseJsonFormatter());

// Persist data protection keys.
builder.Services.AddDataProtection();

// Bind application settings.
builder.BindAppSettings();

// Configure authentication and authorization.
builder.ConfigureAuthentication();
builder.Services.AddAuthorization();

// Add SignalR
builder.Services.AddSignalR();

// Configure HSTS (max age: two years).
if (!isDevelopment) builder.Services.AddHsts(opts => opts.MaxAge = TimeSpan.FromDays(730));

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
