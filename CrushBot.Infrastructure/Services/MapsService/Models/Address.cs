using System.Text.Json.Serialization;

namespace CrushBot.Infrastructure.Services.MapsService.Models;

public class Address
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = null!;

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = null!;

    [JsonPropertyName("countryName")]
    public string CountryName { get; set; } = null!;

    [JsonPropertyName("stateCode")]
    public string StateCode { get; set; } = null!;

    [JsonPropertyName("state")]
    public string State { get; set; } = null!;

    [JsonPropertyName("county")]
    public string County { get; set; } = null!;

    [JsonPropertyName("city")]
    public string City { get; set; } = null!;

    [JsonPropertyName("district")]
    public string District { get; set; } = null!;

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = null!;
}