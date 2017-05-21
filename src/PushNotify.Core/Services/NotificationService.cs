using System;

using Windows.UI.Notifications;

using Microsoft.Toolkit.Uwp.Notifications;

namespace PushNotify.Core.Services
{
    public interface INotificationService
    {
        void Clear();

        void Show(string id, string title, string message);
    }

    public sealed class NotificationService : INotificationService
    {
        private const string NOTIFICATION_GROUP = "DefaultNotificationGroup";
        private readonly ToastNotifier mNotifier;

        public NotificationService()
        {
            mNotifier = ToastNotificationManager.CreateToastNotifier();
        }

        public void Clear()
        {
            ToastNotificationManager.History.Clear();
        }

        public void Show(string id, string title, string message)
        {
            var visual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    Children =
                    {
                        new AdaptiveText
                        {
                            Text = title,
                            HintStyle = AdaptiveTextStyle.Title
                        },
                        new AdaptiveText
                        {
                            Text = message,
                            HintStyle = AdaptiveTextStyle.Body
                        }
                    }
                }
            };
            var actions = new ToastActionsCustom
            {
            };
            var content = new ToastContent
            {
                Visual = visual,
                Actions = actions
            };

            var xml = content.GetXml();
            var toast = new ToastNotification(xml)
            {
                ExpirationTime = DateTimeOffset.Now.AddDays(2),
                Tag = id,
                Group = NOTIFICATION_GROUP
            };

            mNotifier.Show(toast);
        }
    }
}
