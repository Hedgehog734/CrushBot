using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace CrushBot.Application.StateMachine.States.Registration
{
    public class RegistrationState(
        ITelegramClient client,
        ILocalizer localizer,
        ILogger<RegistrationState> logger)
        : BaseState(client, localizer, logger)
    {
        protected override Task OnEnterCoreAsync(BotUserDto user, Message message,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
            CancellationToken cancellationToken)
        {
            var text = message.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(text) &&
                text.Equals(Commands.Start, StringComparison.OrdinalIgnoreCase))
            {
                return StateTrigger.DataEntered;
            }

            var langCode = message.From!.LanguageCode;
            var template = Localizer.GetString(user.Language, Messages.PressToStart, langCode);
            var startText = string.Format(template, Commands.Start.EnsureLtr());

            await Client.ReplyMessageAsync(startText, message, cancellationToken);
            return StateTrigger.InvalidData;
        }

        public override UserState State => UserState.Registration;
    }
}
