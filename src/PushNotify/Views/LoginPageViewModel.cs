using System.Threading.Tasks;

using PushNotify.Core.Models;
using PushNotify.Core.Services;

using Template10.Mvvm;

namespace PushNotify.Views
{
    public sealed class LoginPageViewModel : ViewModelBase
    {
        private readonly IAuthenticationService mAuthService;
        private string mEmail;
        private bool mIsLoggingIn;
        private bool mLoginFailed;
        private string mPassword;

        public LoginPageViewModel(IAuthenticationService authService)
        {
            mAuthService = authService;
            LoginCommand = new DelegateCommand(Login);
        }

        public string Email
        {
            get => mEmail;
            set => Set(ref mEmail, value);
        }

        public bool IsLoggingIn
        {
            get => mIsLoggingIn;
            private set => Set(ref mIsLoggingIn, value);
        }

        public DelegateCommand LoginCommand { get; }

        public bool LoginFailed
        {
            get => mLoginFailed;
            private set => Set(ref mLoginFailed, value);
        }

        public string Password
        {
            get => mPassword;
            set => Set(ref mPassword, value);
        }

        private Task _LoginFailed()
        {
            LoginFailed = true;
            return Task.CompletedTask;
        }

        private async Task _LoginSuccessful(PushoverAuth auth)
        {
            LoginFailed = false;
            await NavigationService.NavigateAsync(typeof(MainPage));
            NavigationService.ClearHistory();
        }

        public async void Login()
        {
            IsLoggingIn = true;

            if(string.IsNullOrWhiteSpace(Email) || string.IsNullOrEmpty(Password))
            {
                await _LoginFailed();
            }
            else
            {
                var result = await mAuthService.TryLogin(Email, Password);

                await result.Match(
                    _LoginSuccessful,
                    _LoginFailed);
            }

            IsLoggingIn = false;
        }
    }
}
