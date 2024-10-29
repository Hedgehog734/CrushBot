using CrushBot.Core.Entities;
using CrushBot.Core.Interfaces.Data.Repositories;

namespace CrushBot.Infrastructure.Data.Repositories;

public class FilterRepository(AppDbContext context) : IFilterRepository
{
    public void UpdateFilter(UserFilter existing, UserFilter updated)
    {
        updated.Id = existing.Id;
        context.Entry(existing).CurrentValues.SetValues(updated);
    }
}