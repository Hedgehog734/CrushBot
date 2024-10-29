namespace CrushBot.Core.Exceptions;

public class MapsException : Exception
{
    public MapsException()
    {
    }

    public MapsException(string message)
        : base(message)
    {
    }

    public MapsException(string message, Exception inner)
        : base(message, inner)
    {
    }
}