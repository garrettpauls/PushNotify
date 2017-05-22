using System;

using Windows.UI.Notifications;

using MetroLog;

using Microsoft.Toolkit.Uwp.Notifications;

using PushNotify.Core.Services.Pushover.Responses;

namespace PushNotify.Core.Services
{
    public interface INotificationService
    {
        void Clear();

        void Show(PushoverMessage message);
    }

    public sealed class NotificationService : INotificationService
    {
        private readonly ILogger mLog;
        private const string NOTIFICATION_GROUP = "DefaultNotificationGroup";
        private readonly ToastNotifier mNotifier;

        public NotificationService(ILogger log)
        {
            mLog = log;
            mNotifier = ToastNotificationManager.CreateToastNotifier();
        }

        public void Clear()
        {
            ToastNotificationManager.History.Clear();
        }

        public void Show(PushoverMessage message)
        {
            var content = _CreateToastContent(message);

            var xml = content.GetXml();
            var toast = new ToastNotification(xml)
            {
                ExpirationTime = DateTimeOffset.Now.AddDays(2),
                Tag = message.Id.ToString(),
                Group = NOTIFICATION_GROUP
            };

            if(mLog.IsTraceEnabled)
            {
                mLog.Trace($"Show notification with xml: {xml.GetXml()}");
            }

            mNotifier.Show(toast);
        }

        private static ToastActionsCustom _CreateActions(PushoverMessage message)
        {
            var actions = new ToastActionsCustom
            {
            };
            return actions;
        }

        private static ToastAudio _CreateAudio(PushoverMessage message)
        {
            var audio = new ToastAudio();

            if(!string.IsNullOrEmpty(message.Sound))
            {
                var uri = $"https://api.pushover.net/sounds/{message.Sound}.mp3";
                audio.Src = new Uri(uri);
            }

            return audio;
        }

        private static ToastContent _CreateToastContent(PushoverMessage message)
        {
            var visual = _CreateVisual(message);
            var actions = _CreateActions(message);
            var audio = _CreateAudio(message);
            var content = new ToastContent
            {
                Visual = visual,
                Actions = actions,
                Audio = audio,
                Header = new ToastHeader(message.Id.ToString(), message.Title, "")
            };
            return content;
        }

        private static ToastVisual _CreateVisual(PushoverMessage message)
        {
            var iconUrl = $"https://api.pushover.net/icons/{message.Icon}.png";
            var visual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    AppLogoOverride = new ToastGenericAppLogo
                    {
                        Source = iconUrl
                    },
                    Children =
                    {
                        new AdaptiveText
                        {
                            Text = message.Title,
                            HintStyle = AdaptiveTextStyle.Title
                        },
                        new AdaptiveText
                        {
                            Text = message.Message,
                            HintStyle = AdaptiveTextStyle.Body
                        }
                    }
                }
            };
            return visual;
        }
    }
}
