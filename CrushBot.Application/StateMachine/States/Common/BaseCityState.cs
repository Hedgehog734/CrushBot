using System.Net;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.Context;
using CrushBot.Core.Dto;
using CrushBot.Core.Entities;
using CrushBot.Core.Enums;
using CrushBot.Core.Exceptions;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Common;

public abstract class BaseCityState(
    IMapsService mapsService,
    ICityService cityService,
    UserContextProvider provider,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<BaseCityState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    public const string ContextDataKey = nameof(BaseCityState);

    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;

        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };
        var locationText = Localizer.GetDirected(language, Buttons.SendLocation);
        keyboard.AddNewRow(KeyboardButton.WithRequestLocation(locationText));

        if (!string.IsNullOrWhiteSpace(user.CityId))
        {
            keyboard.AddNewRow(GetCurrentButton(language));
        }

        var askText = Localizer.GetString(language, Messages.AskCity);
        await Client.SendMessageAsync(askText, message, cancellationToken, keyboard, ParseMode.MarkdownV2);
    }

    protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        if (message.Type == MessageType.Text)
        {
            return await HandleTextAsync(user, message, cancellationToken);
        }

        if (message.Type == MessageType.Location)
        {
            return await HandleLocationAsync(user, message, cancellationToken);
        }

        return StateTrigger.WrongMessageType;
    }

    protected override MessageType[] AllowedTypes { get; } = [MessageType.Text, MessageType.Location];

    private async Task<StateTrigger> HandleTextAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var currentText = GetCurrentButton(language);

            if (text.Equals(currentText, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(user.CityId))
            {
                return StateTrigger.DataEntered;
            }

            try
            {
                var langCode = LanguageHelper.GetCode(language);
                var cities = await mapsService.GetCitiesFromTextAsync(text, langCode);
                return await ProcessCitiesAsync(cities, user, message, cancellationToken, true);
            }
            catch (MapsException ex)
            {
                return await HandleMapsException(ex, language, message, cancellationToken);
            }
        }

        return await CityNotFoundResultAsync(language, message, cancellationToken);
    }

    private async Task<StateTrigger> HandleLocationAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var location = message.Location;

        if (location != null)
        {
            var language = user.Language;

            try
            {
                var langCode = LanguageHelper.GetCode(language);
                var cities = await mapsService.GetCitiesFromLocationAsync(location.Latitude, location.Longitude, langCode);
                return await ProcessCitiesAsync(cities, user, message, cancellationToken);
            }
            catch (MapsException ex)
            {
               return await HandleMapsException(ex, language, message, cancellationToken);
            }
        }

        return StateTrigger.WrongMessageType;
    }

    private async Task<StateTrigger> ProcessCitiesAsync(IEnumerable<ICityInfo> cities, BotUserDto user, Message message,
        CancellationToken cancellationToken, bool forceToChoose = false)
    {
        var cityList = cities.ToList();

        if (cityList.Count == 1 && !forceToChoose)
        {
            var city = cityList.Single();
            return await CityEnteredResultAsync(city, user, message, cancellationToken);
        }

        if (cityList.Count > 1 || (forceToChoose && cityList.Count > 0))
        {
            provider.SetData(user, ContextDataKey, cityList);
            return StateTrigger.DataProcessed;
        }

        return await CityNotFoundResultAsync(user.Language, message, cancellationToken);
    }

    private async Task<StateTrigger> CityEnteredResultAsync(ICityInfo cityInfo, BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var cityId = cityInfo.Id;

        var city = new City { Id = cityId, Latitude = cityInfo.Latitude, Longitude = cityInfo.Longitude };
        var cityName = new CityName { CityId = cityId, Name = cityInfo.City, Language = language };

        await cityService.AddCityWithCityNameAsync(city, cityName);
        user.CityId = cityId;

        var name = Markdown.Escape(cityName.Name).EnsureDirection(language);
        var text = Localizer.GetFormatted(language, Messages.CityEntered, name);

        await Client.SendMessageAsync(text, message, cancellationToken, parseMode: ParseMode.MarkdownV2);
        return StateTrigger.DataEntered;
    }

    private async Task<StateTrigger> CityNotFoundResultAsync(Language language, Message message,
        CancellationToken cancellationToken)
    {
        var text = Localizer.GetString(language, Messages.InvalidCity);
        await Client.ReplyMessageAsync(text, message, cancellationToken);
        return StateTrigger.DataNotFound;
    }

    private async Task<StateTrigger> ExternalErrorResultAsync(Language language, Message message,
        CancellationToken cancellationToken)
    {
        var text = Localizer.GetString(language, Messages.ExternalError);
        await Client.SendMessageAsync(text, message, cancellationToken);
        return StateTrigger.ExternalServiceError;
    }

    private async Task<StateTrigger> HandleMapsException(MapsException ex, Language language, Message message,
        CancellationToken cancellationToken)
    {
        if (ex.InnerException is HttpRequestException httpEx)
        {
            if (httpEx.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.MethodNotAllowed)
            {
                return await CityNotFoundResultAsync(language, message, cancellationToken);
            }

            if (httpEx.StatusCode is HttpStatusCode.ServiceUnavailable)
            {
                return await ExternalErrorResultAsync(language, message, cancellationToken);
            }
        }

        throw ex;
    }
}