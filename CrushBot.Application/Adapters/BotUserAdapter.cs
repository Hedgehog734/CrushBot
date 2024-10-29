using CrushBot.Application.Models;
using CrushBot.Core.Entities;

namespace CrushBot.Application.Adapters;

public static class BotUserAdapter
{
    public static BotUserDto ToDto(this BotUser user)
    {
        return new BotUserDto
        {
            Id = user.Id,
            Language = user.Language,
            Name = user.Name,
            Age = user.Age,
            Sex = user.Sex,
            State = user.State,
            PhotoIds = user.PhotoIds,
            VideoId = user.VideoId,
            Description = user.Description,
            IsSubscribed = user.IsSubscribed,
            ShowEmoji = user.ShowEmoji,
            UpdateTimestamp = user.UpdateTimestamp,
            DaysVisited = user.DaysVisited,
            Weight = user.Weight,
            IsLowWeight = user.IsLowWeight,
            Likes = user.Likes,
            LikedBy = user.LikedBy,
            CityId = user.CityId,
            Filter = user.Filter
        };
    }

    public static BotUser ToEntity(this BotUserDto userDto)
    {
        return new BotUser
        {
            Id = userDto.Id,
            Language = userDto.Language,
            Name = userDto.Name,
            Age = userDto.Age,
            Sex = userDto.Sex,
            State = userDto.State,
            PhotoIds = userDto.PhotoIds,
            VideoId = userDto.VideoId,
            Description = userDto.Description,
            IsSubscribed = userDto.IsSubscribed,
            ShowEmoji = userDto.ShowEmoji,
            UpdateTimestamp = userDto.UpdateTimestamp,
            DaysVisited = userDto.DaysVisited,
            Weight = userDto.Weight,
            IsLowWeight = userDto.IsLowWeight,
            Likes = userDto.Likes,
            LikedBy = userDto.LikedBy,
            CityId = userDto.CityId,
            Filter = userDto.Filter
        };
    }
}