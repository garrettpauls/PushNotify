using System.Runtime.Serialization;

namespace PushNotify.Core.Services.Pushover.Responses
{
    [DataContract]
    public sealed class MessageListResponse : PushoverResponse
    {
        [DataMember(Name = "messages")]
        public PushoverMessage[] Messages { get; set; }
    }
}
