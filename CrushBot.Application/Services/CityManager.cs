using CrushBot.Application.Facades.Interfaces;
using CrushBot.Application.Interfaces;
using CrushBot.Application.Services.Exceptions;
using CrushBot.Core.Entities;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;

namespace CrushBot.Application.Services;

public class CityManager(
    ICityDistributedFacade cityFacade,
    IMapsService mapsService)
    : ICityManager
{
    public async Task<string> GetAddCityName(string cityId, Language language)
    {
        var city = cityFacade.GetCacheCity(cityId);

        if (city == null)
        {
            city = await cityFacade.GetDbCityAsync(cityId);

            if (city == null)
            {
                throw new ServiceException($"No city found with Id {cityId}.");
            }
        }

        var cityName = city.CityNames.FirstOrDefault(x => x.Language == language);

        if (cityName == null)
        {
            var code = LanguageHelper.GetCode(language);
            var cityInfo = await mapsService.GetCityByIdAsync(cityId, code);

            cityName = new CityName
            {
                CityId = cityId,
                Language = language,
                Name = cityInfo.City
            };

            await cityFacade.AddDbCityNameAsync(cityName);
            city.CityNames.Add(cityName);

            cityFacade.UpdateCacheCity(cityId, city);
        }

        return cityName.Name;
    }
}