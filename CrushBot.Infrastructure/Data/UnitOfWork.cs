using CrushBot.Core.Interfaces.Data;
using CrushBot.Core.Interfaces.Data.Repositories;
using CrushBot.Infrastructure.Data.Repositories;

namespace CrushBot.Infrastructure.Data;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IUserRepository? _users;
    private IFilterRepository? _filters;
    private ICityRepository? _cities;
    private ICityNameRepository? _cityNames;
    private ILikeRepository? _likes;

    public IUserRepository Users => _users ??= new UserRepository(context);
    public IFilterRepository Filters => _filters ??= new FilterRepository(context);
    public ICityRepository Cities => _cities ??= new CityRepository(context);
    public ICityNameRepository CityNames => _cityNames ??= new CityNameRepository(context);
    public ILikeRepository Likes => _likes ??= new LikeRepository(context);


    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                context.Dispose();
            }
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}