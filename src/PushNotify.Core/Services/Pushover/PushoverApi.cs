using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

using LanguageExt;

namespace PushNotify.Core.Services.Pushover
{
    public sealed class RegisterDeviceErrors
    {
    }

    public interface IPushoverApi
    {
        Task<Option<string>> Login(string email, string password);

        Task<Either<RegisterDeviceErrors, string>> RegisterDevice(string secret, string deviceId);
    }

    public sealed class PushoverApi : IPushoverApi
    {
        private readonly AssemblyName mAppInfo;
        private readonly IHttpFilter mFilter;

        public PushoverApi(AssemblyName appInfo, IHttpFilter filter = null)
        {
            mAppInfo = appInfo;
            mFilter = filter ?? new HttpBaseProtocolFilter();
        }

        private HttpClient _CreateClient()
        {
            var client = new HttpClient(mFilter);

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

                var result = JsonObject.Parse(content);
                if(result["status"].Stringify() != "1")
                {
                    return null;
                }

                var secret = result["secret"].GetString();

                return secret;
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
                if(!response.IsSuccessStatusCode)
                {
                    return new RegisterDeviceErrors();
                }

                var content = await response.Content.ReadAsStringAsync();

                var result = JsonObject.Parse(content);
                if(result["status"].Stringify() != "1")
                {
                    // TODO: nicely report errors
                    return new RegisterDeviceErrors();
                }

                var id = result["id"].GetString();

                return id;
            }
        }
    }
}
