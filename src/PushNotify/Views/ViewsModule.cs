using Autofac;

namespace PushNotify.Views
{
    public sealed class ViewsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainPageViewModel>().AsSelf().InstancePerDependency();
        }
    }
}