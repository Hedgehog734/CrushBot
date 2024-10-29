using CrushBot.Application.Facades;
using CrushBot.Application.Facades.Interfaces;
using CrushBot.Application.Interfaces;
using CrushBot.Application.Localization;
using CrushBot.Application.Models;
using CrushBot.Application.Services;
using CrushBot.Application.Services.AgeService;
using CrushBot.Application.StateMachine;
using CrushBot.Application.StateMachine.Context;
using CrushBot.Application.StateMachine.Factories;
using CrushBot.Application.StateMachine.States.EditFilters;
using CrushBot.Application.StateMachine.States.EditProfile;
using CrushBot.Application.StateMachine.States.Profile;
using CrushBot.Application.StateMachine.States.Registration;
using CrushBot.Application.StateMachine.States.Settings;
using CrushBot.Application.StateMachine.States.ViewProfiles;
using CrushBot.Core;
using CrushBot.Core.Entities;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Interfaces.Cache;
using CrushBot.Core.Interfaces.Data;
using CrushBot.Core.Interfaces.Data.Repositories;
using CrushBot.Core.Interfaces.StateMachine;
using CrushBot.Core.Localization;
using CrushBot.Core.Settings;
using CrushBot.Infrastructure;
using CrushBot.Infrastructure.Data;
using CrushBot.Infrastructure.Data.Repositories;
using CrushBot.Infrastructure.Services.Cache;
using CrushBot.Infrastructure.Services.MapsService;
using CrushBot.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<AppSettings>().BindConfiguration("AppSettings");
builder.Services.AddOptions<Subscription>().BindConfiguration("AppSettings:Subscription");

builder.Services.AddScoped<IAppSettings>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<AppSettings>>().Value;
    return settings;
});

builder.Services.AddScoped<ISubscription>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<Subscription>>().Value;
    return settings;
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IUserMemoryCache<BotUserDto>, UserMemoryCache<BotUserDto>>();
builder.Services.AddSingleton<ICityMemoryCache<City>, CityMemoryCache<City>>();
builder.Services.AddSingleton<UserContextProvider>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFilterRepository, FilterRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICityNameRepository, CityNameRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IUserDistributedFacade, UserDistributedFacade>();
builder.Services.AddScoped<ICityDistributedFacade, CityDistributedFacade>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<ICityManager, CityManager>();
builder.Services.AddScoped<IStateMachineService, StateMachineService>();
builder.Services.AddScoped<IMapsService, MapsService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAgeService, AgeService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IWeightService, WeightService>();
builder.Services.AddScoped<IUserLookupService, UserLookupService>();

AddTelegramClient();

AddHttpClient("https://revgeocode.search.hereapi.com/v1/", MapsService.RevgeocodeClient);
AddHttpClient("https://discover.search.hereapi.com/v1/", MapsService.DiscoverClient);
AddHttpClient("https://lookup.search.hereapi.com/v1/", MapsService.LookupClient);

builder.Services.AddSingleton<ILocalizer, Localizer>(_ =>
    new Localizer("CrushBot.Application.Localization.Resources.Resources"));

builder.Services.AddScoped<IStateFactory, StateFactory>();
builder.Services.AddScoped<RegistrationState>();
builder.Services.AddScoped<AskLanguageState>();
builder.Services.AddScoped<AskNameState>();
builder.Services.AddScoped<AskAgeState>();
builder.Services.AddScoped<AskSexState>();
builder.Services.AddScoped<AskSexFilterState>();
builder.Services.AddScoped<AskCityState>();
builder.Services.AddScoped<ChooseCityState>();
builder.Services.AddScoped<AskMediaState>();
builder.Services.AddScoped<ChoosePhotoState>();
builder.Services.AddScoped<ChooseVideoState>();
builder.Services.AddScoped<AskDescriptionState>();
builder.Services.AddScoped<ProfileState>();
builder.Services.AddScoped<ViewProfilesState>();
builder.Services.AddScoped<EditProfileState>();
builder.Services.AddScoped<ChangeCityState>();
builder.Services.AddScoped<ChangeChooseCityState>();
builder.Services.AddScoped<ChangeMediaState>();
builder.Services.AddScoped<ChangeChoosePhotoState>();
builder.Services.AddScoped<ChangeChooseVideoState>();
builder.Services.AddScoped<ChangeDescriptionState>();
builder.Services.AddScoped<EditFiltersState>();
builder.Services.AddScoped<ChangeSexFilterState>();
builder.Services.AddScoped<ChangeAgeFilterState>();
builder.Services.AddScoped<SettingsState>();
builder.Services.AddScoped<SubscriptionState>();
builder.Services.AddScoped<ChangeLanguageState>();
builder.Services.AddScoped<DeleteProfileState>();

builder.Services.AddHostedService<PollingService>();

var host = builder.Build();
await host.RunAsync();

return;

void AddTelegramClient()
{
    builder.Services.AddHttpClient("telegram")
        .AddTypedClient<ITelegramClient>((httpClient, provider) =>
        {
            var settings = provider.GetRequiredService<IOptions<AppSettings>>().Value;

            var options = new TelegramBotClientOptions(settings.TelegramToken)
            {
                RetryThreshold = settings.RetryThreshold,
                RetryCount = settings.RetryCount
            };

            var baseClient = new TelegramBotClient(options, httpClient);
            return new TelegramClient(baseClient);
        });
}

void AddHttpClient(string uri, string name)
{
    builder.Services.AddHttpClient(name, client =>
    {
        client.BaseAddress = new Uri(uri);
    })
    .AddHttpMessageHandler(provider =>
    {
        var settings = provider.GetRequiredService<IOptions<AppSettings>>().Value;
        return new ApiKeyHandler(settings.MapsApiKey);
    });
}
