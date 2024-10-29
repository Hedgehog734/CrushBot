using CrushBot.Core.Settings;

namespace CrushBot.Infrastructure.Settings;

public class AppSettings : IAppSettings
{
    public required string TelegramToken { get; set; }

    public int RetryThreshold { get; set; } = 60;

    public int RetryCount { get; set; } = 3;

    public required string MapsApiKey { get; set; }

    public int UserCacheLifetimeInMinutes { get; set; } = 15;

    public int CityCacheLifetimeInMinutes { get; set; } = 60;

    public required string PlaceholderImageId { get; set; }

    public required string BotLink { get; set; }

    public required Subscription Subscription { get; set; }
}