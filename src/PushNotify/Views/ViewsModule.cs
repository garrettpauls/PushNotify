using System.Reflection;

using Autofac;

using Template10.Mvvm;

using Module = Autofac.Module;

namespace PushNotify.Views
{
    public sealed class ViewsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(ViewsModule).GetTypeInfo().Assembly)
                   .InNamespaceOf<ViewsModule>()
                   .AssignableTo<ViewModelBase>()
                   .AsSelf()
                   .InstancePerDependency();
        }
    }
}