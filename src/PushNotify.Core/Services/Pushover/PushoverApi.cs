using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Windows.Web.Http;
using Windows.Web.Http.Filters;

using LanguageExt;

using PushNotify.Core.Services.Pushover.Responses;

namespace PushNotify.Core.Services.Pushover
{
    public interface IPushoverApi
    {
        Task<Option<string>> Login(string email, string password);

        Task<Either<RegisterDeviceErrors, string>> RegisterDevice(string secret, string deviceId);
    }

    public sealed class PushoverApi : IPushoverApi
    {
        private readonly AssemblyName mAppInfo;
        private readonly Func<IHttpFilter> mFilterFactory;

        public PushoverApi(AssemblyName appInfo, Func<IHttpFilter> filterFactory = null)
        {
            mAppInfo = appInfo;
            mFilterFactory = filterFactory ?? (() => new HttpBaseProtocolFilter());
        }

        private HttpClient _CreateClient()
        {
            var client = new HttpClient(mFilterFactory());

            client.DefaultRequestHeaders.UserAgent.ParseAdd($"{mAppInfo.Name}/{mAppInfo.Version}");

            return client;
        }

        public async Task<Option<string>> Login(string email, string password)
        {
            using(var client = _CreateClient())
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
                var result = Json.Read<LoginResponse>(content);

                if(!result.IsSuccessful)
                {
                    return null;
                }

                return result.Secret;
            }
        }

        public async Task<Either<RegisterDeviceErrors, string>> RegisterDevice(string secret, string deviceName)
        {
            using(var client = _CreateClient())
            {
                var payload = new HttpFormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secret,
                    ["name"] = deviceName,
                    ["os"] = "O"
                });

                var response = await client.PostAsync(new Uri("https://api.pushover.net/1/devices.json"), payload);
                var content = await response.Content.ReadAsStringAsync();

                var json = Json.Read<RegisterDeviceResponse>(content);
                if(json == null)
                {
                    return new RegisterDeviceErrors(new[] {"Unknown error registering device."});
                }

                if(!json.IsSuccessful)
                {
                    if(json.Errors == null || !json.Errors.TryGetValue("name", out string[] nameErrors))
                    {
                        nameErrors = new[] {"Unknown error registering device."};
                    }

                    return new RegisterDeviceErrors(nameErrors);
                }

                return json.Id;
            }
        }
    }
}
