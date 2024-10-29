using System.Text.Json.Serialization;

namespace CrushBot.Infrastructure.Services.MapsService.Models;

public class Position
{
    [JsonPropertyName("lng")]
    public double Lat { get; set; }

    [JsonPropertyName("lat")]
    public double Lng { get; set; }
}