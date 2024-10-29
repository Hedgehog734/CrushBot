using CrushBot.Application.StateMachine.Context;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;

namespace CrushBot.Application.StateMachine.States.EditProfile;

public class ChangeChooseVideoState(
    UserContextProvider provider,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<ChangeChooseVideoState> logger)
    : BaseChooseVideoState(provider, client, localizer, logger)
{
    public override UserState State => UserState.ChangeChooseVideo;
}
