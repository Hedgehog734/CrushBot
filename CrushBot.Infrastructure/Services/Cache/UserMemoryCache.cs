using CrushBot.Core.Interfaces;
using CrushBot.Core.Interfaces.Cache;
using CrushBot.Infrastructure.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace CrushBot.Infrastructure.Services.Cache;

public class UserMemoryCache<T>(
    IServiceProvider provider,
    IMemoryCache cache,
    IOptions<AppSettings> settings,
    ILogger<UserMemoryCache<T>> logger)
    : IUserMemoryCache<T> where T : class
{
    private readonly AppSettings _settings = settings.Value;

    public T? GetUser(long userId)
    {
        return cache.Get<T>(userId);
    }

    public void UpdateUser(long userId, T user)
    {
        var options = GetCacheOptions();
        cache.Set(userId, user, options);
    }

    public void RemoveUser(long userId)
    {
        cache.Remove(userId);
    }

    private MemoryCacheEntryOptions GetCacheOptions()
    {
        var lifetime = _settings.UserCacheLifetimeInMinutes;
        var expirationTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(lifetime));
        var expirationToken = new CancellationChangeToken(
            new CancellationTokenSource(TimeSpan.FromMinutes(lifetime + .0001)).Token);

        return new MemoryCacheEntryOptions()
            .SetPriority(CacheItemPriority.NeverRemove)
            .SetAbsoluteExpiration(expirationTime)
            .AddExpirationToken(expirationToken)
            .RegisterPostEvictionCallback(callback: EvictionCallback, state: this);
    }

    private void EvictionCallback(object userId, object? user, EvictionReason reason, object? state)
    {
        if (user == null)
        {
            logger.LogWarning("Eviction callback called with null user.");
            logger.LogDebug(Environment.StackTrace);
            return;
        }

        if (reason is EvictionReason.Removed or EvictionReason.Expired or EvictionReason.TokenExpired
            or EvictionReason.Capacity)
        {
            using var scope = provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IUserService>();
            OnEviction((T)user, service, reason == EvictionReason.Removed);
        }
    }

    public Func<T, IUserService, bool, Task> OnEviction { get; set; } = (_, _, _) => Task.CompletedTask;
}