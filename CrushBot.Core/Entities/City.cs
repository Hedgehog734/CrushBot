namespace CrushBot.Core.Entities;

public class City
{
    public required string Id { get; set; }

    public required double Latitude { get; set; }

    public required double Longitude { get; set; }

    public ICollection<CityName> CityNames { get; } = [];
}