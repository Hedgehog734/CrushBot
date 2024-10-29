using CrushBot.Application.Facades.Interfaces;
using CrushBot.Core.Entities;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Interfaces.Cache;

namespace CrushBot.Application.Facades;

public class CityDistributedFacade(
    ICityMemoryCache<City> cityCache,
    ICityService cityService)
    : ICityDistributedFacade
{
    public City? GetCacheCity(string cityId)
    {
        return cityCache.GetCity(cityId);
    }

    public async Task<City?> GetDbCityAsync(string cityId)
    {
        return await cityService.GetCityAsync(cityId);
    }

    public async Task AddDbCityNameAsync(CityName cityName)
    {
        await cityService.AddCityNameAsync(cityName);
    }

    public void UpdateCacheCity(string cityId, City city)
    {
        cityCache.UpdateCity(cityId, city);
    }
}