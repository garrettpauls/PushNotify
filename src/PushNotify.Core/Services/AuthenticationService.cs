using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Windows.Data.Json;
using Windows.Web.Http;

using LanguageExt;

using PushNotify.Core.Models;

namespace PushNotify.Core.Services
{
    public interface IAuthenticationService
    {
        bool TryGetCachedAuth(out PushoverAuth auth);

        Task<Option<PushoverAuth>> TryLogin(string email, string password, string deviceName);
    }

    public sealed class AuthenticationService : IAuthenticationService
    {
        private readonly AssemblyName mAppInfo;
        private readonly IConfigService mConfig;

        public AuthenticationService(IConfigService config, AssemblyName appInfo)
        {
            mConfig = config;
            mAppInfo = appInfo;
        }

        private HttpClient _CreateHttpClient()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.UserAgent.ParseAdd($"{mAppInfo.Name}/{mAppInfo.Version}");

            return client;
        }

        private async Task<Option<string>> _RegisterDevice(HttpClient client, string secret, string deviceName)
        {
            var payload = new HttpFormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = secret,
                ["name"] = deviceName,
                ["os"] = "O"
            });

            var response = await client.PostAsync(new Uri("https://api.pushover.net/1/devices.json"), payload);
            if(!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonObject.Parse(content);
            if(result["status"].Stringify() != "1")
            {
                // TODO: nicely report errors
                return null;
            }

            var id = result["id"].GetString();

            return id;
        }

        public bool TryGetCachedAuth(out PushoverAuth auth)
        {
            return mConfig.TryGetAuthentication(out auth);
        }

        public async Task<Option<PushoverAuth>> TryLogin(string email, string password, string deviceName)
        {
            using(var client = _CreateHttpClient())
            {
                var payload = new HttpFormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["email"] = email,
                    ["password"] = password
                });

                var response = await client.PostAsync(new Uri("https://api.pushover.net/1/users/login.json"), payload);
                if(!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                var result = JsonObject.Parse(content);
                if(result["status"].Stringify() != "1")
                {
                    return null;
                }

                var secret = result["secret"].GetString();

                var deviceId = await _RegisterDevice(client, secret, deviceName);

                var auth = deviceId.Map(id => new PushoverAuth(id, secret));

                auth.IfSome(x => mConfig.SetAuthentication(x));

                return auth;
            }
        }
    }
}
