using System;
using System.Linq;
using System.Reactive.Subjects;

using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Storage;

using LanguageExt;

using PushNotify.Core.Models;

namespace PushNotify.Core.Services
{
    public interface IConfigService
    {
        IObservable<Option<PushoverAuth>> Authentication { get; }

        void SetAuthentication(Option<PushoverAuth> auth);

        bool TryGetAuthentication(out PushoverAuth auth);
    }

    public sealed class ConfigService : IConfigService
    {
        private const string SETTING_DEVICE_ID = "DeviceId";
        private const string VAULT_RESOURCE = "Push Notify";
        private readonly BehaviorSubject<Option<PushoverAuth>> mAuthentication = new BehaviorSubject<Option<PushoverAuth>>(Option<PushoverAuth>.None);
        private readonly IPropertySet mSettings;
        private readonly PasswordVault mVault;

        public ConfigService()
        {
            mVault = new PasswordVault();
            mSettings = ApplicationData.Current.LocalSettings.Values;
        }

        public IObservable<Option<PushoverAuth>> Authentication => mAuthentication;

        public void Initialize()
        {
            if(TryGetAuthentication(out PushoverAuth auth))
            {
                mAuthentication.OnNext(auth);
            }
        }

        public void SetAuthentication(Option<PushoverAuth> auth)
        {
            auth.Match(
                value =>
                {
                    mSettings[SETTING_DEVICE_ID] = value.DeviceId;
                    mVault.Add(new PasswordCredential(VAULT_RESOURCE, value.DeviceId, value.Secret));
                },
                () =>
                {
                    mSettings.Remove(SETTING_DEVICE_ID);
                    var credentials = mVault.RetrieveAll().Where(cred => cred.Resource == VAULT_RESOURCE);
                    foreach(var credential in credentials)
                    {
                        mVault.Remove(credential);
                    }
                });
            mAuthentication.OnNext(auth);
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
