using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;

using PushNotify.Views;

using Template10.Common;
using Template10.Services.NavigationService;

namespace PushNotify
{
    public sealed partial class App : BootStrapper
    {
        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            return NavigationService.NavigateAsync(typeof(MainPage));
        }

        public override INavigable ResolveForPage(Page page, NavigationService navigationService)
        {
            var vmType = Type.GetType(page.GetType().FullName + "ViewModel", false);

            if(vmType?.GetInterfaces().Contains(typeof(INavigable)) ?? false)
            {
                return (INavigable) Activator.CreateInstance(vmType);
            }

            return null;
        }
    }
}