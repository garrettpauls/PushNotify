using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;

using Autofac;

using PushNotify.Core.Services;
using PushNotify.Views;

using Template10.Common;
using Template10.Services.NavigationService;

namespace PushNotify
{
    public sealed partial class App : BootStrapper
    {
        private IContainer mContainer;

        private IContainer _BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<ViewsModule>();
            builder.RegisterModule<ServicesModule>();

            return builder.Build();
        }

        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            mContainer = _BuildContainer();

            return NavigationService.NavigateAsync(typeof(MainPage));
        }

        public override INavigable ResolveForPage(Page page, NavigationService navigationService)
        {
            var vmType = Type.GetType(page.GetType().FullName + "ViewModel", false);

            if(vmType?.GetInterfaces().Contains(typeof(INavigable)) ?? false)
            {
                return (INavigable) mContainer.Resolve(vmType);
            }

            return null;
        }
    }
}