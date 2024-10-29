namespace CrushBot.Core.Dto;

public interface ICityInfo
{
    public string Id { get; set; }

    public string Title { get; set; }

    public string City { get; set; }

    public string CountryCode { get; set; }

    public string PostalCode { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}