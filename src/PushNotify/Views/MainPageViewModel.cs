using LanguageExt;

using PushNotify.Core.Models;
using PushNotify.Core.Services;

using Template10.Mvvm;

namespace PushNotify.Views
{
    public sealed class MainPageViewModel : ViewModelBase
    {
        private readonly IConfigService mConfigService;

        public MainPageViewModel(IConfigService configService)
        {
            mConfigService = configService;
            LogoutCommand = new DelegateCommand(_Logout);
        }

        public DelegateCommand LogoutCommand { get; }

        private async void _Logout()
        {
            mConfigService.SetAuthentication(Option<PushoverAuth>.None);
            await NavigationService.NavigateAsync(typeof(LoginPage));
            NavigationService.ClearHistory();
        }
    }
}
