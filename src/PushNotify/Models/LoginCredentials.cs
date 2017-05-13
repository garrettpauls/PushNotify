using System.Text.RegularExpressions;

namespace PushNotify.Models
{
    public sealed class LoginCredentials : ValidatableModel<LoginCredentials>
    {
        public LoginCredentials()
        {
            Validator = DefaultValidator;
        }

        public string DeviceName
        {
            get => Read<string>();
            set => Write(value);
        }

        public string Email
        {
            get => Read<string>();
            set => Write(value);
        }

        public string Password
        {
            get => Read<string>();
            set => Write(value);
        }

        public static void DefaultValidator(LoginCredentials model)
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                model.Properties[nameof(Password)].Errors.Add("Password is required");
            }
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                model.Properties[nameof(Email)].Errors.Add("Email is required");
            }

            if (string.IsNullOrWhiteSpace(model.DeviceName))
            {
                model.Properties[nameof(DeviceName)].Errors.Add("Device Name is required");
            }
            else if (model.DeviceName.Length > 25)
            {
                model.Properties[nameof(DeviceName)].Errors.Add("Device Name cannot be longer than 25 characters");
            }
            else if (!Regex.IsMatch(model.DeviceName, "^[A-Za-z0-9_-]{1,25}$"))
            {
                model.Properties[nameof(DeviceName)].Errors
                    .Add("Device Name must only contain basic characters: A-Z, a-z, 0-9, _-");
            }
        }
    }
}
