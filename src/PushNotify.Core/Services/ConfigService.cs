using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Storage;

using PushNotify.Core.Models;

namespace PushNotify.Core.Services
{
    public interface IConfigService
    {
        void SetAuthentication(PushoverAuth auth);

        bool TryGetAuthentication(out PushoverAuth auth);
    }

    public sealed class ConfigService : IConfigService
    {
        private const string SETTING_DEVICE_ID = "DeviceId";
        private const string VAULT_RESOURCE = "Push Notify";
        private readonly IPropertySet mSettings;
        private readonly PasswordVault mVault;

        public ConfigService()
        {
            mVault = new PasswordVault();
            mSettings = ApplicationData.Current.LocalSettings.Values;
        }

        public void SetAuthentication(PushoverAuth auth)
        {
            mSettings[SETTING_DEVICE_ID] = auth.DeviceId;
            mVault.Add(new PasswordCredential(VAULT_RESOURCE, auth.DeviceId, auth.Secret));
        }

        public bool TryGetAuthentication(out PushoverAuth auth)
        {
            auth = null;

            if(!mSettings.TryGetValue(SETTING_DEVICE_ID, out object deviceIdObj) || !(deviceIdObj is string))
            {
                return false;
            }

            var deviceId = deviceIdObj.ToString();
            var allCredentials = mVault.RetrieveAll();

            PasswordCredential credential = null;
            foreach(var cred in allCredentials)
            {
                if(cred.UserName == deviceId && cred.Resource == VAULT_RESOURCE)
                {
                    credential = cred;
                }
                else
                {
                    // clean up invalid credentials to not overload the vault
                    mVault.Remove(cred);
                }
            }

            if(credential == null)
            {
                return false;
            }

            credential.RetrievePassword();
            auth = new PushoverAuth(deviceId, credential.Password);

            return true;
        }
    }
}