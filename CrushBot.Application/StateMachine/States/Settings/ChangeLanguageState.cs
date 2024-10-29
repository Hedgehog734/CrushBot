using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;

namespace CrushBot.Application.StateMachine.States.Settings;

public class ChangeLanguageState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<ChangeLanguageState> logger)
    : BaseLanguageState(client, localizer, logger)
{
    public override UserState State => UserState.ChangeLanguage;
}