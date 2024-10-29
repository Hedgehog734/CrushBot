using CrushBot.Application.StateMachine.Context;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;

namespace CrushBot.Application.StateMachine.States.EditProfile;

public class ChangeChooseCityState(
    ICityService cityService,
    UserContextProvider provider,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<ChangeChooseCityState> logger)
    : BaseChooseCityState(cityService, provider, client, localizer, logger)
{
    public override UserState State => UserState.ChangeChooseCity;
}