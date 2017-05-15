namespace PushNotify.Core.Services.Pushover
{
    public sealed class RegisterDeviceErrors
    {
        public RegisterDeviceErrors(bool invalidLogin)
        {
            InvalidLogin = invalidLogin;
            DeviceNameErrors = new string[0];
        }

        public RegisterDeviceErrors(string[] nameErrors)
        {
            InvalidLogin = false;
            DeviceNameErrors = nameErrors ?? new string[0];
        }

        public string[] DeviceNameErrors { get; }

        public bool InvalidLogin { get; }
    }
}
