using CrushBot.Core.Entities;

namespace CrushBot.Core.Interfaces.Data.Repositories;

public interface ILikeRepository
{
    void UpdateUserLikes(ICollection<UserLike> existing, ICollection<UserLike> updated);
}