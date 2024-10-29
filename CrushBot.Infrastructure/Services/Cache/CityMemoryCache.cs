using CrushBot.Core.Interfaces.Cache;
using CrushBot.Infrastructure.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CrushBot.Infrastructure.Services.Cache;

public class CityMemoryCache<T>(IMemoryCache cache, IOptions<AppSettings> settings)
    : ICityMemoryCache<T> where T : class
{
    private readonly AppSettings _settings = settings.Value;

    public T? GetCity(string cityId)
    {
        return cache.Get<T>(cityId);
    }

    public void UpdateCity(string cityId, T city)
    {
        var options = GetCacheOptions();
        cache.Set(cityId, city, options);
    }

    private MemoryCacheEntryOptions GetCacheOptions()
    {
        var lifetime = _settings.CityCacheLifetimeInMinutes;
        var expiration = TimeSpan.FromMinutes(lifetime);
        return new MemoryCacheEntryOptions().SetSlidingExpiration(expiration);
    }
}