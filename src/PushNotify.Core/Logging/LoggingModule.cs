using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using Autofac.Core;

using MetroLog;

namespace PushNotify.Core.Logging
{
    public sealed class LoggingModule : Module
    {
        private readonly ILogManager mLoggingSession;
        private readonly ILogger mResolveTypeLogger;

        public LoggingModule()
        {
            mLoggingSession = LogManagerFactory.DefaultLogManager;
            mResolveTypeLogger = _ResolveLogger(typeof(LoggingModule), mLoggingSession);
        }

        private ILogger _BuildLogger(IComponentContext ctx, IEnumerable<Parameter> p)
        {
            var parameters = p.ToArr();

            var loggerName =
                parameters
                    .OfType<TypedParameter>()
                    .Where(type => type.Type == typeof(string))
                    .Select(type => (string) type.Value)
                    .FirstOrDefault()
                ?? parameters
                    .OfType<TypedParameter>()
                    .Where(type => type.Type == typeof(Type))
                    .Select(type => ((Type) type.Value).FullName)
                    .FirstOrDefault()
                ?? "Unknown";

            return _ResolveLogger(loggerName, ctx.Resolve<ILogManager>());
        }

        private void _HandleRegistrationPreparing(object sender, PreparingEventArgs e)
        {
            var limitType = e.Component.Activator.LimitType;

            mResolveTypeLogger.Log(LogLevel.Trace, $"Resolving concrete type {limitType}");

            e.Parameters = e
                .Parameters
                .Union(new[]
                {
                    new ResolvedParameter(
                        (p, ctx) => p.ParameterType == typeof(ILogger),
                        (p, ctx) => _ResolveLogger(limitType, ctx.Resolve<ILogManager>()))
                });
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            registration.Preparing += _HandleRegistrationPreparing;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(mLoggingSession).As<ILogManager>().SingleInstance();
            builder.Register(_BuildLogger).As<ILogger>().InstancePerDependency();
        }

        private static ILogger _ResolveLogger(Type type, ILogManager manager)
        {
            return _ResolveLogger(type.FullName, manager);
        }

        private static ILogger _ResolveLogger(string name, ILogManager manager)
        {
            return manager.GetLogger(name);
        }
    }
}
