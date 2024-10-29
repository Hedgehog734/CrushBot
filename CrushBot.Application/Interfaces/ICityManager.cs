using CrushBot.Core.Enums;

namespace CrushBot.Application.Interfaces;

public interface ICityManager
{
    Task<string> GetAddCityName(string cityId, Language language);
}