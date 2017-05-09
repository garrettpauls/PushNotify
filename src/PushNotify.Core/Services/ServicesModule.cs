using Autofac;

namespace PushNotify.Core.Services
{
    public sealed class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AuthenticationService>().As<IAuthenticationService>().SingleInstance();
            builder.RegisterType<ConfigService>().As<IConfigService>().SingleInstance();
        }
    }
}