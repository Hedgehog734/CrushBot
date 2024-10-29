using CrushBot.Core.Entities;

namespace CrushBot.Application.Facades.Interfaces;

public interface ICityDistributedFacade
{
    City? GetCacheCity(string cityId);

    Task<City?> GetDbCityAsync(string cityId);

    Task AddDbCityNameAsync(CityName cityName);

    void UpdateCacheCity(string cityId, City city);
}