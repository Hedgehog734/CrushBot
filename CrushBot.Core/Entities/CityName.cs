using CrushBot.Core.Enums;

namespace CrushBot.Core.Entities;

public class CityName
{
    public required Language Language { get; set; }
    public required string Name { get; set; }

    public required string CityId { get; set; }
    public City City { get; set; } = null!;
}