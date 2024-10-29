using CrushBot.Core.Dto;

namespace CrushBot.Core.Interfaces;

public interface IMapsService
{
    Task<IEnumerable<ICityInfo>> GetCitiesFromLocationAsync(double latitude, double longitude, string language,
        int limit = 1);

    Task<IEnumerable<ICityInfo>> GetCitiesFromTextAsync(string text, string language, int limit = 20);

    Task<ICityInfo> GetCityByIdAsync(string cityId, string language);
}