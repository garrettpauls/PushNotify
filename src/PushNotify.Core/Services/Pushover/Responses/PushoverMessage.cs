using System;
using System.Runtime.Serialization;

namespace PushNotify.Core.Services.Pushover.Responses
{
    [DataContract]
    public sealed class PushoverMessage : IPushoverMessage
    {
        DateTimeOffset IPushoverMessage.Date => DateTimeOffset.FromUnixTimeSeconds(UnixTimestamp);

        [DataMember(Name = "id")]
        public int Id { get; set; }

        bool IPushoverMessage.IsHtml => IsHtml == 1;

        [DataMember(Name = "html")]
        public int IsHtml { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "priority")]
        public PushoverMessagePriority Priority { get; set; }

        [DataMember(Name = "app")]
        public string SendingApp { get; set; }

        [DataMember(Name = "sound")]
        public string Sound { get; set; }

        [DataMember(Name = "url")]
        public string SupplementaryUrl { get; set; }

        [DataMember(Name = "url_title")]
        public string SupplementaryUrlTitle { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "umid")]
        public int UniqueMessageId { get; set; }

        [DataMember(Name = "date")]
        public long UnixTimestamp { get; set; }
    }
}
