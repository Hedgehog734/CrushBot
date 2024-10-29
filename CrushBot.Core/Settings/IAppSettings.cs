namespace CrushBot.Core.Settings;

public interface IAppSettings
{
    public string TelegramToken { get; set; }

    public int RetryThreshold { get; set; }

    public int RetryCount { get; set; }

    public string MapsApiKey { get; set; }

    public int UserCacheLifetimeInMinutes { get; set; }

    public int CityCacheLifetimeInMinutes { get; set; }

    public string PlaceholderImageId { get; set; }

    public string BotLink { get; set; }
}