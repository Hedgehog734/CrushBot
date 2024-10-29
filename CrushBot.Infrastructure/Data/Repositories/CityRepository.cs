using CrushBot.Core.Entities;
using CrushBot.Core.Interfaces.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CrushBot.Infrastructure.Data.Repositories;

public class CityRepository(AppDbContext context) : ICityRepository
{
    public async Task<City?> GetCityAsync(string cityId)
    {
        return await context.Cities
            .AsNoTracking()
            .Include(x => x.CityNames)
            .FirstOrDefaultAsync(x => x.Id == cityId);
    }

    public void AddCity(City city)
    {
        context.Cities.Add(city);
    }
}