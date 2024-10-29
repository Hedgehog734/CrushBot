namespace CrushBot.Application.StateMachine.Context;

public class UserContext
{
    private readonly Dictionary<string, object> _data = [];

    public void SetData<T>(string key, T data)
    {
        if (data != null)
        {
            _data[key] = data;
        }
    }

    public T? GetData<T>(string key)
    {
        if (_data.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return default;
    }

    public void RemoveData(string key)
    {
        _data.Remove(key);
    }
}