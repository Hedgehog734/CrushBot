using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;

namespace CrushBot.Application.StateMachine.States.Common;

public abstract class BaseWithButtonsState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<BaseWithButtonsState> logger)
    : BaseState(client, localizer, logger)
{
    protected string GetContinueButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Continue);
    }

    protected string GetCurrentButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Current);
    }

    protected string GetBackButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Back);
    }
}
