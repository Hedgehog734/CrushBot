using CrushBot.Core.Enums;

namespace CrushBot.Core.Entities;

public class UserFilter
{
    public long Id { get; set; }
    public Sex? Sex { get; set; }
    public int? AgeAfter { get; set; }
    public int? AgeUntil { get; set; }

    public long UserId { get; set; }
    public BotUser User { get; set; } = null!;
}