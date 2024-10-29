using CrushBot.Core.Interfaces.Data.Repositories;

namespace CrushBot.Core.Interfaces.Data;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IFilterRepository Filters { get; }
    ICityRepository Cities { get; }
    ICityNameRepository CityNames { get; }
    ILikeRepository Likes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}