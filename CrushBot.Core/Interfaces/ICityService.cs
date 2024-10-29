using CrushBot.Core.Entities;

namespace CrushBot.Core.Interfaces;

public interface ICityService
{
    Task<City?> GetCityAsync(string cityId);

    Task AddCityNameAsync(CityName cityName);

    Task AddCityWithCityNameAsync(City city, CityName cityName);
}