using CrushBot.Core.Enums;

namespace CrushBot.Core.Entities;

public class BotUser
{
    public long Id { get; set; }
    public Language Language { get; set; } = Language.None;
    public string? Name { get; set; }
    public int? Age { get; set; }
    public Sex? Sex { get; set; }
    public UserState State { get; set; } = UserState.Registration;
    public List<string> PhotoIds { get; set; } = new();
    public string? VideoId { get; set; }
    public string? Description { get; set; }
    public bool IsSubscribed { get; set; }
    public bool ShowEmoji { get; set; }
    public DateTime UpdateTimestamp { get; set; }
    public int DaysVisited { get; set; }
    public int Weight { get; set; }
    public bool IsLowWeight { get; set; }
    public ICollection<UserLike> Likes { get; set; } = [];
    public ICollection<UserLike> LikedBy { get; set; } = [];

    public string? CityId { get; set; }
    public City? City { get; set; }

    public UserFilter? Filter { get; set; } = new();
}