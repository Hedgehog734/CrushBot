using CrushBot.Core.Entities;
using CrushBot.Core.Enums;

namespace CrushBot.Core.Interfaces.Data.Repositories;

public interface ICityNameRepository
{
    Task<CityName?> GetCityNameAsync(string cityId, Language language);

    void AddCityName(CityName cityName);
}