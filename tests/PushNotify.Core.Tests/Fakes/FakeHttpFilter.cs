using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PushNotify.Core.Tests.Fakes
{
    public sealed class FakeHttpFilter : IHttpFilter
    {
        public Func<HttpRequestMessage, HttpResponseMessage> ResponseFactory { get; set; }

        public Dictionary<Uri, HttpResponseMessage> Responses { get; } = new Dictionary<Uri, HttpResponseMessage>();

        public void Dispose()
        {
        }

        public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
        {
            return AsyncInfo.Run<HttpResponseMessage, HttpProgress>((ct, progress) =>
            {
                var response = ResponseFactory?.Invoke(request);
                if(response == null && !Responses.TryGetValue(request.RequestUri, out response))
                {
                    Assert.Fail($"No response set up for {request.RequestUri}");
                }

                return Task.FromResult(response);
            });
        }
    }
}
