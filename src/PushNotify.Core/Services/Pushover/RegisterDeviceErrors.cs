namespace PushNotify.Core.Services.Pushover
{
    public sealed class RegisterDeviceErrors
    {
        public RegisterDeviceErrors(string[] nameErrors)
        {
            NameErrors = nameErrors ?? new string[0];
        }

        public string[] NameErrors { get; }
    }
}
