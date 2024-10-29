namespace CrushBot.Core.Settings;

public interface ISubscription
{
    public long ChannelId { get; set; }

    public string Management { get; set; }

    public string YearlyLink { get; set; }

    public double YearlyPrice { get; set; }

    public int YearlyDiscount { get; set; }

    public string MonthlyLink { get; set; }

    public double MonthlyPrice { get; set; }
}