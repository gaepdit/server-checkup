using WebApp.Platform;

var builder = WebApplication.CreateBuilder(args);
var isLocal = builder.Environment.IsLocalEnv();

// Add services to the container.
if (!isLocal) builder.Services.AddHsts(opts => opts.MaxAge = TimeSpan.FromMinutes(300));
builder.Services.AddRazorPages();
builder.Services.AddWebOptimizer();

var app = builder.Build();
var env = app.Environment;

// Configure the HTTP request pipeline.
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
