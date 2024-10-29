using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;

namespace CrushBot.Application.StateMachine.States.EditFilters;

public class ChangeSexFilterState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<ChangeSexFilterState> logger)
    : BaseSexFilterState(client, localizer, logger)
{
    public override UserState State => UserState.ChangeSexFilter;
}