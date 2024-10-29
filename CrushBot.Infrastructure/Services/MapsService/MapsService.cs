using System.Globalization;
using System.Text.Json;
using CrushBot.Core.Dto;
using CrushBot.Core.Exceptions;
using CrushBot.Core.Interfaces;
using CrushBot.Infrastructure.Services.MapsService.Dto;
using CrushBot.Infrastructure.Services.MapsService.Extensions;
using CrushBot.Infrastructure.Services.MapsService.Models;

namespace CrushBot.Infrastructure.Services.MapsService;

public class MapsService(IHttpClientFactory clientFactory) : IMapsService
{
    public const string RevgeocodeClient = "revgeocode";
    public const string DiscoverClient = "discover";
    public const string LookupClient = "lookup";

    private const string ResultType = "area";

    public async Task<IEnumerable<ICityInfo>> GetCitiesFromLocationAsync(double latitude, double longitude,
        string language, int limit = 1)
    {
        var lat = CoordinateToString(latitude);
        var lng = CoordinateToString(longitude);
        var uri = $"revgeocode?at={lat},{lng}&types={ResultType}&limit={limit}&lang={language}";

        try
        {
            return await GetCitiesAsync(uri, RevgeocodeClient);
        }
        catch (Exception ex)
        {
            var message = "Error while getting cities from location. " +
                          $"Lat: {lat}; lng: {lng}; lang:{language}. Message: {ex.Message}";

            throw new MapsException(message, ex);
        }

        static string CoordinateToString(double coordinate) => coordinate.ToString(CultureInfo.InvariantCulture);
    }

    public async Task<IEnumerable<ICityInfo>> GetCitiesFromTextAsync(string text, string language, int limit = 20)
    {
        var uri = $"geocode?q={text}&types={ResultType}&limit={limit}&lang={language}";

        try
        {
            return await GetCitiesAsync(uri, DiscoverClient);
        }
        catch (Exception ex)
        {
            var message = $"Error while getting cities from text. " +
                          $"Text: {text}; lang:{language}. Message: {ex.Message}";

            throw new MapsException(message, ex);
        }
    }

    public async Task<ICityInfo> GetCityByIdAsync(string cityId, string language)
    {
        var uri = $"lookup?id={cityId}&lang={language}";

        try
        {
            using var client = clientFactory.CreateClient(LookupClient);
            using var response = await client.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var map = JsonSerializer.Deserialize<MapItem>(json);

            return GetCityInfo(map!);
        }
        catch (Exception ex)
        {
            var message = $"Error while getting city by Id. " +
                          $"Id: {cityId}; lang:{language}. Message: {ex.Message}";

            throw new MapsException(message, ex);
        }
    }

    private async Task<IEnumerable<ICityInfo>> GetCitiesAsync(string uri, string clientName)
    {
        using var client = clientFactory.CreateClient(clientName);
        using var response = await client.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var mapsResponse = JsonSerializer.Deserialize<MapResult>(json);

        return mapsResponse!.Items
            .Where(x => !string.IsNullOrWhiteSpace(x.LocalityType) && (x.IsCity() || x.IsDistrict()))
            .Select(GetCityInfo);
    }

    private static CityInfo GetCityInfo(MapItem map)
    {
        var city = map.IsDistrict() ? map.Address.District : map.Address.City;

        return new CityInfo
        {
            Id = map.Id,
            Title = map.Title,
            City = city,
            CountryCode = map.Address.CountryCode,
            PostalCode = map.Address.PostalCode,
            Latitude = map.Position.Lat,
            Longitude = map.Position.Lng
        };
    }
}