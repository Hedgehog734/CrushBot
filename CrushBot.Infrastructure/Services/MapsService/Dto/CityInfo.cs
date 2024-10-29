using CrushBot.Core.Dto;

namespace CrushBot.Infrastructure.Services.MapsService.Dto;

public class CityInfo : ICityInfo
{
    public required string Id { get; set; }

    public required string Title { get; set; }

    public required string City { get; set; }

    public required string CountryCode { get; set; }

    public required string PostalCode { get; set; }

    public required double Latitude { get; set; }

    public required double Longitude { get; set; }
}