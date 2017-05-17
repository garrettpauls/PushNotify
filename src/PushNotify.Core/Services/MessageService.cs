using System.Linq;
using System.Threading.Tasks;

using PushNotify.Core.Models;
using PushNotify.Core.Services.Pushover;

namespace PushNotify.Core.Services
{
    public interface IMessageService
    {
        Task<IPushoverMessage[]> FetchNewMessages();
    }

    public sealed class MessageService : IMessageService
    {
        private readonly IConfigService mConfig;
        private readonly IPushoverApi mPushover;

        public MessageService(IPushoverApi pushover, IConfigService config)
        {
            mPushover = pushover;
            mConfig = config;
        }

        public async Task<IPushoverMessage[]> FetchNewMessages()
        {
            if(!mConfig.TryGetAuthentication(out PushoverAuth auth))
            {
                return new IPushoverMessage[0];
            }

            var messages = await mPushover.FetchMessages(auth.DeviceId, auth.Secret);

            if(messages.Any())
            {
                // TODO: cache messages

                await mPushover.UpdateHighestMessage(auth.DeviceId, messages.Max(msg => msg.Id));
            }

            return messages;
        }
    }
}
