using Autofac;

using PushNotify.Core.Services.Pushover;

namespace PushNotify.Core.Services
{
    public sealed class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AuthenticationService>().As<IAuthenticationService>().SingleInstance();
            builder.RegisterType<ConfigService>().As<IConfigService>()
                .OnActivating(e => e.Instance.Initialize())
                .SingleInstance();
            builder.RegisterType<PushoverApi>().As<IPushoverApi>().SingleInstance();
            builder.RegisterType<MessageService>().As<IMessageService>().SingleInstance();
            builder.RegisterType<NotificationService>().As<INotificationService>().SingleInstance();
        }
    }
}
