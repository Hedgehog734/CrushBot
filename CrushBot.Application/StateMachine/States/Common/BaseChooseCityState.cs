using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.Context;
using CrushBot.Core.Dto;
using CrushBot.Core.Entities;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Common;

public abstract class BaseChooseCityState(
    ICityService cityService,
    UserContextProvider provider,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<BaseChooseCityState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var cities = provider.GetData<List<ICityInfo>>(user, BaseCityState.ContextDataKey);

        if (cities != null)
        {
            var language = user.Language;

            var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };
            cities.ForEach(x => keyboard.AddNewRow(GetCityButton(x, language)));
            keyboard.AddNewRow(GetBackButton(language));

            var text = Localizer.GetFormattedWithDigits(language, Messages.ChooseCity, cities.Count);
            await Client.SendMessageAsync(text, message, cancellationToken, keyboard);
        }
        else
        {
            await HandleCoreAsync(user, message, cancellationToken);
        }
    }

    protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var cities = provider.GetData<List<ICityInfo>>(user, BaseCityState.ContextDataKey);

        if (cities == null)
        {
            return StateTrigger.DataNotFound;
        }

        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var language = user.Language;
            var backText = GetBackButton(language);

            if (text.Equals(backText, StringComparison.OrdinalIgnoreCase))
            {
                return StateTrigger.PreviousStep;
            }

            var addresses = cities.ToDictionary(x => GetCityButton(x, language), x => x);

            if (addresses.TryGetValue(text, out var cityInfo))
            {
                var cityId = cityInfo.Id;

                var city = new City { Id = cityId, Latitude = cityInfo.Latitude, Longitude = cityInfo.Longitude };
                var cityName = new CityName { CityId = cityId, Name = cityInfo.City, Language = language };

                await cityService.AddCityWithCityNameAsync(city, cityName);
                user.CityId = cityId;

                var name = Markdown.Escape(cityName.Name).EnsureDirection(language);
                var enteredText = Localizer.GetFormatted(language, Messages.CityEntered, name);

                await Client.SendMessageAsync(enteredText, message, cancellationToken, parseMode: ParseMode.MarkdownV2);
                return StateTrigger.DataEntered;
            }
        }

        return StateTrigger.InvalidData;
    }

    private static string GetCityButton(ICityInfo city, Language language)
    {
        var postalCode = city.PostalCode;

        var result = string.IsNullOrWhiteSpace(postalCode)
            ? city.Title
            : string.Concat(city.Title, ", ", postalCode);

        return result.NormalizeCityTitle(language);
    }
}