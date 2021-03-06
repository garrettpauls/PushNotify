﻿using System;
using System.Linq;
using System.Reflection;
using Windows.Web.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PushNotify.Core.Services.Pushover;
using PushNotify.Core.Services.Pushover.Responses;
using PushNotify.Core.Tests.Fakes;

namespace PushNotify.Core.Tests.Services
{
    [TestClass]
    public sealed class PushoverApiTests
    {
        private readonly Uri mDevicesUri = new Uri("https://api.pushover.net/1/devices.json");
        private readonly Uri mLoginUri = new Uri("https://api.pushover.net/1/users/login.json");

        private (PushoverApi, FakeHttpFilter) _CreateApi()
        {
            var filter = new FakeHttpFilter();
            var api = new PushoverApi(GetType().GetTypeInfo().Assembly.GetName(), () => filter);

            return (api, filter);
        }

        [TestMethod]
        public void FetchMessagesSuccessful()
        {
            const string SECRET = "the-secret";
            const string DEVICE_ID = "the-id";

            var (service, filter) = _CreateApi();

            var response = new HttpResponseMessage(HttpStatusCode.Ok)
            {
                Content = new HttpStringContent(@"{""status"":1,
""request"":""36fddffb-9f62-444b-bd17-5e7c7febb258"",
""messages"":[
{""id"":1,""message"":""message 1"",""app"":""Pushover"",""aid"":1,""icon"":""HopmnR5uQ4cmXen"",""date"":1409605784,""priority"":0,""acked"":0,""umid"":1,""title"":""title 1""},
{""id"":2,""message"":""message 2"",""app"":""Another"",""aid"":1,""icon"":""default"",""date"":1409605795,""priority"":2,""acked"":0,""umid"":2,""title"":""""}
]}")
            };
            // the order of query parameters doesn't matter, so supply the response for either
            filter.Responses[new Uri($"https://api.pushover.net/1/messages.json?device_id={DEVICE_ID}&secret={SECRET}")]
                = response;
            filter.Responses[new Uri($"https://api.pushover.net/1/messages.json?secret={SECRET}&device_id={DEVICE_ID}")]
                = response;

            var messages = service.FetchMessages(DEVICE_ID, SECRET).Result;

            messages.Should().HaveCount(2);

            var target = messages.Single(msg => msg.Id == 1);
            target.Message.Should().Be("message 1");
            target.SendingApp.Should().Be("Pushover");
            target.Icon.Should().Be("HopmnR5uQ4cmXen");
            target.Date.Should().Be(new DateTimeOffset(2014, 9, 1, 21, 9, 44, 0, TimeSpan.Zero));
            target.Priority.Should().Be(PushoverMessagePriority.Normal);
            target.Title.Should().Be("title 1");

            target = messages.Single(msg => msg.Id == 2);
            target.Message.Should().Be("message 2");
            target.SendingApp.Should().Be("Another");
            target.Icon.Should().Be("default");
            target.Date.Should().Be(new DateTimeOffset(2014, 9, 1, 21, 9, 55, 0, TimeSpan.Zero));
            target.Priority.Should().Be(PushoverMessagePriority.Emergency);
            target.Title.Should().BeEmpty();
        }

        [TestMethod]
        public void LoginBadHttpStatusCode()
        {
            var (service, filter) = _CreateApi();

            filter.Responses[mLoginUri] = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new HttpStringContent(@"{
                    ""status"":0
                }")
            };

            var result = service.Login("email@example.com", "pass").Result;

            result.IfSome(secret => Assert.Fail($"Unexpected successful result: {secret}"));
        }

        [TestMethod]
        public void LoginFailed()
        {
            var (service, filter) = _CreateApi();

            filter.Responses[mLoginUri] = new HttpResponseMessage(HttpStatusCode.Ok)
            {
                Content = new HttpStringContent(@"{
""status"":0
}")
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
                request.RequestUri.Should().Be(mLoginUri);
                var content = request.Content.Should().BeOfType<HttpFormUrlEncodedContent>().Which;
                var actualContent = content.ReadAsStringAsync().GetResults();
                var unescapedContent = Uri.UnescapeDataString(actualContent);
                // TODO: update this so it isn't dependent on parameter order
                unescapedContent.Should().Be("email=email@example.com&password=pass");

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

        [TestMethod]
        public void RegisterDeviceFailedWithDuplicateName()
        {
            var (service, filter) = _CreateApi();

            filter.Responses[mDevicesUri] = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new HttpStringContent(@"{
                    ""status"":0,
                    ""errors"":{""name"":[""has already been taken""]}
                }")
            };

            var result = service.RegisterDevice("secret", "device").Result;

            result.Match(
                id => Assert.Fail($"Unexpected successful id: {id}"),
                err => { err.DeviceNameErrors.Should().HaveCount(1); });
        }

        [TestMethod]
        public void RegisterDeviceFailedWithUnknownErrors()
        {
            var (service, filter) = _CreateApi();

            filter.Responses[mDevicesUri] = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new HttpStringContent(@"{
                    ""status"":0
                }")
            };

            var result = service.RegisterDevice("secret", "device").Result;

            result.Match(
                id => Assert.Fail($"Unexpected successful id: {id}"),
                err => { err.DeviceNameErrors.Should().HaveCount(1); });
        }

        [TestMethod]
        public void RegisterDeviceSuccessful()
        {
            const string SECRET = "the-secret";
            const string DEVICE_NAME = "device-name";
            const string DEVICE_ID = "device-id";

            var (service, filter) = _CreateApi();

            filter.ResponseFactory = request =>
            {
                request.RequestUri.Should().Be(mDevicesUri);
                var content = request.Content.Should().BeOfType<HttpFormUrlEncodedContent>().Which;
                var actualContent = content.ReadAsStringAsync().GetResults();
                var unescapedContent = Uri.UnescapeDataString(actualContent);
                // TODO: update this so it isn't dependent on parameter order
                unescapedContent.Should().Be($"secret={SECRET}&name={DEVICE_NAME}&os=O");

                return new HttpResponseMessage(HttpStatusCode.Ok)
                {
                    Content = new HttpStringContent($@"
{{""status"":1,
""request"":""5e400a78-127b-4078-85e7-8f63cc78c9b2"",
""id"":""{DEVICE_ID}""
}}")
                };
            };

            var result = service.RegisterDevice(SECRET, DEVICE_NAME).Result;

            result.Match(
                success => success.Should().Be(DEVICE_ID),
                err => err.Should().BeNull("expected success"));
        }
    }
}
