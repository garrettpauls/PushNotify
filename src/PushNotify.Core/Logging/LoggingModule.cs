using System;
using System.Collections.Generic;
using System.Linq;

using Windows.Foundation.Diagnostics;

using Autofac;
using Autofac.Core;

namespace PushNotify.Core.Logging
{
    public sealed class LoggingModule : Module
    {
        private readonly ILoggingSession mLoggingSession = new LoggingSession("Default");
        private readonly ILoggingChannel mResolveTypeLogger;

        public LoggingModule()
        {
            mResolveTypeLogger = _ResolveLogger(typeof(LoggingModule), mLoggingSession);
        }

        private ILoggingChannel _BuildLogger(IComponentContext ctx, IEnumerable<Parameter> p)
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

            return _ResolveLogger(loggerName, ctx.Resolve<ILoggingSession[]>());
        }

        private void _HandleRegistrationPreparing(object sender, PreparingEventArgs e)
        {
            var limitType = e.Component.Activator.LimitType;

            if(mResolveTypeLogger.Enabled)
            {
                mResolveTypeLogger.LogMessage($"Resolving concrete type {limitType}", LoggingLevel.Verbose);
            }

            e.Parameters = e
                .Parameters
                .Union(new[]
                {
                    new ResolvedParameter(
                        (p, ctx) => p.ParameterType == typeof(ILoggingChannel),
                        (p, ctx) => _ResolveLogger(limitType, ctx.Resolve<ILoggingSession[]>()))
                });
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            registration.Preparing += _HandleRegistrationPreparing;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(mLoggingSession).As<ILoggingSession>().SingleInstance();
            builder.Register(_BuildLogger).As<ILoggingChannel>().InstancePerDependency();
        }

        private static ILoggingChannel _ResolveLogger(Type type, params ILoggingSession[] sessions)
        {
            return _ResolveLogger(type.FullName, sessions);
        }

        private static ILoggingChannel _ResolveLogger(string name, params ILoggingSession[] sessions)
        {
            var channel = new LoggingChannel(name, new LoggingChannelOptions());

            foreach(var session in sessions)
            {
                session.AddLoggingChannel(channel);
            }

            return channel;
        }
    }
}
