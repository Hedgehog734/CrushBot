using System.Text.Json.Serialization;

namespace CrushBot.Infrastructure.Services.MapsService.Models;

public class MapItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("resultType")]
    public string ResultType { get; set; } = null!;

    [JsonPropertyName("localityType")]
    public string LocalityType { get; set; } = null!;

    [JsonPropertyName("address")]
    public required Address Address { get; set; }

    [JsonPropertyName("position")]
    public required Position Position { get; set; }
}