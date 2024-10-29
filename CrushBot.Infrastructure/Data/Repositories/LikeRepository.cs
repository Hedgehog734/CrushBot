using CrushBot.Core.Entities;
using CrushBot.Core.Interfaces.Data.Repositories;

namespace CrushBot.Infrastructure.Data.Repositories;

public class LikeRepository(AppDbContext context) : ILikeRepository
{
    public void UpdateUserLikes(ICollection<UserLike> existing, ICollection<UserLike> updated) // todo deleted profile
    {
        // todo sync
        var likesToAdd = updated.Where(upd => existing.All(ext => ext.Id != upd.Id)).ToList();
        var likesToRemove = existing.Where(ext => updated.All(upd => upd.Id != ext.Id)).ToList();

        foreach (var like in likesToAdd)
        {
            existing.Add(like);
        }

        foreach (var like in likesToRemove)
        {
            existing.Remove(like);
            context.Likes.Remove(like);
        }
    }
}