using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;

namespace CrushBot.Application.StateMachine.States.Registration;

public class AskDescriptionState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<AskDescriptionState> logger)
    : BaseDescriptionState(client, localizer, logger)
{
    public override UserState State => UserState.AskDescription;

    public override bool SaveUserToDatabase => true;

    public override bool RefreshFromDb => true;
}