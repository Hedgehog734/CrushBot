using System.Net;
using CrushBot.Application.Interfaces;
using CrushBot.Application.StateMachine.Extensions;
using CrushBot.Application.StateMachine.States.EditFilters;
using CrushBot.Application.StateMachine.States.EditProfile;
using CrushBot.Application.StateMachine.States.Profile;
using CrushBot.Application.StateMachine.States.Registration;
using CrushBot.Application.StateMachine.States.Settings;
using CrushBot.Application.StateMachine.States.ViewProfiles;
using CrushBot.Core.Enums;
using CrushBot.Core.Helpers;
using CrushBot.Core.Interfaces.StateMachine;
using Telegram.Bot.Types;

namespace CrushBot.Application.StateMachine;

public class StateMachineService(
    IUserManager facade,
    IStateFactory factory)
    : IStateMachineService
{
    private Stateless.StateMachine<IState, StateTrigger> _stateMachine = null!;

    public async Task ProcessMessageAsync(Message message, CancellationToken cancellationToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            var userId = message.From!.Id;
            var user = await facade.ReceiveCurrentUserAsync(userId);

            ConfigureStateMachine(user.GetState(factory));

            var currentState = _stateMachine.State;
            var stateTrigger = await currentState.HandleAsync(user, message, linkedCts.Token);
            await _stateMachine.FireAsync(stateTrigger);

            if (currentState.RefreshFromCache)
            {
                user = await facade.ReceiveCurrentUserAsync(userId);
            }

            var nextState = _stateMachine.State;
            var isEntered = await nextState.OnEnterAsync(user, message, linkedCts.Token);

            if (isEntered)
            {
                user.State = nextState.State;
            }

            await facade.UpdateCurrentUserAsync(user, !currentState.SaveUserToDatabase);
        }
        catch (Exception ex)
        {
            await linkedCts.CancelAsync();
            HandleException(ex);
        }
    }

    private void ConfigureStateMachine(IState initial)
    {
        _stateMachine = new Stateless.StateMachine<IState, StateTrigger>(initial);

        ConfigureRegistration();

        _stateMachine.Configure(factory.Create<ProfileState>())
            .Permit(StateTrigger.OptionOne, factory.Create<ViewProfilesState>())
            .Permit(StateTrigger.OptionTwo, factory.Create<ViewProfilesState>()) // todo implement matches state
            .Permit(StateTrigger.OptionThree, factory.Create<EditFiltersState>())
            .Permit(StateTrigger.OptionFour, factory.Create<SettingsState>())
            .Permit(StateTrigger.OptionFive, factory.Create<SubscriptionState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ViewProfilesState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<ProfileState>())
            .PermitReentry(StateTrigger.DataProcessed)
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        ConfigureFiltersEdit();
        ConfigureSettings();
    }

    private void ConfigureRegistration()
    {
        _stateMachine.Configure(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskLanguageState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<AskLanguageState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskNameState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<AskNameState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskAgeState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<AskAgeState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskSexState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<AskSexState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskSexFilterState>())
            .Permit(StateTrigger.DataNotFound, factory.Create<AskAgeState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<AskSexFilterState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskCityState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<AskCityState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskMediaState>())
            .Permit(StateTrigger.DataProcessed, factory.Create<ChooseCityState>())
            .PermitReentry(StateTrigger.DataNotFound)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.ExternalServiceError)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChooseCityState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskMediaState>())
            .Permit(StateTrigger.DataNotFound, factory.Create<AskCityState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<AskCityState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<AskMediaState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskDescriptionState>())
            .Permit(StateTrigger.OptionOne, factory.Create<ChoosePhotoState>())
            .Permit(StateTrigger.OptionTwo, factory.Create<ChooseVideoState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChoosePhotoState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskDescriptionState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<AskMediaState>())
            .PermitReentry(StateTrigger.DataProcessed)
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChooseVideoState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<AskDescriptionState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<AskMediaState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<AskDescriptionState>())
            .SubstateOf(factory.Create<RegistrationState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .Permit(StateTrigger.NextStep, factory.Create<ProfileState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);
    }

    private void ConfigureFiltersEdit()
    {
        _stateMachine.Configure(factory.Create<EditFiltersState>())
            .Permit(StateTrigger.OptionOne, factory.Create<ChangeAgeFilterState>())
            .Permit(StateTrigger.OptionTwo, factory.Create<ChangeSexFilterState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<ProfileState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChangeSexFilterState>())
            .SubstateOf(factory.Create<EditFiltersState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChangeAgeFilterState>())
            .SubstateOf(factory.Create<EditFiltersState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<EditFiltersState>())
            .Permit(StateTrigger.DataNotFound, factory.Create<AskNameState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);
    }

    private void ConfigureSettings()
    {
        _stateMachine.Configure(factory.Create<SettingsState>())
            .Permit(StateTrigger.OptionOne, factory.Create<EditProfileState>())
            .Permit(StateTrigger.OptionTwo, factory.Create<SubscriptionState>())
            .Permit(StateTrigger.OptionThree, factory.Create<ChangeLanguageState>())
            .Permit(StateTrigger.OptionFour, factory.Create<DeleteProfileState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<ProfileState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<SubscriptionState>())
            .SubstateOf(factory.Create<SettingsState>())
            .Permit(StateTrigger.OptionOne, factory.Create<SettingsState>())
            .Permit(StateTrigger.OptionTwo, factory.Create<ProfileState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChangeLanguageState>())
            .SubstateOf(factory.Create<SettingsState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<DeleteProfileState>())
            .SubstateOf(factory.Create<SettingsState>())
            .Permit(StateTrigger.DataEntered, factory.Create<RegistrationState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<SettingsState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        ConfigureProfileEdit();
    }

    private void ConfigureProfileEdit()
    {
        _stateMachine.Configure(factory.Create<EditProfileState>())
            .SubstateOf(factory.Create<SettingsState>())
            .Permit(StateTrigger.OptionOne, factory.Create<AskNameState>())
            .Permit(StateTrigger.OptionTwo, factory.Create<ChangeCityState>())
            .Permit(StateTrigger.OptionThree, factory.Create<ChangeMediaState>())
            .Permit(StateTrigger.OptionFour, factory.Create<ChangeDescriptionState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<SettingsState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChangeCityState>())
            .SubstateOf(factory.Create<EditProfileState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .Permit(StateTrigger.DataProcessed, factory.Create<ChangeChooseCityState>())
            .PermitReentry(StateTrigger.DataNotFound)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.ExternalServiceError)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChangeChooseCityState>())
            .SubstateOf(factory.Create<EditProfileState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .Permit(StateTrigger.DataNotFound, factory.Create<ChangeCityState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<ChangeCityState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChangeMediaState>())
            .SubstateOf(factory.Create<EditProfileState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .Permit(StateTrigger.OptionOne, factory.Create<ChangeChoosePhotoState>())
            .Permit(StateTrigger.OptionTwo, factory.Create<ChangeChooseVideoState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChangeChoosePhotoState>())
            .SubstateOf(factory.Create<EditProfileState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<ChangeMediaState>())
            .PermitReentry(StateTrigger.DataProcessed)
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChangeChooseVideoState>())
            .SubstateOf(factory.Create<EditProfileState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .Permit(StateTrigger.PreviousStep, factory.Create<ChangeMediaState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);

        _stateMachine.Configure(factory.Create<ChangeDescriptionState>())
            .SubstateOf(factory.Create<EditProfileState>())
            .Permit(StateTrigger.DataEntered, factory.Create<ProfileState>())
            .Permit(StateTrigger.NextStep, factory.Create<ProfileState>())
            .PermitReentry(StateTrigger.InvalidData)
            .PermitReentry(StateTrigger.WrongMessageType)
            .PermitReentry(StateTrigger.UnhandledError);
    }

    private static void HandleException(Exception ex)
    {
        if (ExceptionHelper.IsApiException(ex, out _, HttpStatusCode.Forbidden))
        {
            return;
        }

        throw ex;
    }
}