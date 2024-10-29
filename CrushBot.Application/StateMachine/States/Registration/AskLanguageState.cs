using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;

namespace CrushBot.Application.StateMachine.States.Registration;

public class AskLanguageState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<AskLanguageState> logger)
    : BaseLanguageState(client, localizer, logger)
{
    public override UserState State => UserState.AskLanguage;
}