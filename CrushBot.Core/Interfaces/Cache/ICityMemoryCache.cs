namespace CrushBot.Core.Interfaces.Cache;

public interface ICityMemoryCache<T> where T : class
{
    public T? GetCity(string cityId);

    public void UpdateCity(string cityId, T city);
}