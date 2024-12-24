using DashiToon.Api.Application;
using DashiToon.Api.Application.Common.Services;
using DashiToon.Api.Infrastructure;
using DashiToon.Api.Infrastructure.Data;
using DashiToon.Api.Web;
using DashiToon.Api.Web.Hubs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddKeyVaultIfConfigured(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices(builder.Configuration);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

await app.InitialiseDatabaseAsync();
await app.SeedDatabaseAsync();

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseCors("dev");
}
else
{
    app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.MapControllerRoute(
    "default",
    "{controller}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapHub<KonichiwassupHub>("/konichiwassup");

app.MapFallbackToFile("index.html");

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();

app.Run();

app.Services.GetRequiredService<ChapterAnalyticService>();

public partial class Program
{
}
