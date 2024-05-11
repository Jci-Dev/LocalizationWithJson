using System.Globalization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace weapploc.Services;

public class JsonStringLocalizer(IDistributedCache cache, CultureInfoSettings cultureInfoSettings) : IStringLocalizer
{
    private readonly JsonSerializer _serializer = new();

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value ?? name, value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var actualValue = this[name];
            return !actualValue.ResourceNotFound
                ? new LocalizedString(name, string.Format(actualValue.Value, arguments), false)
                : actualValue;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var filePath = $"Resources/MyFile.{Thread.CurrentThread.CurrentCulture.Name}.json";
        using (var str = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var sReader = new StreamReader(str))
        using (var reader = new JsonTextReader(sReader))
        {
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    continue;
                var key = (string)reader.Value!;
                reader.Read();
                var value = _serializer.Deserialize<string>(reader);
                yield return new LocalizedString(key, value!, false);
            }
        }
    }

    private string GetString(string key)
    {
        var relativeFilePath = $"Resources/MyFile.{Thread.CurrentThread.CurrentCulture.Name}.json";
        var fullFilePath = Path.GetFullPath(relativeFilePath);
        if (!File.Exists(fullFilePath))
        {
            var culture = new CultureInfo(cultureInfoSettings.DefaultCulture);
            Thread.CurrentThread.CurrentCulture = culture;
            relativeFilePath = $"Resources/MyFile.{Thread.CurrentThread.CurrentCulture.Name}.json";
            fullFilePath = Path.GetFullPath(relativeFilePath);

            if (!File.Exists(fullFilePath)) return default;
        }

        var cacheKey = $"locale_{Thread.CurrentThread.CurrentCulture.Name}_{key}";
        var cacheValue = cache.GetString(cacheKey);
        if (!string.IsNullOrEmpty(cacheValue)) return cacheValue;
        var result = GetValueFromJson(key, Path.GetFullPath(relativeFilePath));
        if (!string.IsNullOrEmpty(result)) cache.SetString(cacheKey, result);
        return result;
    }

    private string GetValueFromJson(string propertyName, string filePath)
    {
        if (propertyName == string.Empty) return default;
        if (filePath == string.Empty) return default;
        using (var str = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var sReader = new StreamReader(str))
        using (var reader = new JsonTextReader(sReader))
        {
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName || (string)reader.Value! != propertyName) continue;
                reader.Read();
                return _serializer.Deserialize<string>(reader);
            }

            return default;
        }
    }
}