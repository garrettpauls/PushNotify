using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Autofac;

using PushNotify.Core.Services;
using PushNotify.Framework.Xaml;
using PushNotify.Views;

using Template10.Common;
using Template10.Services.NavigationService;

namespace PushNotify
{
    public sealed partial class App : BootStrapper
    {
        private IContainer mContainer;

        public App()
        {
            UnhandledException += _HandleException;
        }

        private IContainer _BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<ViewsModule>();
            builder.RegisterModule<ServicesModule>();
            builder.RegisterInstance(typeof(App).GetTypeInfo().Assembly.GetName()).As<AssemblyName>().SingleInstance();

            return builder.Build();
        }

        private void _HandleException(object sender, UnhandledExceptionEventArgs e)
        {
#if DEBUG
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.Exception);
#endif
        }

        public override Task OnInitializeAsync(IActivatedEventArgs args)
        {
#if DEBUG
            DebugSettings.IsBindingTracingEnabled = true;
#endif

            mContainer = _BuildContainer();

            return base.OnInitializeAsync(args);
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            var authService = mContainer.Resolve<IAuthenticationService>();

            if(authService.TryGetCachedAuth(out _))
            {
                await NavigationService.NavigateAsync(typeof(MainPage));
            }
            else
            {
                await NavigationService.NavigateAsync(typeof(LoginPage));
            }
        }

        public override INavigable ResolveForPage(Page page, NavigationService navigationService)
        {
            var hasViewModel = page.GetType()
                                   .GetInterfaces()
                                   .FirstOrDefault(iface => iface.IsClosedTypeOf(typeof(IHasViewModel<>)));

            if(hasViewModel != null)
            {
                var vmType = hasViewModel.GenericTypeArguments[0];
                return (INavigable) mContainer.Resolve(vmType);
            }

            return null;
        }
    }
}
