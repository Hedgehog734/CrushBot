using CrushBot.Core.Entities;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CrushBot.Infrastructure.Data.Repositories;

public class CityNameRepository(AppDbContext context) : ICityNameRepository
{
    public async Task<CityName?> GetCityNameAsync(string cityId, Language language)
    {
        return await context.CityNames
            .AsNoTracking()
            .Where(x => x.CityId == cityId && x.Language == language)
            .SingleOrDefaultAsync();
    }

    public void AddCityName(CityName cityName)
    {
        context.CityNames.Add(cityName);
    }
}