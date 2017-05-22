using System;

namespace PushNotify.Core.Models
{
    public sealed class Config
    {
        public TimeSpan NotificationAgeThreshold { get; set; } = TimeSpan.FromMinutes(5);
    }
}
