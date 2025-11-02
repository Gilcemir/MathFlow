using Jering.Javascript.NodeJS;
using MathFlow.Infrastructure.Converters;
using MathFlow.Infrastructure.Observability;
using MathFlow.Services;
using MathFlow.Services.Coverters;
using MathFlow.Infrastructure.IdentityServer.Configuration;
using MathFlow.Infrastructure.IdentityServer.Seeders;
using MathFlow.Application.Services.Identity;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddNodeJS();

// Configure OpenTelemetry
builder.AddOpenTelemetry();

// Configure Identity
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

// Register Email Sender (required for Identity)
builder.Services
    .AddScoped<IEmailSender<MathFlow.Infrastructure.IdentityServer.Models.ApplicationUser>,
        MathFlow.Infrastructure.IdentityServer.Services.EmailSender>();

// Register application services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();

builder.Services.Configure<OutOfProcessNodeJSServiceOptions>(options =>
{
    options.Concurrency = Concurrency.MultiProcess;
    options.ConcurrencyDegree = 4;
    options.EnableFileWatching = false;
});
builder.Services.Configure<NodeJSProcessOptions>(options =>
{
    options.ProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure/Converters/Scripts");
});

builder.Services.AddSingleton<WordProcessor>();
builder.Services.AddSingleton<OmmlToMathMLConverter>();

var app = builder.Build();

// Seed Identity data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await IdentitySeeder.SeedAsync(services, builder.Configuration);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

public partial class Program { }
