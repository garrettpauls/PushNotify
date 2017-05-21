using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Autofac;

using MetroLog;
using MetroLog.Targets;

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
        private ILogger mLogger;

        public App()
        {
            UnhandledException += _HandleException;

#if DEBUG
            LogManagerFactory.DefaultConfiguration.AddTarget(
                LogLevel.Trace, LogLevel.Fatal, new DebugTarget(new DebugLineLayout()));
#endif

            GlobalCrashHandler.Configure();
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
            mLogger?.Log(LogLevel.Error, e.Message);
            mLogger?.Log(LogLevel.Error, e.ToString());
        }

        [Conditional("DEBUG")]
        private void _InitializeDebug()
        {
            DebugSettings.IsBindingTracingEnabled = true;
        }

        private void _InitializeLogging()
        {
            mLogger = mContainer.Resolve<ILogger>(new TypedParameter(typeof(Type), typeof(App)));
        }

        public override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            mContainer = _BuildContainer();
            _InitializeLogging();
            _InitializeDebug();

            mContainer.Resolve<INotificationService>().Initialize();

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

                mLogger.Log(LogLevel.Trace, $"ResolveForPage: {page.GetType().FullName}->{vmType.FullName}");

                return (INavigable) mContainer.Resolve(vmType);
            }

            mLogger.Log(LogLevel.Trace, $"ResolveForPage: {page.GetType().FullName}->No ViewModel");
            return null;
        }
    }
}
