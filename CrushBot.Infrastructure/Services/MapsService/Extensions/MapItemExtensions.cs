using CrushBot.Infrastructure.Services.MapsService.Models;

namespace CrushBot.Infrastructure.Services.MapsService.Extensions;

public static class MapItemExtensions
{
    private const string City = "City";
    private const string District = "District";

    public static bool IsCity(this MapItem item)
    {
        return item.LocalityType.Equals(City, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsDistrict(this MapItem item)
    {
        return item.LocalityType.Equals(District, StringComparison.OrdinalIgnoreCase);
    }
}