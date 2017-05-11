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

        Task<Option<PushoverAuth>> TryLogin(string email, string password);
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

        public bool TryGetCachedAuth(out PushoverAuth auth)
        {
            return mConfig.TryGetAuthentication(out auth);
        }

        public async Task<Option<PushoverAuth>> TryLogin(string email, string password)
        {
            await Task.Delay(2000);
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

                var id = result["id"].GetString();
                var secret = result["secret"].GetString();

                var auth = new PushoverAuth(id, secret);

                mConfig.SetAuthentication(auth);

                return auth;
            }
        }
    }
}
