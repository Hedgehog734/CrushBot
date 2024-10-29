namespace CrushBot.Core.Entities;

public class UserLike
{
    public UserLike()
    {
    }

    public UserLike(long likerUserId, long likedUserId, bool value)
    {
        Value = value;
        LikerUserId = likerUserId;
        LikedUserId = likedUserId;
    }

    public long Id { get; set; }
    public DateTime Time { get; set; } = DateTime.UtcNow;
    public bool Value { get; set; }
    public bool MatchShown { get; set; }

    public long LikerUserId { get; set; }
    public long LikedUserId { get; set; }
}