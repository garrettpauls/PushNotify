namespace PushNotify.Core.Models
{
    public sealed class PushoverAuth
    {
        public PushoverAuth(string deviceId, string secret)
        {
            DeviceId = deviceId;
            Secret = secret;
        }

        public string DeviceId { get; }

        public string Secret { get; }
    }
}