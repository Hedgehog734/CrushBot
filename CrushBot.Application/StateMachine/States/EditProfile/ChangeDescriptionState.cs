using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;

namespace CrushBot.Application.StateMachine.States.EditProfile;

public class ChangeDescriptionState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<ChangeDescriptionState> logger)
    : BaseDescriptionState(client, localizer, logger)
{
    public override UserState State => UserState.ChangeDescription;
}