namespace CrushBot.Core.Interfaces.Cache;

public interface IUserMemoryCache<T> where T : class
{
    T? GetUser(long userId);

    void UpdateUser(long userId, T user);

    void RemoveUser(long userId);

    Func<T, IUserService, bool, Task> OnEviction { get; set; }
}