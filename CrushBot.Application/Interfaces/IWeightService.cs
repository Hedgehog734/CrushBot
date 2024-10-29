using CrushBot.Application.Models;

namespace CrushBot.Application.Interfaces;

public interface IWeightService
{
    Task UpdateCurrentUserWeight(BotUserDto user);

    Task UpdateFeedUserWeight(BotUserDto user);
}