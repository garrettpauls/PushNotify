﻿using System;
using System.Reflection;

using Windows.Web.Http;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PushNotify.Core.Services.Pushover;
using PushNotify.Core.Tests.Fakes;

namespace PushNotify.Core.Tests.Services
{
    [TestClass]
    public sealed class PushoverApiTests
    {
        private (PushoverApi, FakeHttpFilter) _CreateApi()
        {
            var filter = new FakeHttpFilter();
            var api = new PushoverApi(GetType().GetTypeInfo().Assembly.GetName(), filter);

            return (api, filter);
        }

        [TestMethod]
        public void LoginFailed()
        {
            var (service, filter) = _CreateApi();

            filter.ResponseFactory = request =>
            {
                request.RequestUri.Should().Be(new Uri("https://api.pushover.net/1/users/login.json"));
                var content = request.Content.Should().BeOfType<HttpFormUrlEncodedContent>().Which;
                content.Headers.Should().ContainKey("email").WhichValue.Should().Be("email@example.com");
                content.Headers.Should().ContainKey("password").WhichValue.Should().Be("pass");

                return new HttpResponseMessage(HttpStatusCode.Ok)
                {
                    Content = new HttpStringContent(@"{{
""status"":0
}}")
                };
            };

            var result = service.Login("email@example.com", "pass").Result;

            result.IfSome(secret => Assert.Fail($"Unexpected successful result: {secret}"));
        }

        [TestMethod]
        public void LoginSuccessful()
        {
            const string SECRET = "the secret";

            var (service, filter) = _CreateApi();

            filter.ResponseFactory = request =>
            {
                request.RequestUri.Should().Be(new Uri("https://api.pushover.net/1/users/login.json"));
                var content = request.Content.Should().BeOfType<HttpFormUrlEncodedContent>().Which;
                content.Headers.Should().ContainKey("email").WhichValue.Should().Be("email@example.com");
                content.Headers.Should().ContainKey("password").WhichValue.Should().Be("pass");

                return new HttpResponseMessage(HttpStatusCode.Ok)
                {
                    Content = new HttpStringContent($@"{{
""status"":1,
""request"":""7df577c3-da18-4fb3-898b-c1ab4985633b"",
""id"":""uQiRzpo4DXghDmr9QzzfQu27cmVRsG"",
""secret"":""{SECRET}""
}}")
                };
            };

            var result = service.Login("email@example.com", "pass").Result;

            result.Match(
                successful => successful.Should().Be(SECRET),
                () => Assert.Fail("Unsuccessful result"));
        }
    }
}
