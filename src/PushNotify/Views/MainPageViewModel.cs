using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Windows.UI.Xaml.Navigation;

using LanguageExt;

using PushNotify.Core.Models;
using PushNotify.Core.Services;
using PushNotify.Core.Services.Pushover;
using PushNotify.Core.Services.Pushover.Responses;

using Template10.Mvvm;

namespace PushNotify.Views
{
    public sealed class MainPageViewModel : ViewModelBase
    {
        private readonly IConfigService mConfigService;
        private readonly IMessageService mMessageService;
        private readonly IPushoverApi mPushover;

        public MainPageViewModel(IConfigService configService, IPushoverApi pushover, IMessageService messageService)
        {
            mConfigService = configService;
            mPushover = pushover;
            mMessageService = messageService;
            LogoutCommand = new DelegateCommand(_Logout);
        }

        public DelegateCommand LogoutCommand { get; }

        public ObservableCollection<MessageViewModel> Messages { get; } = new ObservableCollection<MessageViewModel>();

        private async void _Logout()
        {
            mConfigService.SetAuthentication(Option<PushoverAuth>.None);
            await NavigationService.NavigateAsync(typeof(LoginPage));
            NavigationService.ClearHistory();
        }

        private async Task _Refresh()
        {
            await mMessageService.FetchNewMessages();
            var messages = await mMessageService.GetCachedMessages();

            Messages.Clear();
            foreach(var message in messages.OrderByDescending(msg => msg.Date))
            {
                Messages.Add(new MessageViewModel(message));
            }
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            return _Refresh();
        }
    }

    public sealed class MessageViewModel : ViewModelBase
    {
        public MessageViewModel(PushoverMessage message)
        {
            Message = message;
        }

        public PushoverMessage Message { get; }
    }
}
