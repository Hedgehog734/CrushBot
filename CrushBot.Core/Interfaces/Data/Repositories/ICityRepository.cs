using CrushBot.Core.Entities;

namespace CrushBot.Core.Interfaces.Data.Repositories;

public interface ICityRepository
{
    Task<City?> GetCityAsync(string cityId);

    void AddCity(City city);
}