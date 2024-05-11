# Localization Services and Configuration in ASP.NET Core

This documentation page explains how to set up localization services and configuration in an ASP.NET Core application. The goal is to create a basic website that supports multiple cultures.

## Services

### CultureInfoSettings

`CultureInfoSettings` is a class that represents the culture settings of your application. It includes the default culture and a list of supported cultures. These settings are loaded from the `appsettings.json` file.

```csharp
public class CultureInfoSettings
{
    public string DefaultCulture { get; set; }
    public List<System.Globalization.CultureInfo> SupportedCultures { get; set; }
}
```

### JsonStringLocalizerFactory

`JsonStringLocalizerFactory` is a factory that creates instances of `JsonStringLocalizer`. It uses `IDistributedCache` to cache localized strings and `CultureInfoSettings` to get the current culture.

```csharp
public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource) =>
        new JsonStringLocalizer(cache, cultureInfoSettings);

    public IStringLocalizer Create(string baseName, string location) =>
        new JsonStringLocalizer(cache, cultureInfoSettings);
}
```

### JsonStringLocalizer

`JsonStringLocalizer` is a class that provides localized strings. It uses `IDistributedCache` to cache localized strings and `CultureInfoSettings` to get the current culture.

```csharp
public class JsonStringLocalizer : IStringLocalizer
{
    // Implementation details...
}
```

## Configuration

In the `Program.cs` file, you configure your application to use these services.

First, you load the `CultureInfoSettings` from the `appsettings.json` file and register it in the DI container.

```csharp
builder.Services.Configure<CultureInfoSettings>(builder.Configuration.GetSection("CultureInfoSettings"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<CultureInfoSettings>>().Value);
```

Then, you configure the `RequestLocalizationOptions` to use the `CultureInfoSettings`.

```csharp
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = cultureInfoSettings.SupportedCultures;
    options.DefaultRequestCulture = new RequestCulture(cultureInfoSettings.DefaultCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
```

Finally, you register the `LocalizationMiddleware` and `JsonStringLocalizerFactory` in the DI container.

```csharp
builder.Services.AddSingleton<LocalizationMiddleware>();
builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
```

Now, your application is ready to support multiple cultures. The `LocalizationMiddleware` will set the current culture for each request based on the `Accept-Language` HTTP header, and the `JsonStringLocalizer` will provide localized strings based on the current culture.

Sure, here's how localization is implemented on the `Index` page in your ASP.NET Core application.

## Implementation on Index Page

The `Index` page uses the `IStringLocalizer<T>` interface to localize text. `IStringLocalizer<T>` is injected into the view using the `@inject` directive, and then the `["key"]` indexer is used to get localized strings.

Here's an example of how you might use `IStringLocalizer<T>` in your `Index` page:

```razor
@page
@model IndexModel
@inject Microsoft.Extensions.Localization.IStringLocalizer<IndexModel> loc


<h1>Country</h1>
<p><strong>@loc["CountryId"]:</strong> @Model.CountryId</p>
<p><strong>@loc["CountryCode"]:</strong> @Model.CountryCode</p>
<p><strong>@loc["CountryName"]:</strong> @Model.CountryName</p>
<p><strong>@loc["Latitude"]:</strong> @Model.Latitude</p>
<p><strong>@loc["Longitude"]:</strong> @Model.Longitude</p>
```

In this example, "Welcome", "WelcomeText", "Country", "CountryId", "CountryCode", "CountryName", "Latitude", and "Longitude" are keys that correspond to entries in your resource files. The actual text that gets displayed will depend on the current culture.

The resource files should be named `MyFile.en-US.resx`, `MyFile.es-ES.resx`, etc., and placed in the `Resources` folder. Each resource file should contain entries for "Welcome", "WelcomeText", "Country", "CountryId", "CountryCode", "CountryName", "Latitude", and "Longitude".

When a user visits the `Index` page, the text that gets displayed will be localized according to the current culture. If the current culture is "en-US", the user will see the English version of the text. If the current culture is "es-ES", the user will see the Spanish version of the text.