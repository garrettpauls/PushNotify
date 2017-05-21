using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;
using Windows.Foundation.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Autofac;

using PushNotify.Core.Logging;
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
        private ILoggingChannel mLogger;

        public App()
        {
            UnhandledException += _HandleException;
        }

        private IContainer _BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<LoggingModule>();
            builder.RegisterModule<ViewsModule>();
            builder.RegisterModule<ServicesModule>();
            builder.RegisterInstance(typeof(App).GetTypeInfo().Assembly.GetName()).As<AssemblyName>().SingleInstance();

            return builder.Build();
        }

        private void _HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            mLogger?.LogMessage(e.Message, LoggingLevel.Error);
            mLogger?.LogMessage(e.ToString(), LoggingLevel.Error);

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
            mLogger = mContainer.Resolve<ILoggingChannel>(new TypedParameter(typeof(Type), typeof(App)));

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

                mLogger.LogMessage($"ResolveForPage: {page.GetType().FullName}->{vmType.FullName}");

                return (INavigable) mContainer.Resolve(vmType);
            }

            mLogger.LogMessage($"ResolveForPage: {page.GetType().FullName}->No ViewModel");
            return null;
        }
    }
}
