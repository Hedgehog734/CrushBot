using CrushBot.Application.Services.Exceptions;
using CrushBot.Core.Entities;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Interfaces.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrushBot.Application.Services;

public class CityService(IUnitOfWork uow, ILogger<CityService> logger) : ICityService
{
    public async Task<City?> GetCityAsync(string cityId)
    {
        return await uow.Cities.GetCityAsync(cityId);
    }

    public async Task AddCityNameAsync(CityName cityName)
    {
        try
        {
            uow.CityNames.AddCityName(cityName);
            await uow.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var message = $"Database update error: {ex.Message}";
            logger.LogWarning(message);
            throw new ServiceException(message, ex);
        }
        catch (Exception ex)
        {
            var message = $"Error while adding city name with key " +
                          $"({cityName.CityId}, {cityName.Language}). Message: {ex.Message}";

            logger.LogError(message);
            throw new ServiceException(message, ex);
        }
    }

    public async Task AddCityWithCityNameAsync(City city, CityName cityName)
    {
        try
        {
            var existingCity = await uow.Cities.GetCityAsync(city.Id);

            if (existingCity == null)
            {
                uow.Cities.AddCity(city);
            }

            var existingCityName = await uow.CityNames.GetCityNameAsync(cityName.CityId, cityName.Language);

            if (existingCityName == null)
            {
                uow.CityNames.AddCityName(cityName);
            }

            await uow.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var message = $"Database update error: {ex.Message}";
            logger.LogWarning(message);
            throw new ServiceException(message, ex);
        }
        catch (Exception ex)
        {
            var message = $"Error while adding city with key {city.Id} or city name " +
                          $"with key ({cityName.CityId}, {cityName.Language}). Message: {ex.Message}";

            logger.LogError(message);
            throw new ServiceException(message, ex);
        }
    }
}