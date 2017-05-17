using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Windows.Foundation.Collections;
using Windows.Storage;

using LanguageExt;

using PushNotify.Core.Models;
using PushNotify.Core.Services.Pushover;
using PushNotify.Core.Services.Pushover.Responses;

namespace PushNotify.Core.Services
{
    public interface IMessageService
    {
        Task<PushoverMessage[]> FetchNewMessages();

        Task<PushoverMessage[]> GetCachedMessages();
    }

    public sealed class MessageService : IMessageService
    {
        private const int MAX_CACHE_SIZE = 15;
        private readonly IConfigService mConfig;
        private readonly IPropertySet mMessageContainer;
        private readonly IPushoverApi mPushover;

        public MessageService(IPushoverApi pushover, IConfigService config)
        {
            mPushover = pushover;
            mConfig = config;
            mMessageContainer = ApplicationData
                .Current.LocalSettings
                .CreateContainer("messages", ApplicationDataCreateDisposition.Always)
                .Values;
        }

        private void _AddToCache(PushoverMessage message)
        {
            var key = _GetKey(message.Id);
            if(!mMessageContainer.ContainsKey(key))
            {
                mMessageContainer.Add(key, Json.Write(message));
            }
        }

        private void _EnforceCacheSize()
        {
            if(mMessageContainer.Keys.Count > MAX_CACHE_SIZE)
            {
                var oldestKeys = mMessageContainer
                    .Keys
                    .Select(key => Prelude.parseInt(key))
                    .Somes()
                    .OrderBy(Prelude.identity)
                    .Select(_GetKey)
                    .Take(mMessageContainer.Keys.Count - MAX_CACHE_SIZE);

                foreach(var key in oldestKeys)
                {
                    mMessageContainer.Remove(key);
                }
            }
        }

        private string _GetKey(int messageId)
        {
            return messageId.ToString(CultureInfo.InvariantCulture);
        }

        public async Task<PushoverMessage[]> FetchNewMessages()
        {
            if(!mConfig.TryGetAuthentication(out PushoverAuth auth))
            {
                return new PushoverMessage[0];
            }

            var messages = await mPushover.FetchMessages(auth.DeviceId, auth.Secret);
            messages = messages.OrderByDescending(msg => msg.Date).ToArray();

            if(messages.Any())
            {
                foreach(var message in messages)
                {
                    _AddToCache(message);
                }

                _EnforceCacheSize();

                await mPushover.UpdateHighestMessage(auth, messages.Max(msg => msg.Id));
            }

            return messages;
        }

        public Task<PushoverMessage[]> GetCachedMessages()
        {
            var cachedMessages = mMessageContainer
                .Values
                .OfType<string>()
                .Select(Json.Read<PushoverMessage>)
                .Somes()
                .OrderByDescending(msg => msg.Date)
                .ToArray();

            return Task.FromResult(cachedMessages);
        }
    }
}
