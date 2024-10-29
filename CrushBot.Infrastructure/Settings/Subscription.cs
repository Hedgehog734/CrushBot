using CrushBot.Core.Settings;

namespace CrushBot.Infrastructure.Settings;

public class Subscription : ISubscription
{
    public required long ChannelId { get; set; }

    public required string Management { get; set; }

    public required string YearlyLink { get; set; }

    public required double YearlyPrice { get; set; }

    public required int YearlyDiscount { get; set; }

    public required string MonthlyLink { get; set; }

    public required double MonthlyPrice { get; set; }
}