using CrushBot.Core.Entities;

namespace CrushBot.Core.Interfaces.Data.Repositories;

public interface IFilterRepository
{
    void UpdateFilter(UserFilter existing, UserFilter updated);
}