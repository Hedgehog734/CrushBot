using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Settings;

public class DeleteProfileState(
    IUserManager userFacade,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<DeleteProfileState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;

        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };
        keyboard.AddNewRow(GetBackButton(language));

        var name = Markdown.Escape(user.Name!);
        var text = Localizer.GetFormatted(language, Messages.DeleteProfile, name);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard, ParseMode.MarkdownV2);
    }

    protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            if (text.Equals(user.Name, StringComparison.Ordinal))
            {
                await userFacade.RemoveUserAsync(user);

                var startText = Localizer.GetFormatted(language, Messages.PressToStart, Commands.Start.EnsureLtr());
                await Client.SendMessageAsync(startText, message, cancellationToken);
                return StateTrigger.DataEntered;
            }

            var backText = GetBackButton(language);

            if (text.Equals(backText, StringComparison.OrdinalIgnoreCase))
            {
                return StateTrigger.PreviousStep;
            }
        }

        return StateTrigger.InvalidData;
    }

    public override UserState State => UserState.DeleteProfile;

    public override bool RefreshFromCache => true;
}