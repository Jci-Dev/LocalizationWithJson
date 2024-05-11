using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;

namespace weapploc.Services;

public class JsonStringLocalizerFactory(IDistributedCache cache, CultureInfoSettings cultureInfoSettings) : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource) =>
        new JsonStringLocalizer(cache, cultureInfoSettings);

    public IStringLocalizer Create(string baseName, string location) =>
        new JsonStringLocalizer(cache, cultureInfoSettings);
}