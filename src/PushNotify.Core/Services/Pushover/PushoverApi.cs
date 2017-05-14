using System;
using System.Reflection;
using System.Threading.Tasks;

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

        Task<Either<string, RegisterDeviceErrors>> RegisterDevice(string secret, string deviceId);
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

        public Task<Option<string>> Login(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Either<string, RegisterDeviceErrors>> RegisterDevice(string secret, string deviceId)
        {
            throw new NotImplementedException();
        }
    }
}
