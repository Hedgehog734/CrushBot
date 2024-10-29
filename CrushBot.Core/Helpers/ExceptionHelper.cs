using System.Net;
using Telegram.Bot.Exceptions;

namespace CrushBot.Core.Helpers;

public static class ExceptionHelper
{
    public static bool IsRequestException(Exception exception, out RequestException requestEx,
        params HttpStatusCode[] codes)
    {
        var currentEx = exception;

        while (currentEx != null)
        {
            if (currentEx is RequestException { HttpStatusCode: not null } ex &&
                codes.Select(x => (int)x).Contains((int)ex.HttpStatusCode))
            {
                requestEx = ex;
                return true;
            }

            currentEx = currentEx.InnerException;
        }

        requestEx = null!;
        return false;
    }

    public static bool IsApiException(Exception exception, out ApiRequestException apiEx,
        params HttpStatusCode[] codes)

    {
        var currentEx = exception;

        while (currentEx != null)
        {
            if (currentEx is ApiRequestException ex &&
                codes.Contains((HttpStatusCode)ex.ErrorCode))
            {
                apiEx = ex;
                return true;
            }

            currentEx = currentEx.InnerException;
        }

        apiEx = null!;
        return false;
    }
}