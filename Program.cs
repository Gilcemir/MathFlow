using Jering.Javascript.NodeJS;
using MathFlow.Infrastructure.Converters;
using MathFlow.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddNodeJS();

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
