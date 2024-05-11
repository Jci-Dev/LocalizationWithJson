using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using weapploc.Middleware;
using weapploc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<CultureInfoSettings>(builder.Configuration.GetSection("CultureInfoSettings"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<CultureInfoSettings>>().Value);

var cultureInfoSettingsSection = builder.Configuration.GetSection("CultureInfoSettings");
var cultureInfoSettings = cultureInfoSettingsSection.Get<CultureInfoSettings>();

builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = cultureInfoSettings.SupportedCultures;
    options.DefaultRequestCulture = new RequestCulture(cultureInfoSettings.DefaultCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddSingleton<LocalizationMiddleware>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<LocalizationMiddleware>();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
