using MyAppRoot.WebApp.Platform.Settings;
using WebApp.Platform;

var builder = WebApplication.CreateBuilder(args);
var isLocal = builder.Environment.IsLocalEnv();

// Bind application settings.
builder.Configuration.GetSection(nameof(CheckServerSetup.Checks.CheckEmailOptions))
    .Bind(ApplicationSettings.CheckEmailOptions);
builder.Configuration.GetSection(nameof(CheckServerSetup.Checks.CheckDatabaseOptions))
    .Bind(ApplicationSettings.CheckDatabaseOptions);
builder.Configuration.GetSection(nameof(CheckServerSetup.Checks.CheckExternalServiceOptions))
    .Bind(ApplicationSettings.CheckExternalServiceOptions);

if (!isLocal) builder.Services.AddHsts(opts => opts.MaxAge = TimeSpan.FromMinutes(300));
builder.Services.AddRazorPages();
builder.Services.AddWebOptimizer();

var app = builder.Build();
var env = app.Environment;

if (env.IsProduction() || env.IsStaging())
{
    // Production or Staging
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // Development or Local
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseWebOptimizer();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
