using System.Threading.Tasks;

using PushNotify.Core.Models;
using PushNotify.Core.Services;
using PushNotify.Core.Services.Pushover;
using PushNotify.Models;

using Template10.Mvvm;

namespace PushNotify.Views
{
    public sealed class LoginPageViewModel : ViewModelBase
    {
        private readonly IAuthenticationService mAuthService;
        private bool mIsLoggingIn;

        public LoginPageViewModel(IAuthenticationService authService)
        {
            mAuthService = authService;
            LoginCommand = new DelegateCommand(Login);
        }

        public LoginCredentials Credentials { get; } = new LoginCredentials();

        public bool IsLoggingIn
        {
            get => mIsLoggingIn;
            private set => Set(ref mIsLoggingIn, value);
        }

        public DelegateCommand LoginCommand { get; }

        private Task _LoginFailed(RegisterDeviceErrors errors)
        {
            if(errors.InvalidLogin)
            {
                Credentials.AddError(nameof(LoginCredentials.Email), "Login failed");
                Credentials.AddError(nameof(LoginCredentials.Password), "Login failed");
            }
            else
            {
                foreach(var error in errors.DeviceNameErrors)
                {
                    Credentials.AddError(nameof(LoginCredentials.DeviceName), error);
                }
            }

            return Task.CompletedTask;
        }

        private async Task _LoginSuccessful(PushoverAuth auth)
        {
            await NavigationService.NavigateAsync(typeof(MainPage));
            NavigationService.ClearHistory();
        }

        public async void Login()
        {
            IsLoggingIn = true;

            if(Credentials.Validate())
            {
                var result = await mAuthService.TryLogin(Credentials.Email, Credentials.Password,
                    Credentials.DeviceName);

                await result.Match(
                    _LoginSuccessful,
                    _LoginFailed);
            }

            IsLoggingIn = false;
        }
    }
}
