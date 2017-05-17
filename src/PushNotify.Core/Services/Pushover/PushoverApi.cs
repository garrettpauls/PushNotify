using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

using Windows.Web.Http;
using Windows.Web.Http.Filters;

using LanguageExt;

using PushNotify.Core.Models;
using PushNotify.Core.Services.Pushover.Responses;
using PushNotify.Core.Web;

namespace PushNotify.Core.Services.Pushover
{
    public interface IPushoverApi
    {
        Task<PushoverMessage[]> FetchMessages(string deviceId, string secret);

        Task<Option<string>> Login(string email, string password);

        Task<Either<RegisterDeviceErrors, string>> RegisterDevice(string secret, string deviceId);

        Task<bool> UpdateHighestMessage(PushoverAuth auth, int messageId);
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

        public async Task<PushoverMessage[]> FetchMessages(string deviceId, string secret)
        {
            using(var client = _CreateClient())
            {
                var uri = new UriQueryBuilder("https://api.pushover.net/1/messages.json")
                    .AddParameter("device_id", deviceId)
                    .AddParameter("secret", secret)
                    .ToUri();

                var response = await client.GetAsync(uri);

                if(!response.IsSuccessStatusCode)
                {
                    // TODO: better error reporting
                    return new PushoverMessage[0];
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = Json.Read<MessageListResponse>(content);

                return result.Match(
                    success => success.IsSuccessful ? success.Messages : new PushoverMessage[0],
                    () => new PushoverMessage[0]);
            }
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

                return result.Match(
                    success => success.IsSuccessful ? success.Secret : null,
                    () => null);
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
                    return RegisterDeviceErrors.Unknown;
                }

                return json.Match(GetResult, () => RegisterDeviceErrors.Unknown);

                Either<RegisterDeviceErrors, string> GetResult(RegisterDeviceResponse result)
                {
                    if(!result.IsSuccessful)
                    {
                        if(result.Errors == null || !result.Errors.TryGetValue("name", out string[] nameErrors))
                        {
                            nameErrors = new[] {"Unknown error registering device."};
                        }

                        return new RegisterDeviceErrors(nameErrors);
                    }

                    return result.Id;
                }
            }
        }

        public async Task<bool> UpdateHighestMessage(PushoverAuth auth, int messageId)
        {
            using(var client = _CreateClient())
            {
                var payload = new HttpFormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = auth.Secret,
                    ["message"] = messageId.ToString(CultureInfo.InvariantCulture)
                });

                var response = await client.PostAsync(new Uri($"https://api.pushover.net/1/devices/{auth.DeviceId}/update_highest_message.json"), payload);
                var content = await response.Content.ReadAsStringAsync();

                var json = Json.Read<PushoverResponse>(content);

                return json.Match(
                    success => success.IsSuccessful,
                    () => false);
            }
        }
    }
}
