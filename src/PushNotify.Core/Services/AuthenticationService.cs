using System.Threading.Tasks;

using LanguageExt;

using PushNotify.Core.Models;
using PushNotify.Core.Services.Pushover;

namespace PushNotify.Core.Services
{
    public interface IAuthenticationService
    {
        bool TryGetCachedAuth(out PushoverAuth auth);

        Task<Option<PushoverAuth>> TryLogin(string email, string password, string deviceName);
    }

    public sealed class AuthenticationService : IAuthenticationService
    {
        private readonly IConfigService mConfig;

        private readonly IPushoverApi mPushover;

        public AuthenticationService(IConfigService config, IPushoverApi pushover)
        {
            mConfig = config;
            mPushover = pushover;
        }

        public bool TryGetCachedAuth(out PushoverAuth auth)
        {
            return mConfig.TryGetAuthentication(out auth);
        }

        public async Task<Option<PushoverAuth>> TryLogin(string email, string password, string deviceName)
        {
            var maybeSecret = await mPushover.Login(email, password);
            return await maybeSecret.Match(
                RegisterDevice,
                () => Task.FromResult(Option<PushoverAuth>.None));

            async Task<Option<PushoverAuth>> RegisterDevice(string secret)
            {
                var result = await mPushover.RegisterDevice(secret, deviceName);

                return result.ToOption().Map(deviceId =>
                {
                    var auth = new PushoverAuth(deviceId, secret);
                    mConfig.SetAuthentication(auth);

                    return auth;
                });
            }
        }
    }
}
