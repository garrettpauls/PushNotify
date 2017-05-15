using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Windows.UI.Xaml.Navigation;

using LanguageExt;

using PushNotify.Core.Models;
using PushNotify.Core.Services;
using PushNotify.Core.Services.Pushover;

using Template10.Mvvm;

namespace PushNotify.Views
{
    public sealed class MainPageViewModel : ViewModelBase
    {
        private readonly IConfigService mConfigService;
        private readonly IPushoverApi mPushover;

        public MainPageViewModel(IConfigService configService, IPushoverApi pushover)
        {
            mConfigService = configService;
            mPushover = pushover;
            LogoutCommand = new DelegateCommand(_Logout);
        }

        public DelegateCommand LogoutCommand { get; }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            return _Refresh();
        }

        private async Task _Refresh()
        {
            if(mConfigService.TryGetAuthentication(out PushoverAuth auth))
            {
                var messages = await mPushover.FetchMessages(auth.DeviceId, auth.Secret);
                Debug.WriteLine(messages);
            }
        }

        private async void _Logout()
        {
            mConfigService.SetAuthentication(Option<PushoverAuth>.None);
            await NavigationService.NavigateAsync(typeof(LoginPage));
            NavigationService.ClearHistory();
        }
    }
}
