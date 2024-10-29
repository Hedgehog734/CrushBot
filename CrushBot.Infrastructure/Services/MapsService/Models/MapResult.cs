using System.Text.Json.Serialization;

namespace CrushBot.Infrastructure.Services.MapsService.Models;

public class MapResult
{
    [JsonPropertyName("items")]
    public List<MapItem> Items { get; set; } = new();
}